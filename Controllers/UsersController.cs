using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Users;
using Trading.Shared.Results;

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUsersService _usersService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly UserManager<ApplicationUser> _userManager;
        public UsersController(IUsersService usersService, ILogger<UsersController> logger, UserManager<ApplicationUser> userManager, IMapper mapper)
        {
            _usersService = usersService;
            _logger = logger;
            _mapper = mapper;
            _userManager = userManager;
        }

        /// <summary>
        /// Returns all users.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try {
                var operationResult = await _usersService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<ApplicationUser>>, OperationResult<List<UserDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(UsersController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns user by email.
        /// </summary>
        /// <param name="email">Represnts email address.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetByEmail/{email}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetByEmail(string email)
        {
            try
            {
                var operationResult = await _usersService.FindByEmailAsync(email).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<ApplicationUser>, OperationResult<UserDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(UsersController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns user by id.
        /// </summary>
        /// <param name="id">Represents user identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetById/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetById(string id)
        {
            try
            {
                var operationResult = await _usersService.GetByIdAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<ApplicationUser>, OperationResult<UserDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(UsersController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns system information for current authenticated user.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(UserExtendedDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("Me")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Me()
        {
            try
            {
                var result = await _usersService.GetUserInfo(User.Identity.Name).ConfigureAwait(false);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(UsersController.Me)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
