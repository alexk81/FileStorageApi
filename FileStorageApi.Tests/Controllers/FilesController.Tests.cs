using FileStorageApi.Controllers;
using FileStorageApi.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace FileStorageApi.Tests.Controllers
{
    public class FilesControllerTests
    {
        //** Constructor Tests **//

        [Fact]
        public void FilesController_Constructor_ThrowsArgumentNullException_WhenFileStorageFactoryIsNull()
        {
            // Arrange
            IFileStorageFactory? fileStorageFactory = null;
            var loggerMock = Substitute.For<ILogger<FilesController>>();

            // Act
            Action act = () => new FilesController(fileStorageFactory, loggerMock);

            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*fileStorageFactory*")
                .And.ParamName.Should().Be("fileStorageFactory");
        }

        [Fact]
        public void FilesController_Constructor_ThrowsArgumentNullException_WhenLoggerIsNull()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            ILogger<FilesController>? logger = null;
            // Act
            Action act = () => new FilesController(fileStorageFactoryMock, logger);
            // Assert
            act.Should().Throw<ArgumentNullException>()
                .WithMessage("*logger*")
                .And.ParamName.Should().Be("logger");
        }

        [Fact]
        public void FilesController_Constructor_CreatesInstance_WhenParametersAreValid()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<ILogger<FilesController>>();
            // Act
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            // Assert
            controller.Should().NotBeNull();
        }

        //** End Constructor Tests **//

        //** GetFile Tests **//

        [Fact]
        public void GetFile_LogsWarningAndReturnsNotFound_WhenFileDoesNotExist()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "nonExistentFile.txt";
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.GetFile(category, location, fileName).Returns((byte[]?)null);
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);
            var expectedWarningMessage = "File not found: {Category}/{Location}/{FileName}";
            loggerMock.IsEnabled(LogLevel.Warning).Returns(true); // Ensure logger is enabled for warning level

            // Act
            var result = controller.GetFile(category, location, fileName);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                expectedWarningMessage,
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void GetFile_Returns500Error_WhenFileStorageServiceThrowsException()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "errorFile.txt";
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.GetFile(category, location, fileName).Throws(new Exception("Test exception"));
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);

            // Act
            var result = controller.GetFile(category, location, fileName);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Is<Exception>(e => e.Message == "Test exception"),
                "Error retrieving file: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void GetFile_ReturnsFileStreamResult_WhenFileExists()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "existingFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.GetFile(category, location, fileName).Returns(fileContent);
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);

            // Act
            var result = controller.GetFile(category, location, fileName);

            // Assert
            result.Should().BeOfType<FileStreamResult>();
            var fileStreamResult = (FileStreamResult)result;
            byte[] readContent = new byte[fileContent.Length];
            fileStreamResult.FileStream.Read(readContent);
            readContent.Length.Should().Be(fileContent.Length);
            readContent.Should().BeEquivalentTo(fileContent);
            fileStreamResult.ContentType.Should().Be("application/octet-stream");
            fileStreamResult.FileDownloadName.Should().Be(fileName);
            loggerMock.Received(1).Log(
                LogLevel.Information,
                "File retrieved successfully: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        //** End GetFile Tests **//

        //** SaveFile Tests **//

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenFileContentIsNull()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "emptyFile.txt";
            byte[]? fileContent = null;

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("File content cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "File content is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenFileContentIsEmpty()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "emptyFile.txt";
            var fileContent = Array.Empty<byte>();

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("File content cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "File content is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenCategoryIsNull() {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            string? category = null;
            var location = 1;
            var fileName = "testFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Category cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "Category is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenCategoryIsEmpty()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = string.Empty;
            var location = 1;
            var fileName = "testFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Category cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "Category is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenLocationIsLessThanZero()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = -1; // Invalid location
            var fileName = "testFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("Location must be a non-negative integer.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "Invalid location for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenFileNameIsNull()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            string? fileName = null; // Invalid file name
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("File name cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "File name is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns400BadRequest_WhenFileNameIsEmpty()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = string.Empty; // Invalid file name
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<BadRequestObjectResult>();
            var badRequestResult = (BadRequestObjectResult)result;
            badRequestResult.StatusCode.Should().Be(400);
            badRequestResult.Value.Should().Be("File name cannot be null or empty.");
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "File name is null or empty for: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns500Error_WhenFileStorageServiceThrowsException()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "errorFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.SaveFile(category, location, fileName, fileContent).Throws(new Exception("Test exception"));
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            loggerMock.Received(1).Log(
                LogLevel.Error,
                Arg.Is<Exception>(e => e.Message == "Test exception"),
                "Error saving file: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns500Error_WhenFileStorageServiceReturnsFalse()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "failedSaveFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.SaveFile(category, location, fileName, fileContent).Returns(false);
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);
            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);
            // Assert
            result.Should().BeOfType<ObjectResult>().Which.StatusCode.Should().Be(500);
            loggerMock.Received(1).Log(
                LogLevel.Warning,
                "Failed to save file: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }

        [Fact]
        public void SaveFile_Returns200Ok_WhenFileIsSavedSuccessfully()
        {
            // Arrange
            var fileStorageFactoryMock = Substitute.For<IFileStorageFactory>();
            var loggerMock = Substitute.For<TestableLogger<FilesController>>();
            var controller = new FilesController(fileStorageFactoryMock, loggerMock);
            var category = "testCategory";
            var location = 1;
            var fileName = "testFile.txt";
            var fileContent = new byte[] { 1, 2, 3, 4, 5 };
            var fileStorageServiceMock = Substitute.For<IFileStorageService>();
            fileStorageServiceMock.SaveFile(category, location, fileName, fileContent).Returns(true);
            fileStorageFactoryMock.Create().Returns(fileStorageServiceMock);

            // Act
            var result = controller.SaveFile(category, location, fileName, fileContent);

            // Assert
            result.Should().BeOfType<OkResult>();
            loggerMock.Received(1).Log(
                LogLevel.Information,
                "File saved successfully: {Category}/{Location}/{FileName}",
                category,
                location,
                fileName
            );
            loggerMock.ReceivedCalls().Count().Should().Be(1); // Ensure the logger was called exactly once
        }
    }
}
