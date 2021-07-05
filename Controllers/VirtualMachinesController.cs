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
using Trading.Services.Groups;
using Trading.Services.Terminals;
using Trading.Services.Users;
using Trading.Services.VirtualMachines;
using Trading.Shared.Results;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class VirtualMachinesController : ControllerBase
    {
        private readonly IVirtualMachinesService _virtualMachinesService;
        private readonly ITerminalsService _terminalsService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        private readonly IUsersService _usersService;
        private readonly IGroupsService _groupsService;

        public VirtualMachinesController(IVirtualMachinesService virtualMachinesService,
            ITerminalsService terminalsService,
            IUsersService usersService,
            IGroupsService groupsService,
            ILogger<VirtualMachinesController> logger,
            IMapper mapper)
        {
            _virtualMachinesService = virtualMachinesService;
            _terminalsService = terminalsService;
            _logger = logger;
            _mapper = mapper;
            _usersService = usersService;
            _groupsService = groupsService;
        }

        /// <summary>
        /// Returns all virtual machines.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<VirtualMachineDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var operationResult = await _virtualMachinesService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<VirtualMachine>>, OperationResult<List<VirtualMachineDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns virtual machine by id.
        /// </summary>
        /// <param name="id">Represents virtual machine identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                var operationResult = await _virtualMachinesService.GetAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<VirtualMachine>, OperationResult<VirtualMachineDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Create virtual machine.
        /// </summary>
        /// <param name="virtualMachineDto">Represents virtual machine data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Post([FromBody] CreateVirtualMachineDto virtualMachineDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(VirtualMachinesController.Post)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<VirtualMachine>(virtualMachineDto);

                var operationResult = await _virtualMachinesService.CreateAsync(entity).ConfigureAwait(false);

                if (operationResult.Succeeded)
                {
                    var defaultTerminalsOperationResult = await _terminalsService.GetDefaultTerminalsAsync().ConfigureAwait(false);
                    if (defaultTerminalsOperationResult != null && defaultTerminalsOperationResult.Value != null && defaultTerminalsOperationResult.Value.Count > 0)
                    {
                        foreach (var defaultTerminal in defaultTerminalsOperationResult.Value)
                        {
                            var terminal = _mapper.Map<DefaultTerminal, Terminal>(defaultTerminal);
                            terminal.Id = 0;
                            terminal.VirtualMachineId = operationResult.Value.Id;
                            await _terminalsService.CreateAsync(terminal).ConfigureAwait(false);
                        }
                    }

                    return await Get(operationResult.Value.Id);
                }

                var result = _mapper.Map<OperationResult<VirtualMachine>, OperationResult<VirtualMachineDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.Post)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Edit virtual machine.
        /// </summary>
        /// <param name="id">Represents virtual machine identifier.</param>
        /// <param name="virtualMachineDto">Represents virtual machine data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(VirtualMachineDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Put(long id, [FromBody] PutVirtualMachineDto virtualMachineDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(VirtualMachinesController.Put)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var entity = _mapper.Map<VirtualMachine>(virtualMachineDto);
                entity.Id = id;


                var operationResult = await _virtualMachinesService.EditAsync(id, entity).ConfigureAwait(false);

                if (operationResult.Succeeded)
                {
                    return await Get(operationResult.Value.Id);
                }

                var result = _mapper.Map<OperationResult<VirtualMachine>, OperationResult<VirtualMachineDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.Put)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Delete virtual machine.
        /// </summary>
        /// <param name="id">Represents virtual machine identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var operationResult = await _virtualMachinesService.DeleteAsync(id).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.Delete)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns all virtual machines the are members of current group.
        /// </summary>
        /// <param name="groupId">Represents group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<VirtualMachineDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("GetByGroupId/{groupId}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> GetByGroupId(long groupId)
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

                var operationResult = await _virtualMachinesService.GetByGroupIdAsync(groupId).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<VirtualMachine>>, OperationResult<List<VirtualMachineDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.GetByGroupId)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Add virtual machine to group.
        /// </summary>
        /// <param name="virtualMachineId">Represents virtual machine identifier.</param>
        /// <param name="groupId">Represents group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("AddVirtualMachineToGroup/{virtualMachineId}/{groupId}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddVirtualMachineToGroup(long virtualMachineId, long groupId)
        {
            try
            {
                var operationResult = await _virtualMachinesService.AddVirtualMachineToGroupAsync(groupId, virtualMachineId).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.AddVirtualMachineToGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Remove virtual machine from group.
        /// </summary>
        /// <param name="virtualMachineId">Represents virtual machine identifier.</param>
        /// <param name="groupId">Represents group identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("RemoveVirtualMachineFromGroup/{virtualMachineId}/{groupId}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> RemoveVirtualMachineFromGroup(long virtualMachineId, long groupId)
        {
            try
            {
                var operationResult = await _virtualMachinesService.RemoveVirtualMachineFromGroupAsync(groupId, virtualMachineId).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(VirtualMachinesController.RemoveVirtualMachineFromGroup)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
