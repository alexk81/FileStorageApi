namespace FileStorageApi.Models
{
    /// <summary>
    /// Interface for file storage configuration.
    /// </summary>
    public interface IFileStorageConfig
    {
        /// <summary>
        /// Gets the name of the file storage configuration.
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the base path for file storage.
        /// </summary>
        string BasePath { get; }

        /// <summary>
        /// Gets the default file extension for stored files.
        /// </summary>
        string DefaultFileExtension { get; }

        /// <summary>
        /// Gets the maximum allowed file size in bytes.
        /// </summary>
        long MaxFileSize { get; }

        /// <summary>
        /// Gets the API key for authentication with the file storage service.
        /// </summary>
        public string ApiKey { get; }

        /// <summary>
        /// Gets the connection string for the file storage service.
        /// </summary>
        public string ConnectionString { get; }

        /// <summary>
        /// Gets the storage type (e.g., "local", "cloud", etc.) for the file storage service.
        /// </summary>
        public string StorageType { get; }

        /// <summary>
        /// Gets the username for authentication with the file storage service.
        /// </summary>
        public string Username { get; }

        /// <summary>
        /// Gets the password for authentication with the file storage service.
        /// </summary>
        public string Password { get; }

        /// <summary>
        /// Gets a value indicating whether to enable compression for stored files.
        /// </summary>
        public bool Compression { get; }
    }
}
