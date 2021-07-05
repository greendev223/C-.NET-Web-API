using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Applications;
using Trading.Services.Authentication;
using Trading.Services.Terminals;
using Trading.Shared.Results;

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SystemController : ControllerBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuthService _authService;
        private readonly ITerminalsService _terminalsService;
        private readonly IApplicationsService _applicationsService;
        private readonly ILogger _logger;
        private readonly IMemoryCache _cache;
        private readonly IHostingEnvironment _hostingEnvironment;

        public SystemController(IAuthService authService,
            ITerminalsService terminalsService,
            IApplicationsService applicationsService,
            IMemoryCache cache,
            ILogger<SystemController> logger,
            IHttpContextAccessor httpContextAccessor,
            IHostingEnvironment hostingEnvironment)
        {
            _authService = authService;
            _terminalsService = terminalsService;
            _applicationsService = applicationsService;
            _cache = cache;
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _hostingEnvironment = hostingEnvironment;
        }

        /// <summary>
        /// Returns all default applications that are predefined on the file system.
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetDefaultApplicationPaths")]
        public async Task<IActionResult> GetDefaultApplicationPathsAsync()
        {
            string[] filePaths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Applications"));

            return Ok(filePaths);
        }


        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("seed/{secretKey}")]
        public async Task<IActionResult> Seed(string secretKey)
        {
            try
            {
                if (_cache.TryGetValue("temporarySecretKey", out string temporarySecretKey))
                {
                    if (secretKey != temporarySecretKey)
                    {
                        return NotFound();
                    }
                }
                else
                {
                    return NotFound();
                }

                var adminRoleResult = await _authService.CreateRoleAsync("Admin");
                var userRoleResult = await _authService.CreateRoleAsync("User");

                var adminResult = await _authService.RegisterAsync(new RegisterDto
                {
                    Email = "AdminDemo@CloudTrader.io",
                    Password = "Demoap843##"
                }, "Admin");

                var defaultTerminals = new List<DefaultTerminal>
                {
                    new DefaultTerminal {

                        FullLocalPath = "C:\\Users\\Administrator\\AppData\\Roaming\\MetaQuotes\\Terminal\\50CA3DFB510CC5A8F28B48D1BF2A5702",
                        Name = "Terminal #1",
                        ShortCutFullPath = "C:\\Users\\Administrator\\Desktop\\Terminal #1.lnk",
                        Description = "Default Terminal #1"
                    },
                    new DefaultTerminal
                    {
                        FullLocalPath = "C:\\Users\\Administrator\\AppData\\Roaming\\MetaQuotes\\Terminal\\C6D03BEE984A8FF7763AA4060BA5C4AC",
                        Name = "Terminal #2",
                        ShortCutFullPath = "C:\\Users\\Administrator\\Desktop\\Terminal #2.lnk",
                        Description = "Default Terminal #2"
                    },
                    new DefaultTerminal
                    {
                        FullLocalPath = "C:\\Users\\Administrator\\AppData\\Roaming\\MetaQuotes\\Terminal\\ECCC797A06C091715C9987E98B809476",
                        Name = "Terminal #3",
                        ShortCutFullPath = "C:\\Users\\Administrator\\Desktop\\Terminal #3.lnk",
                    },
                    //
                    new DefaultTerminal
                    {
                        FullLocalPath = "C:\\Users\\Administrator\\AppData\\Roaming\\MetaQuotes\\Terminal\\01ECB1DDA67EAD0AF666EEF79492AD67",
                        Name = "Terminal #4",
                        ShortCutFullPath = "C:\\Users\\Administrator\\Desktop\\Terminal #4.lnk",
                    }
                };

                var defaultExistanceTerminalsResult = await _terminalsService.GetDefaultTerminalsAsync().ConfigureAwait(false);
                if (defaultExistanceTerminalsResult != null && defaultExistanceTerminalsResult.Value != null && defaultExistanceTerminalsResult.Value.Count > 0)
                {
                    foreach (var defaultTerminal in defaultTerminals.Where(x => !defaultExistanceTerminalsResult.Value.Select(q => q.FullLocalPath).Contains(x.FullLocalPath)).ToList())
                    {
                        await _terminalsService.CreateDefaultTerminalAsync(defaultTerminal).ConfigureAwait(false);
                    }
                }
                else
                {
                    foreach (var defaultTerminal in defaultTerminals)
                    {
                        await _terminalsService.CreateDefaultTerminalAsync(defaultTerminal).ConfigureAwait(false);
                    }
                }

                var defaultApplications = new List<DefaultApplication>();
                var defaultAppPaths = Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, "Applications"));
                foreach (var appPath in defaultAppPaths)
                {
                    var fileInfo = new System.IO.FileInfo(appPath);
                    var fileBytes = System.IO.File.ReadAllBytes(appPath);
                    var app = new DefaultApplication 
                    {
                        Name = fileInfo.Name.Split(".")[0],
                        UrlToApplicationOnFtpServer = $"ftp://204.44.125.88/{fileInfo.Name}",
                        IsEnabled = true,
                        Level = ApplicationLevel.Production
                    };

                    var appFilesResult = await _applicationsService.ParseApplicationSkeletonAsync(fileBytes).ConfigureAwait(false);
                    if (!appFilesResult.Succeeded)
                    {
                        break;
                    }

                    app.Files = appFilesResult.Value.Select(x => new DefaultApplicationFile 
                    {
                        FileNameInTerminal = x.FileNameInTerminal,
                        AbsoluteSourcePath = x.AbsoluteSourcePath,
                        AbsolutePathInTerminal = x.AbsolutePathInTerminal,
                        DirectoryPathInTerminal = x.DirectoryPathInTerminal
                    }).ToList();

                    defaultApplications.Add(app);
                }

                #region comment
                //var defaultApplications = new List<DefaultApplication>()
                //{
                //    new DefaultApplication
                //    {
                //               Name = "ServerFront",
                //               IsEnabled = true,
                //               Level = ApplicationLevel.Production,
                //               UrlToApplicationOnFtpServer = "ftp://auvoria.cloudtrader.io/ServerFront.zip",
                //               Files = new List<DefaultApplicationFile>{
                //                  new DefaultApplicationFile{
                //                     AbsoluteSourcePath = "ServerFront.ex4",
                //                     AbsolutePathInTerminal = "MQL4/Experts/ServerFront.ex4"
                //                  },
                //                  new DefaultApplicationFile{
                //                     AbsoluteSourcePath = "ServerFront.dll",
                //                     AbsolutePathInTerminal = "MQL4/Libraries/ServerFront.dll"
                //                  }
                //               }
                //    },

                //    new DefaultApplication
                //    {
                //       Name = "FXTrader",
                //       IsEnabled = true,
                //       Level = ApplicationLevel.Production,
                //       UrlToApplicationOnFtpServer = "ftp://auvoria.cloudtrader.io/FXTrader.zip",
                //       Files = new List<DefaultApplicationFile>{
                //          new DefaultApplicationFile{
                //             AbsoluteSourcePath = "FXTrader.ex4",
                //             AbsolutePathInTerminal = "MQL4/Experts/FXTrader.ex4"
                //          },
                //          new DefaultApplicationFile{
                //             AbsoluteSourcePath = "FxTrader.dll",
                //             AbsolutePathInTerminal = "MQL4/Libraries/FxTrader.dll"
                //          }
                //       }
                //    }
                //};
                #endregion

                if (defaultApplications != null && defaultApplications.Count > 0)
                {
                    var defaultExistanceApplicationsResult = await _applicationsService.GetDefaultApplicationsAsync().ConfigureAwait(false);
                    if (defaultExistanceApplicationsResult != null && defaultExistanceApplicationsResult.Value != null && defaultExistanceApplicationsResult.Value.Count > 0)
                    {
                        foreach (var defaultApplication in defaultApplications.Where(x => !defaultExistanceApplicationsResult.Value.Select(q => q.Name).Contains(x.Name)).ToList())
                        {
                            await _applicationsService.CreateDefaultApplicationAsync(defaultApplication).ConfigureAwait(false);
                        }
                    }
                    else
                    {
                        foreach (var defaultApplication in defaultApplications)
                        {
                            await _applicationsService.CreateDefaultApplicationAsync(defaultApplication).ConfigureAwait(false);
                        }
                    }
                }

                return NotFound();
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(SystemController.Seed)}", e);
                return NotFound();
            }
        }

        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpGet]
        [Route("GetOneTimeSecretKey")]
        public async Task<IActionResult> GetOneTimeSecretKey()
        {
            var secret = Guid.NewGuid();
            _cache.Set("temporarySecretKey", secret.ToString(), TimeSpan.FromSeconds(15));

            return Ok(secret);
        }

        /// <summary>
        /// Returns ip address of the http client.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Route("GetMyIp")]
        public async Task<IActionResult> GetMyIp()
        {
            return Ok(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress.ToString());
        }

        [HttpGet]
        [Route("StatusCodes")]
        public async Task<IActionResult> StautsCodes()
        {
            var enumNames = Enum.GetNames(typeof(ServiceResponseStatusCode));
            var dictionary = new Dictionary<int, string>();
            foreach (var eName in enumNames)
            {
                dictionary[(int)Enum.Parse(typeof(ServiceResponseStatusCode), eName)] = eName;
            }

            return Ok(dictionary);
        }
    }
}