using FileStorageApi.Models;

namespace FileStorageApi.Services
{
    /// <summary>
    /// Interface for file storage service.
    /// </summary>
    public interface IFileStorageService
    {
        /// <summary>
        /// Checks if the file storage service has been initialized.
        /// </summary>
        bool IsInitialized { get; }

        /// <summary>
        /// Gets the current configuration for the file storage service.
        /// </summary>
        IFileStorageConfig CurrentConfiguration { get; }

        /// <summary>
        /// Initializes the file storage service with the current configuration.
        /// </summary>
        void Initialize();

        /// <summary>
        /// Retrieves a file from the storage.
        /// </summary>
        /// <param name="category">The category of the file.</param>
        /// <param name="location">The location identifier.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <returns>A byte array representing the file content.</returns>
        byte[]? GetFile(string category, int location, string fileName);

        /// <summary>
        /// Saves a file to the storage.
        /// </summary>
        /// <param name="category">The category of the file.</param>
        /// <param name="location">The location identifier.</param>
        /// <param name="fileName">The name of the file.</param>
        /// <param name="fileContent">The content of the file as a byte array.</param>
        bool SaveFile(string category, int location, string fileName, byte[] fileContent);
    }
}
