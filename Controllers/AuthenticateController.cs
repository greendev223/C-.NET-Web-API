using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Applications;
using Trading.Services.Authentication;
using Trading.Services.Groups;

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticateController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IGroupsService _groupsService;
        private readonly IApplicationsService _applicationsService;
        private readonly IMapper _mapper;
        private readonly ILogger _logger;

        public AuthenticateController(IAuthService authService,
            IGroupsService groupsService,
            IApplicationsService applicationsService,
            IMapper mapper,
            ILogger<AuthenticateController> logger)
        {
            _authService = authService;
            _groupsService = groupsService;
            _applicationsService = applicationsService;
            _mapper = mapper;
            _logger = logger;
        }


        /// <summary>
        /// Authenticate user username and password credentials. Returns JWT token on success.
        /// </summary>
        /// <param name="loginDto">Represents authentication data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(JwtSecurityTokenDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost]
        [Route("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var operationResult = await _authService.LoginAsync(loginDto);
                if (!operationResult.Succeeded)
                {
                    _logger.LogWarning($"{nameof(AuthenticateController.Login)}", operationResult);
                    return Unauthorized(operationResult);
                }

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(AuthenticateController.Login)}", e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Register user.
        /// </summary>
        /// <param name="registerDto">Represents body with user creadentials.</param>
        /// <param name="createGroup">Represents user creation type(with or without group). Default true.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto, bool createGroup = true)
        {
            try
            {
                var operationResult = await _authService.RegisterAsync(registerDto);
                if (!operationResult.Succeeded)
                {
                    _logger.LogWarning($"{nameof(AuthenticateController.Register)}", operationResult);
                    return StatusCode(StatusCodes.Status500InternalServerError, operationResult);
                }

                if (createGroup)
                {
                    try
                    {
                        var group = new Group()
                        {
                            Name = $"{registerDto.Email} Group"
                        };

                        var groupOperationResult = await _groupsService.CreateAsync(group).ConfigureAwait(false);
                        if (groupOperationResult.Succeeded)
                        {
                            await _groupsService.AddUserToGroupByEmailAsync(groupOperationResult.Value.Id, registerDto.Email, true).ConfigureAwait(false);
                            var defaultApplicationsResult = await _applicationsService.GetDefaultApplicationsAsync().ConfigureAwait(false);
                            if (defaultApplicationsResult != null && defaultApplicationsResult.Value != null && defaultApplicationsResult.Value.Count > 0)
                            {
                                foreach (var defaultApplication in defaultApplicationsResult.Value)
                                {
                                    var application = _mapper.Map<DefaultApplication, Application>(defaultApplication);
                                    application.Id = 0;
                                    application.GroupId = groupOperationResult.Value.Id;
                                    await _applicationsService.CreateAsync(application).ConfigureAwait(false);
                                }
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.LogError($"{nameof(AuthenticateController.Register)}", e.ToString());
                    }
                }

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(AuthenticateController.Register)}", e.ToString());
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}