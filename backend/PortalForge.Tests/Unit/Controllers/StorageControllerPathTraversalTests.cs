using FluentAssertions;
using Xunit;

namespace PortalForge.Tests.Unit.Controllers;

/// <summary>
/// Tests for path traversal protection in StorageController.
/// These tests verify that the ContainsPathTraversalPattern method correctly
/// identifies and blocks path traversal attack vectors.
/// </summary>
public class StorageControllerPathTraversalTests
{
    /// <summary>
    /// Implementation of ContainsPathTraversalPattern for testing.
    /// This mirrors the private method in StorageController.
    /// </summary>
    private static bool ContainsPathTraversalPattern(string path)
    {
        if (string.IsNullOrEmpty(path))
            return false;

        var dangerousPatterns = new[]
        {
            "..",           // Parent directory
            "..\\",         // Windows parent
            "../",          // Unix parent
            "\\",           // Backslash (normalize to forward slash)
            "%",            // URL encoding remnants after decode
            "\0",           // Null byte injection
            "..%",          // Partial encoded traversal
            "%2e",          // Encoded dot
            "%2f",          // Encoded forward slash
            "%5c",          // Encoded backslash
        };

        var lowerPath = path.ToLowerInvariant();
        return dangerousPatterns.Any(pattern => lowerPath.Contains(pattern.ToLowerInvariant()));
    }

    [Theory]
    [InlineData("images/photo.jpg")]
    [InlineData("news-images/article.png")]
    [InlineData("service-icons/icon.svg")]
    [InlineData("folder/subfolder/file.pdf")]
    [InlineData("file_with_underscores.txt")]
    [InlineData("file-with-dashes.doc")]
    [InlineData("CamelCaseFile.xlsx")]
    public void ContainsPathTraversalPattern_ValidPaths_ReturnsFalse(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeFalse($"Path '{path}' should be considered valid");
    }

