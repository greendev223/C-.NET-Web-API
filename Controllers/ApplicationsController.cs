using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trading.Data.Entities;
using Trading.Dtos;
using Trading.Services.Applications;
using Trading.Services.Groups;
using Trading.Shared.Results;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Trading.WebApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationsService _applicationsService;
        private readonly IGroupsService _groupsService;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;
        public ApplicationsController(IApplicationsService applicationsService,
            IGroupsService groupsService,
            ILogger<ApplicationsController> logger,
            IMapper mapper)
        {
            _applicationsService = applicationsService;
            _groupsService = groupsService;
            _logger = logger;
            _mapper = mapper;
        }

        /// <summary>
        /// Returns all applications.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ApplicationDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get()
        {
            try
            {
                var operationResult = await _applicationsService.GetAllAsync().ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Application>>, OperationResult<List<ApplicationDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns application by id.
        /// </summary>
        /// <param name="id">Represents application indentifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpGet("{id}")]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Get(long id)
        {
            try
            {
                var operationResult = await _applicationsService.GetAsync(id).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Application>, OperationResult<ApplicationDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        ///// <summary>
        ///// Upload file to the server.
        ///// </summary>
        ///// <param name="file">Represents a file sent with the HttpRequest.</param>
        ///// <returns></returns>
        //[ProducesResponseType(typeof(List<string>), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[HttpPost("PostApplicationFile")]
        //[AllowAnonymous]
        //public async Task<IActionResult> PostApplicationFileAsync([FromForm] IFormFile file)
        //{
        //    var fileNames = new List<string>();
        //    if (file.Length > 0)
        //    {
        //        using (var memoryStream = new MemoryStream())
        //        {
        //            file.CopyTo(memoryStream);
        //            var fileBytes = memoryStream.ToArray();

        //            string s = Convert.ToBase64String(fileBytes);
        //        }
        //    }

        //    return Ok(fileNames);
        //}

        /// <summary>
        /// Create application.
        /// </summary>
        /// <param name="applicationDto">Represents application data transfer object.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [HttpPost]
        [Authorize(Roles = "Admin, User")]
        public async Task<IActionResult> Post([FromBody] ApplicationDto app)
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
                    if (!userGroups.Value.Any(q => q.Id == app.GroupId))
                    {
                        var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"You have no access to process this request." });
                        return Ok(errorResult);
                    }
                }

                //if (file == null || file.Length == 0)
                //{
                //    var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"Couldn't create application without file. Please upload the archive with application." });
                //    return Ok(errorResult);
                //}

                //var supportedTypes = new[] { "zip" };
                //var fileExt = System.IO.Path.GetExtension(file.FileName).Substring(1);
                //if (!supportedTypes.Contains(fileExt))
                //{
                //    var errorResult = OperationResult.ReturnError(ServiceResponseStatusCode.Forbidden, new List<string>() { $"Couldn't upload file. File extention should be zip." });
                //    return Ok(errorResult);
                //}

                //var application = new Application()
                //{
                //    Name = file.FileName.Split(".")[0],
                //    IsEnabled = true,
                //    GroupId = groupId,
                //    Level = ApplicationLevel.Production
                //};

                //using (var memoryStream = new MemoryStream())
                //{
                //    file.CopyTo(memoryStream);
                //    application.AppZipSourceBytes = memoryStream.ToArray();
                //}

                var application = _mapper.Map<ApplicationDto, Application>(app);
                string fileName = $"{Guid.NewGuid()}.zip";
                var ftpUploadOperationResult = await _applicationsService.UploadApplicationToFtpServer(app.AppZipSourceBytes, fileName).ConfigureAwait(false);
                if (!ftpUploadOperationResult.Succeeded)
                {
                    return Ok(ftpUploadOperationResult);
                }

                application.UrlToApplicationOnFtpServer = ftpUploadOperationResult.Value;
                var applicationFilesOperationResult = new OperationResult<List<ApplicationFile>>();
                applicationFilesOperationResult = await _applicationsService.ParseApplicationSkeletonAsync(app.AppZipSourceBytes, app.Name).ConfigureAwait(false);

                if (!applicationFilesOperationResult.Succeeded)
                {
                    return Ok(applicationFilesOperationResult);
                }

                application.Files = applicationFilesOperationResult.Value;

                var operationResult = await _applicationsService.CreateAsync(application).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<Application>, OperationResult<ApplicationDto>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationsController.Post)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        ///// <summary>
        ///// Edit application.
        ///// </summary>
        ///// <param name="id">Represents application indentifier.</param>
        ///// <param name="applicationDto">Represents application data transfer object.</param>
        ///// <returns></returns>
        //[ProducesResponseType(typeof(ApplicationDto), StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status500InternalServerError)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //[HttpPut("{id}")]
        //[Authorize(Roles = "Admin, User")]
        //public async Task<IActionResult> Put(long id, [FromBody] ApplicationDto applicationDto)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        _logger.LogWarning($"{nameof(ApplicationsController.Put)}", ModelState.Values);
        //        return BadRequest(ModelState);
        //    }

        //    try
        //    {
        //        var application = new Application()
        //        {
        //            Id = id,
        //            Name = applicationDto.Name,
        //            IsEnabled = applicationDto.IsEnabled,
        //            GroupId = applicationDto.GroupId,
        //            Level = (ApplicationLevel)Enum.ToObject(typeof(ApplicationLevel), (int)applicationDto.Level),
        //            UrlToApplicationOnFtpServer = applicationDto.UrlToApplicationOnFtpServer
        //        };

        //        var operationResult = await _applicationsService.EditAsync(id, application).ConfigureAwait(false);

        //        var result = _mapper.Map<OperationResult<Application>, OperationResult<ApplicationDto>>(operationResult);

        //        return Ok(result);
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.LogError($"{nameof(ApplicationsController.Put)}", e);
        //        return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
        //    }
        //}

        /// <summary>
        /// Delete application.
        /// </summary>
        /// <param name="id">Represents application indentifier.</param>
        /// <returns></returns>
        [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var operationResult = await _applicationsService.DeleteAsync(id).ConfigureAwait(false);

                return Ok(operationResult);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationsController.Delete)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }

        /// <summary>
        /// Returns all applications that belongs to specific group.
        /// </summary>
        /// <returns></returns>
        [ProducesResponseType(typeof(List<ApplicationDto>), StatusCodes.Status200OK)]
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

                var operationResult = await _applicationsService.GetByGroupId(groupId).ConfigureAwait(false);

                var result = _mapper.Map<OperationResult<List<Application>>, OperationResult<List<ApplicationDto>>>(operationResult);

                return Ok(result);
            }
            catch (Exception e)
            {
                _logger.LogError($"{nameof(ApplicationsController.Get)}", e);
                return StatusCode(StatusCodes.Status500InternalServerError, e.Message);
            }
        }
    }
}