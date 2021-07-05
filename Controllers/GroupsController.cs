using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Applications;
using Trading.Services.Groups;
using Trading.Services.Users;
using Trading.Shared.Results;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class GroupsController : ControllerBase
    {
        private readonly IGroupsService _groupsService;
        private readonly IApplicationsService _applicationsService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUsersService _usersService;
        public GroupsController(IGroupsService groupsService,
            IApplicationsService applicationsService,
            IUsersService usersService,
            ILogger<GroupsController> logger,
            IMapper mapper)
        {
            _groupsService = groupsService;
            _applicationsService = applicationsService;
            _logger = logger;
            _mapper = mapper;
            _usersService = usersService;
        }

        /// <summary>
        /// Returns all groups.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var operationResult = await _groupsService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Group>>, OperationResult<List<GroupDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns group by id.
        /// </summary>
        /// <param name="id">Represnts group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == id))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.GetAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Group>, OperationResult<GroupDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Create group.
        /// </summary>
        /// <param name="groupDto">Represents group data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] GroupDto groupDto)
        {
            try
            {
                var group = new Group()
                {
                    Name = groupDto.Name
                };

                var operationResult = await _groupsService.CreateAsync(group).ConfigureAwait(false);
                if (operationResult.Succeeded)
                {
                    var defaultApplicationsResult = await _applicationsService.GetDefaultApplicationsAsync().ConfigureAwait(false);
                    if (defaultApplicationsResult != null && defaultApplicationsResult.Value != null && defaultApplicationsResult.Value.Count > 0)
                    {
                        foreach (var defaultApplication in defaultApplicationsResult.Value)
                        {
                            var application = _mapper.Map<DefaultApplication, Application>(defaultApplication);
                            application.Id = 0;
                            application.GroupId = operationResult.Value.Id;
                            await _applicationsService.CreateAsync(application).ConfigureAwait(false);
                        }
                    }
                }

                var result = _mapper.Map<OperationResult<Group>, OperationResult<GroupDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.Post)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Edit group.
        /// </summary>
        /// <param name="id">Represents group identifier.</param>
        /// <param name="groupDto">Represents group data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(GroupDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Put(long id, [FromBody] GroupDto groupDto)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == id))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var group = new Group()
                {
                    Name = groupDto.Name,
                    Id = id
                };

                var operationResult = await _groupsService.EditAsync(id, group).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Group>, OperationResult<GroupDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.Put)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Delete group.
        /// </summary>
        /// <param name="id">Represents group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == id))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.DeleteAsync(id).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.Delete)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Add specific user to specific group.
        /// </summary>
        /// <param name="groupId">Represents group identifier.</param>
        /// <param name="userId">Represents user identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("AddUserToGroup/{groupId}/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddUserToGroup(long groupId, string userId)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == groupId))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.AddUserToGroupAsync(groupId, userId).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.AddUserToGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Add specific user to specific group.
        /// </summary>
        /// <param name="groupId">Represents group identifier.</param>
        /// <param name="email">Represents email address of user.</param>
        /// <param name="isAdmin">Represents the admin status of user.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("AddUserToGroupByEmail/{groupId}/{email}/{isAdmin}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> AddUserToGroupByEmail(long groupId, string email, bool isAdmin)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var isAdminResult = await _groupsService.CheckUserIsAdminInGroupAsync(groupId, User.Identity.Name).ConfigureAwait(false);
                    if (!isAdminResult.Succeeded || !isAdminResult.Value)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.AddUserToGroupByEmailAsync(groupId, email, isAdmin).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.AddUserToGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Remove specific user from specific group.
        /// </summary>
        /// <param name="groupId">Represents group identifier.</param>
        /// <param name="userId">Represents user identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("RemoveUserFromGroup/{groupId}/{userId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> RemoveUserFromGroup(long groupId, string userId)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == groupId))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.RemoveUserFromGroupAsync(groupId, userId).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.RemoveUserFromGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns the groups the user is a member of. Returns using user id.
        /// </summary>
        /// <param name="userId">Represents user identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetUserGroupsByUserId/{userId}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetUserGroupsByUserId(string userId)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userResult = await _usersService.FindByNameAsync(User.Identity.Name).ConfigureAwait(false);
                    if (userResult?.Value?.Id?.ToLower() != userId.ToLower())
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.GetUserGroupsByUserIdAsync(userId).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Group>>, OperationResult<List<GroupDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.GetUserGroupsByUserId)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns the groups the user is a member of. Returns using user email.
        /// </summary>
        /// <param name="email">Represents user`s email.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<GroupDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetUserGroupsByEmail/{email}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetUserGroupsByEmail(string email)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userResult = await _usersService.FindByNameAsync(User.Identity.Name).ConfigureAwait(false);
                    if (userResult?.Value?.Email?.ToLower() != email.ToLower())
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.GetUserGroupsByEmailAsync(email).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Group>>, OperationResult<List<GroupDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.GetUserGroupsByEmail)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns users that are members of specific group.
        /// </summary>
        /// <param name="groupId">Represents group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetUsersInGroup/{groupId}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetUsersInGroup(long groupId)
        {
            try
            {
                if (!User.IsInRole("Admin"))
                {
                    var userGroups = await _groupsService.GetUserGroupsByEmailAsync(User.Identity.Name).ConfigureAwait(false);
                    if (!userGroups.Succeeded)
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                    if (!userGroups.Value.Any(q => q.Id == groupId))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                var operationResult = await _groupsService.GetUsersInGroupAsync(groupId).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<ApplicationUser>>, OperationResult<List<UserDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.GetUsersInGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns groupId identifier that represents the group identifier the virtual machine is member of.
        /// </summary>
        /// <param name="ipAddress">Represents the ip address of virtual machine.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [AllowAnonymous]
        [HttpGet("GetGroupIdByVirtualMachineIpAddress/{ipAddress}")]
        public async Task<IActionResult> GetGroupIdByVirtualMachineIpAddress(string ipAddress)
        {
            try
            {
                var operationResult = await _groupsService.GetGroupIdByVirtualMachineIpAddress(ipAddress).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<string>, OperationResult<string>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(GroupsController.GetGroupIdByVirtualMachineIpAddress)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}