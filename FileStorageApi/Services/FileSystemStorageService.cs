using FileStorageApi.Models;

namespace FileStorageApi.Services
{
    /// <summary>
    /// Service for handling file storage operations using the local file system.
    /// </summary>
    /// <param name="currentConfiguration"></param>
    /// <param name="logger"></param>
    public class FileSystemStorageService(IFileStorageConfig currentConfiguration, ILogger<IFileStorageService> logger)
        : IFileStorageService
    {
        private readonly ILogger<IFileStorageService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        #region Implementation of IFileStorageService

        /// <inheritdoc />
        public bool IsInitialized { get; }

        /// <inheritdoc />
        public IFileStorageConfig CurrentConfiguration { get; } = currentConfiguration ?? throw new ArgumentNullException(nameof(currentConfiguration));

        /// <inheritdoc />
        public void Initialize()
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public byte[]? GetFile(string category, int location, string fileName)
        {
            throw new NotImplementedException();
        }

        /// <inheritdoc />
        public bool SaveFile(string category, int location, string fileName, byte[] fileContent)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}