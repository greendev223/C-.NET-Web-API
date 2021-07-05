using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Terminals;
using Trading.Shared.Results;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class TerminalsController : ControllerBase
    {
        private readonly ITerminalsService _terminalsService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public TerminalsController(ITerminalsService terminalsService, ILogger<TerminalsController> logger, IMapper mapper)
        {
            _terminalsService = terminalsService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all terminals.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<TerminalDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try {
                var operationResult = await _terminalsService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Terminal>>, OperationResult<List<TerminalDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns terminal by id.
        /// </summary>
        /// <param name="id">Represents terminal identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(TerminalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                var operationResult = await _terminalsService.GetAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Terminal>, OperationResult<TerminalDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
        
        /// <summary>
        /// Create terminal.
        /// </summary>
        /// <param name="terminalDto">Represents terminal data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(TerminalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CreateTerminalDto terminalDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(TerminalsController.Post)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var terminal = new Terminal()
                {
                    Name = terminalDto.Name,
                    ApplicationId = terminalDto.ApplicationId,
                    VirtualMachineId = terminalDto.VirtualMachineId,
                    FullLocalPath = terminalDto.FullLocalPath,
                    ShortCutFullPath = terminalDto.ShortCutFullPath,
                    Description = terminalDto.Description
                };

                var operationResult = await _terminalsService.CreateAsync(terminal).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Terminal>, OperationResult<TerminalDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.Post)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Edit terminal.
        /// </summary>
        /// <param name="id">Represents terminal identifier.</param>
        /// <param name="terminalDto">Represents terminal data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(TerminalDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] UpdateTerminalDto terminalDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(TerminalsController.Put)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var terminal = new Terminal()
                {
                    Id = id,
                    Name = terminalDto.Name,
                    ApplicationId = terminalDto.ApplicationId,
                    VirtualMachineId = terminalDto.VirtualMachineId,
                    FullLocalPath = terminalDto.FullLocalPath,
                    ShortCutFullPath = terminalDto.ShortCutFullPath,
                    Description = terminalDto.Description
                };

                var operationResult = await _terminalsService.EditAsync(id, terminal).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Terminal>, OperationResult<TerminalDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.Put)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Delete terminal.
        /// </summary>
        /// <param name="id">Represents terminal identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var operationResult = await _terminalsService.DeleteAsync(id).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.Delete)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Update terminal application.
        /// </summary>
        /// <param name="updateTerminalApplication">Represents updateTerminalApplication data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("UpdateTerminalApplication")]
        public async Task<IActionResult> UpdateTerminalApplication([FromBody] UpdateTerminalApplication updateTerminalApplication)
        {
            try
            {
                var operationResult = await _terminalsService.UpdateTerminalApplication(updateTerminalApplication.TerminalId,
                    updateTerminalApplication.ApplicationId, updateTerminalApplication.ShortCutFullPath).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(TerminalsController.UpdateTerminalApplication)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}