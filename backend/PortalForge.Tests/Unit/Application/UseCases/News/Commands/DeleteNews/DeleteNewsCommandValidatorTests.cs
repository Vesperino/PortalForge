using FluentAssertions;
using Moq;
using PortalForge.Application.Common.Interfaces;
using PortalForge.Application.UseCases.News.Commands.DeleteNews;
using PortalForge.Application.UseCases.News.Commands.DeleteNews.Validation;
using Xunit;

namespace PortalForge.Tests.Unit.Application.UseCases.News.Commands.DeleteNews;

public class DeleteNewsCommandValidatorTests
{
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteNewsCommandValidator _validator;

    public DeleteNewsCommandValidatorTests()
    {
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _validator = new DeleteNewsCommandValidator(_unitOfWorkMock.Object);
        SetupDefaultMocks();
    }

    private void SetupDefaultMocks()
    {
        // Setup default mocks for invalid values that trigger async validation
        _unitOfWorkMock.Setup(x => x.NewsRepository.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Domain.Entities.News?)null);
    }

    [Fact]
    public async Task Validate_ValidNewsId_PassesValidation()
    {
        // Arrange
        var command = new DeleteNewsCommand { NewsId = 1 };

        _unitOfWorkMock.Setup(x => x.NewsRepository.GetByIdAsync(1))
            .ReturnsAsync(new Domain.Entities.News { Id = 1, Title = "Test" });

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public async Task Validate_InvalidNewsId_FailsValidation(int newsId)
    {
        // Arrange
        var command = new DeleteNewsCommand { NewsId = newsId };

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "NewsId" &&
            e.ErrorMessage.Contains("greater than 0"));
    }

    [Fact]
    public async Task Validate_NewsDoesNotExist_FailsValidation()
    {
        // Arrange
        var command = new DeleteNewsCommand { NewsId = 999 };

        _unitOfWorkMock.Setup(x => x.NewsRepository.GetByIdAsync(999))
            .ReturnsAsync((Domain.Entities.News?)null);

        // Act
        var result = await _validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e =>
            e.PropertyName == "NewsId" &&
            e.ErrorMessage.Contains("does not exist"));
    }

    [Fact]
    public async Task Validate_NewsExists_CallsRepository()
    {
        // Arrange
        var command = new DeleteNewsCommand { NewsId = 5 };

        _unitOfWorkMock.Setup(x => x.NewsRepository.GetByIdAsync(5))
            .ReturnsAsync(new Domain.Entities.News { Id = 5 });

        // Act
        await _validator.ValidateAsync(command);

        // Assert
        _unitOfWorkMock.Verify(x => x.NewsRepository.GetByIdAsync(5), Times.Once);
    }
}
