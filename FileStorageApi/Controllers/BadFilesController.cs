using FileStorageApi.Models;
using FileStorageApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FileStorageApi.Controllers
{
    /// <summary>
    /// Controller for handling file storage operations.
    /// </summary>
    [Route("api/[controller]")]
    [Authorize]
    public class BadFilesController : ControllerBase
    {
        private readonly IFileStorageConfig _fileStorageConfig;

        /// <summary>
        /// Logger for the <see cref="BadFilesController"/> class.
        /// </summary>
        protected ILogger<BadFilesController> Logger { get; }


        protected ILogger<IFileStorageService> StorageLogger { get; }

        /// <summary>
        /// Default constructor for the <see cref="BadFilesController"/> class.
        /// </summary>
        /// <param name="fileStorageConfig"></param>
        /// <param name="loggerFactory"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public BadFilesController(IFileStorageConfig fileStorageConfig, ILoggerFactory loggerFactory)
        {
            _fileStorageConfig = fileStorageConfig ?? throw new ArgumentNullException(nameof(fileStorageConfig));
            Logger = loggerFactory.CreateLogger<BadFilesController>() ?? throw new ArgumentNullException(nameof(loggerFactory));
            StorageLogger = loggerFactory.CreateLogger<IFileStorageService>() ?? throw new ArgumentNullException(nameof(loggerFactory));
        }

        // This could be a lot more complicated...
        private IFileStorageService GetFileStorageService()
        {
            return _fileStorageConfig.Name switch
            {
                "LocalFileSystem" => new FileSystemStorageService(_fileStorageConfig, StorageLogger),
                "AwsS3Storage" => new AwsS3StorageService(_fileStorageConfig, StorageLogger),
                "GoogleBucketsStorage" => new GoogleBucketsStorageService(_fileStorageConfig, StorageLogger),
                _ => throw new NotSupportedException($"Unsupported storage service: {_fileStorageConfig.Name}")
            };
        }

        /// <summary>
        /// Endpoint to return a stored file.
        /// </summary>
        /// <returns></returns>
        [HttpGet("{category}/{location:int}/{fileName}", Name = nameof(BadFilesController) + nameof(GetFile))]
        [AllowAnonymous]
        public IActionResult GetFile(string category, int location, string fileName)
        {
            try {
                var result = GetFileStorageService().GetFile(category, location, fileName);

                if (result == null)
                {
                    Logger.LogWarning("File not found: {Category}/{Location}/{FileName}", category, location, fileName);
                    return NotFound();
                }

                var stream = new MemoryStream(result);
                var contentType = "application/octet-stream";
                var response = new FileStreamResult(stream, contentType)
                {
                    FileDownloadName = fileName
                };
                Logger.LogInformation("File retrieved successfully: {Category}/{Location}/{FileName}", category, location, fileName);

                return response;
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error retrieving file: {Category}/{Location}/{FileName}", category, location, fileName);
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Endpoint to save a file to the storage service.
        /// </summary>
        /// <param name="category"></param>
        /// <param name="location"></param>
        /// <param name="fileName"></param>
        /// <param name="fileContent"></param>
        /// <returns></returns>
        [HttpPost("{category}/{location:int}/{fileName}", Name = nameof(BadFilesController) + nameof(SaveFile))]
        public IActionResult SaveFile(string category, int location, string fileName, [FromBody] byte[]? fileContent)
        {
            if (fileContent == null || fileContent.Length == 0)
            {
                Logger.LogWarning("File content is null or empty for: {Category}/{Location}/{FileName}", category, location, fileName);
                return BadRequest("File content cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(category))
            {
                Logger.LogWarning("Category is null or empty for: {Category}/{Location}/{FileName}", category, location, fileName);
                return BadRequest("Category cannot be null or empty.");
            }

            if (string.IsNullOrWhiteSpace(fileName))
            {
                Logger.LogWarning("File name is null or empty for: {Category}/{Location}/{FileName}", category, location, fileName);
                return BadRequest("File name cannot be null or empty.");
            }

            if (location < 0)
            {
                Logger.LogWarning("Invalid location for: {Category}/{Location}/{FileName}", category, location, fileName);
                return BadRequest("Location must be a non-negative integer.");
            }

            try
            {
                var result = GetFileStorageService().SaveFile(category, location, fileName, fileContent);
                if (result)
                {
                    Logger.LogInformation("File saved successfully: {Category}/{Location}/{FileName}", category, location, fileName);
                    return Ok();
                }

                Logger.LogWarning("Failed to save file: {Category}/{Location}/{FileName}", category, location, fileName);
                return StatusCode(500, "Failed to save the file.");
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "Error saving file: {Category}/{Location}/{FileName}", category, location, fileName);
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
