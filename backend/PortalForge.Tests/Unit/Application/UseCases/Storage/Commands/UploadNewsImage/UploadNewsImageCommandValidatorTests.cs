using FluentAssertions;
using PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage;
using PortalForge.Application.UseCases.Storage.Commands.UploadNewsImage.Validation;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.Storage.Commands.UploadNewsImage;

public class UploadNewsImageCommandValidatorTests
{
    private readonly UploadNewsImageCommandValidator _validator;

    public UploadNewsImageCommandValidatorTests()
    {
        _validator = new UploadNewsImageCommandValidator();
    }

    [Fact]
    public async Task Validate_ValidCommand_PassesValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]), // 1KB
            FileName = "test-image.jpg",
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public async Task Validate_NullFileStream_FailsValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = null!,
            FileName = "test-image.jpg",
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileStream" &&
            e.ErrorMessage.Contains("required"));
    }

    [Fact]
    public async Task Validate_EmptyFileName_FailsValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]),
            FileName = string.Empty,
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileName" &&
            e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("document.pdf")]
    [InlineData("script.js")]
    [InlineData("video.mp4")]
    [InlineData("noextension")]
    public async Task Validate_InvalidFileExtension_FailsValidation(string fileName)
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]),
            FileName = fileName,
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileName" &&
            e.ErrorMessage.Contains("Invalid file type"));
    }

    [Theory]
    [InlineData("image.jpg")]
    [InlineData("photo.jpeg")]
    [InlineData("picture.png")]
    [InlineData("animation.gif")]
    [InlineData("modern.webp")]
    public async Task Validate_ValidFileExtension_PassesValidation(string fileName)
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]),
            FileName = fileName,
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Validate_FileSizeZero_FailsValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[0]),
            FileName = "image.jpg",
            FileSize = 0
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileSize" &&
            e.ErrorMessage.Contains("cannot be empty"));
    }

    [Fact]
    public async Task Validate_FileSizeTooLarge_FailsValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]),
            FileName = "huge-image.jpg",
            FileSize = 11 * 1024 * 1024 // 11MB (over 10MB limit)
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileSize" &&
            e.ErrorMessage.Contains("10MB"));
    }

    [Fact]
    public async Task Validate_FileNameTooLong_FailsValidation()
    {
        // Arrange
        var command = new UploadNewsImageCommand
        {
            FileStream = new MemoryStream(new byte[1024]),
            FileName = new string('a', 256) + ".jpg", // 260 characters
            FileSize = 1024
        };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e =>
            e.PropertyName == "FileName" &&
            e.ErrorMessage.Contains("255 characters"));
    }
}
