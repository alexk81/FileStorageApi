namespace FileStorageApi.Models
{
    /// <summary>
    /// Default implementation of the file storage configuration.
    /// </summary>
    public class FileStorageConfig : IFileStorageConfig
    {
        #region Implementation of IFileStorageConfig

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string BasePath { get; }

        /// <inheritdoc />
        public string DefaultFileExtension { get; }

        /// <inheritdoc />
        public long MaxFileSize { get; }

        /// <inheritdoc />
        public string ApiKey { get; }

        /// <inheritdoc />
        public string ConnectionString { get; }

        /// <inheritdoc />
        public string StorageType { get; }

        /// <inheritdoc />
        public string Username { get; }

        /// <inheritdoc />
        public string Password { get; }

        /// <inheritdoc />
        public bool Compression { get; }

        #endregion

        /// <summary>
        /// Default constructor for the <see cref="FileStorageConfig"/> class.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="basePath"></param>
        /// <param name="defaultFileExtension"></param>
        /// <param name="maxFileSize"></param>
        /// <param name="apiKey"></param>
        /// <param name="connectionString"></param>
        /// <param name="storageType"></param>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="compression"></param>
        public FileStorageConfig(string name, string? basePath = null, string? defaultFileExtension = null, long maxFileSize = default, string? apiKey = null, string? connectionString = null, string? storageType = null, string? username = null, string? password = null, bool compression = default)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            BasePath = basePath ?? string.Empty;
            DefaultFileExtension = defaultFileExtension ?? string.Empty;
            MaxFileSize = maxFileSize;
            ApiKey = apiKey ?? string.Empty;
            ConnectionString = connectionString ?? string.Empty;
            StorageType = storageType ?? string.Empty;
            Username = username ?? string.Empty;
            Password = password ?? string.Empty;
            Compression = compression;
        }
    }
}
