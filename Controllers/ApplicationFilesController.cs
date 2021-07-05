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
using Trading.Services.ApplicationFiles;
using Trading.Services.Applications;
using Trading.Shared.Results;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class ApplicationFilesController : ControllerBase
    {
        private readonly IApplicationFilesService _applicationFilesService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public ApplicationFilesController(IApplicationFilesService applicationFilesService,
            ILogger<ApplicationsController> logger, 
            IMapper mapper)
        {
            _applicationFilesService = applicationFilesService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all application files.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ApplicationFileDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try {
                var operationResult = await _applicationFilesService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<ApplicationFile>>, OperationResult<List<ApplicationFileDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationFilesController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Return application file id.
        /// </summary>
        /// <param name="id">Represents application file identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApplicationFileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                var operationResult = await _applicationFilesService.GetAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<ApplicationFile>, OperationResult<ApplicationFileDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationFilesController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Create application file.
        /// </summary>
        /// <param name="applicationFileDto">Represents application file data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApplicationFileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ApplicationFileDto applicationFileDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(ApplicationFilesController.Post)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var applicationFile = new ApplicationFile()
                {
                    ApplicationId = applicationFileDto.ApplicationId,
                    AbsolutePathInTerminal = applicationFileDto.AbsolutePathInTerminal,
                    AbsoluteSourcePath = applicationFileDto.AbsoluteSourcePath
                };

                var operationResult = await _applicationFilesService.CreateAsync(applicationFile).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<ApplicationFile>, OperationResult<ApplicationFileDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationFilesController.Post)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Edit application file.
        /// </summary>
        /// <param name="id">Represents application file identifier.</param>
        /// <param name="applicationFileDto">Represents application file data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApplicationFileDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(long id, [FromBody] ApplicationFileDto applicationFileDto)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning($"{nameof(ApplicationFilesController.Put)}", ModelState.Values);
                return BadRequest(ModelState);
            }

            try
            {
                var applicationFile = new ApplicationFile()
                {
                    ApplicationId = id,
                    AbsolutePathInTerminal = applicationFileDto.AbsolutePathInTerminal,
                    AbsoluteSourcePath = applicationFileDto.AbsoluteSourcePath
                };

                var operationResult = await _applicationFilesService.EditAsync(id, applicationFile).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<ApplicationFile>, OperationResult<ApplicationFileDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationFilesController.Put)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Delete application file.
        /// </summary>
        /// <param name="id">Represents application file identifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var operationResult = await _applicationFilesService.DeleteAsync(id).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationFilesController.Delete)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}