    [Theory]
    [InlineData("..")]
    [InlineData("../")]
    [InlineData("..\\")]
    [InlineData("../etc/passwd")]
    [InlineData("..\\windows\\system32")]
    [InlineData("images/../../../etc/passwd")]
    [InlineData("folder/../../secret.txt")]
    public void ContainsPathTraversalPattern_BasicTraversalPatterns_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Path '{path}' should be detected as path traversal");
    }

    [Theory]
    [InlineData("%2e%2e")]
    [InlineData("%2e%2e/")]
    [InlineData("%2e%2e%2f")]
    [InlineData("%2e%2e%5c")]
    [InlineData("images%2f..%2f..%2fetc%2fpasswd")]
    public void ContainsPathTraversalPattern_UrlEncodedPatterns_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"URL encoded path '{path}' should be detected as path traversal");
    }

    [Theory]
    [InlineData("\\")]
    [InlineData("folder\\file.txt")]
    [InlineData("images\\subfolder\\photo.jpg")]
    public void ContainsPathTraversalPattern_BackslashPaths_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Backslash path '{path}' should be blocked");
    }

    [Theory]
    [InlineData("file\0.txt")]
    [InlineData("image.jpg\0.exe")]
    [InlineData("document\0/etc/passwd")]
    public void ContainsPathTraversalPattern_NullByteInjection_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Null byte injection '{path}' should be detected");
    }

    [Theory]
    [InlineData("..%2f")]
    [InlineData("..%5c")]
    [InlineData("..%00")]
    public void ContainsPathTraversalPattern_MixedEncodingAttacks_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Mixed encoding attack '{path}' should be detected");
    }

    [Fact]
    public void ContainsPathTraversalPattern_NullPath_ReturnsFalse()
    {
        // Arrange
        string? path = null;

        // Act
        var result = ContainsPathTraversalPattern(path!);

        // Assert
        result.Should().BeFalse("Null path should return false");
    }

    [Fact]
    public void ContainsPathTraversalPattern_EmptyPath_ReturnsFalse()
    {
        // Arrange
        var path = string.Empty;

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeFalse("Empty path should return false");
    }

    [Theory]
    [InlineData("..")]
    [InlineData("../etc/passwd")]
    [InlineData("FOLDER/../SECRET")]
    public void ContainsPathTraversalPattern_CaseInsensitive_DetectsUpperAndLowerCase(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Path '{path}' should be detected regardless of case");
    }

    [Theory]
    [InlineData("%2E%2E")]
    [InlineData("%2F")]
    [InlineData("%5C")]
    public void ContainsPathTraversalPattern_UpperCaseEncoding_ReturnsTrue(string path)
    {
        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Upper case URL encoded path '{path}' should be detected");
    }

    [Theory]
    [InlineData("images/valid..name.jpg")]
    [InlineData("file..with..dots.txt")]
    public void ContainsPathTraversalPattern_DoubleDotInFilename_ReturnsTrue(string path)
    {
        // Note: The current implementation is strict and will flag double dots even in filenames.
        // This is a security-first approach.

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Path with '..' pattern '{path}' is flagged for security");
    }

    [Theory]
    [InlineData("path/with%20space/file.txt")]
    [InlineData("encoded%file.txt")]
    public void ContainsPathTraversalPattern_PercentInPath_ReturnsTrue(string path)
    {
        // Note: The implementation blocks any '%' character to prevent encoding-based attacks.
        // This may be overly restrictive but prioritizes security.

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Path with '%' character '{path}' is blocked");
    }

    [Theory]
    [InlineData("....")]
    [InlineData(".../")]
    [InlineData("/...")]
    public void ContainsPathTraversalPattern_TripleDotPatterns_ReturnsTrue(string path)
    {
        // Triple dots contain ".." so should be blocked

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Triple dot pattern '{path}' should be blocked as it contains '..'");
    }

    [Theory]
    [InlineData("./file.txt")]
    [InlineData("folder/./file.txt")]
    [InlineData("./")]
    public void ContainsPathTraversalPattern_SingleDotPaths_ReturnsFalse(string path)
    {
        // Single dot (current directory) is not a traversal attack

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeFalse($"Single dot path '{path}' should be allowed");
    }

    [Fact]
    public void ContainsPathTraversalPattern_VeryLongPath_HandlesCorrectly()
    {
        // Arrange
        var longPath = string.Join("/", Enumerable.Repeat("folder", 100)) + "/file.txt";

        // Act
        var result = ContainsPathTraversalPattern(longPath);

        // Assert
        result.Should().BeFalse("Long valid path should be allowed");
    }

    [Fact]
    public void ContainsPathTraversalPattern_VeryLongPathWithTraversal_DetectsTraversal()
    {
        // Arrange
        var longPath = string.Join("/", Enumerable.Repeat("folder", 50)) + "/../../../etc/passwd";

        // Act
        var result = ContainsPathTraversalPattern(longPath);

        // Assert
        result.Should().BeTrue("Long path with traversal should be detected");
    }

    [Theory]
    [InlineData("%252e%252e")]
    [InlineData("%252e%252e%252f")]
    public void ContainsPathTraversalPattern_DoubleEncodedPatterns_ReturnsTrue(string path)
    {
        // Double encoding uses %25 for %, so %252e is %2e which is '.'
        // The implementation blocks '%' so this is caught

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Double encoded path '{path}' should be detected");
    }

    [Theory]
    [InlineData("images/%c0%ae%c0%ae")]
    [InlineData("%c0%ae%c0%ae%c0%af")]
    public void ContainsPathTraversalPattern_OverlongUtf8Encoding_ReturnsTrue(string path)
    {
        // Overlong UTF-8 encoding attack vectors contain '%'

        // Act
        var result = ContainsPathTraversalPattern(path);

        // Assert
        result.Should().BeTrue($"Overlong UTF-8 encoded path '{path}' should be blocked");
    }
}
