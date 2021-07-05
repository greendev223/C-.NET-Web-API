using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using Trading.Data;
using Trading.Data.Entities;
using Trading.Services.ApplicationFiles;
using Trading.Services.Applications;
using Trading.Services.Authentication;
using Trading.Services.Groups;
using Trading.Services.Terminals;
using Trading.Services.Users;
using Trading.Services.VirtualMachines;
using Trading.WebApi.Configurations;
using Trading.WebApi.SignalR.Hubs;

namespace Trading.WebApi
{
    public class Startup
    {
        readonly string AllowAllOrigins = "_AllowAllOrigins";
        readonly string AllowDevEnvirement = "_AllowDevEnvirement";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("ConnectionString")));

            // For Identity  
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            // Adding Authentication  
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
            })

            // Adding Jwt Bearer  
            .AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidAudience = Configuration["JWT:ValidAudience"],
                    ValidIssuer = Configuration["JWT:ValidIssuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Secret"]))
                };
            });

            services.AddSwaggerGen(swagger =>
            {
                //This is to generate the Default UI of Swagger Documentation    
                swagger.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Trading platform",
                    Description = "Trading platform swagger documentation"
                });

                // To Enable authorization using Swagger (JWT)    
                swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer' [space] and then your valid token in the text input below.\r\n\r\nExample: \"Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9\"",
                });

                swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                          new OpenApiSecurityScheme
                            {
                                Reference = new OpenApiReference
                                {
                                    Type = ReferenceType.SecurityScheme,
                                    Id = "Bearer"
                                }
                            },
                            new string[] {}
                    }
                });

                // Set the comments path for the Swagger JSON and UI.**
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                swagger.IncludeXmlComments(xmlPath);

                // Upload files with swagger.
                swagger.OperationFilter<SwaggerFileOperationFilter>();
            });

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IGroupsService, GroupsService>();
            services.AddScoped<IVirtualMachinesService, VirtualMachinesService>();
            services.AddScoped<IUsersService, UsersService>();
            services.AddScoped<IApplicationsService, ApplicationsService>();
            services.AddScoped<IApplicationFilesService, ApplicationFilesService>();
            services.AddScoped<ITerminalsService, TerminalsService>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddAutoMapper(typeof(Startup));

            services.AddSignalR(options =>
            {
                options.MaximumParallelInvocationsPerClient = 20;
                options.MaximumReceiveMessageSize = 20000000;
            });

            services.AddMemoryCache();

            services.AddCors(options =>
            {
                options.AddPolicy(AllowAllOrigins,
                    builder =>
                    {
                        builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                    });

                options.AddPolicy(AllowDevEnvirement,
                    builder =>
                    {
                        builder.WithOrigins(
                            "http://204.44.125.96:8080",
                            "http://64.188.17.102",
                            "https://localhost:5001",
                            "http://localhost:8080",
                            "http://auvoria.cloudtrader.io",
                            "https://localhost:44315",
                            "http://localhost:61297",
                            "http://localhost",
                            "http://78.137.58.34:100",
                            "http://78.137.6.56:100",
                            "http://104.223.118.53",
                            "https://104.223.118.53",
                            "http://panel.cloudtrader.io",
                            "https://panel.cloudtrader.io")
                            .AllowAnyHeader()
                            .WithMethods("GET", "POST", "PUT", "DELETE", "OPTIONS")
                            .AllowCredentials();
                    });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Trading v1"));
            app.UseDeveloperExceptionPage();

            //app.UseHttpsRedirection();

            app.UseRouting();

            app.UseCors(AllowDevEnvirement);

            //app.UseCors(x => x
            //    .AllowAnyMethod()
            //    .AllowAnyHeader()
            //    .SetIsOriginAllowed(origin => true) // allow any origin
            //    .AllowCredentials()); // allow credentials

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                //endpoints.MapHub<VirtualMachinesHub>("/VirtualMachinesHub");
                endpoints.MapHub<VirtualMachinesHub>("/VirtualMachinesHub",
                    configureOptions =>
                    {
                        configureOptions.ApplicationMaxBufferSize = 20000000;
                        configureOptions.TransportMaxBufferSize = 20000000;
                    });
            });
        }
    }
}