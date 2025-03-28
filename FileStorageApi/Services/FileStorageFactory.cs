using FileStorageApi.Models;

namespace FileStorageApi.Services
{
    /// <summary>
    /// Factory for creating instances of file storage services based on the current configuration.
    /// </summary>
    /// <param name="currentConfiguration"></param>
    /// <param name="logger"></param>
    public class FileStorageFactory(IFileStorageConfig currentConfiguration, ILogger<IFileStorageService> logger)
        : IFileStorageFactory
    {
        private readonly ILogger<IFileStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IFileStorageConfig _currentConfiguration = currentConfiguration ?? throw new ArgumentNullException(nameof(currentConfiguration));

        #region Implementation of IFileStorageFactory

        /// <inheritdoc />
        public IFileStorageService Create()
        {
            switch (_currentConfiguration.Name) {
                case "AwsS3Storage":
                    return new AwsS3StorageService(_currentConfiguration, _logger);
                case "GoogleBucketsStorage":
                    return new GoogleBucketsStorageService(_currentConfiguration, _logger);
                case "LocalFileSystem":
                    return new FileSystemStorageService(_currentConfiguration, _logger);
                default:
                    _logger.LogError("Unsupported file storage configuration: {StorageName}", _currentConfiguration.Name);
                    throw new NotSupportedException($"The file storage configuration '{_currentConfiguration.Name}' is not supported.");
            }
        }

        #endregion
    }
}
