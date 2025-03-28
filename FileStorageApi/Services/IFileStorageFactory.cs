namespace FileStorageApi.Services
{
    /// <summary>
    /// Factory interface for creating instances of file storage services.
    /// </summary>
    public interface IFileStorageFactory
    {
        /// <summary>
        /// Creates an instance of a file storage service.
        /// </summary>
        /// <returns></returns>
        IFileStorageService Create();
    }
}
