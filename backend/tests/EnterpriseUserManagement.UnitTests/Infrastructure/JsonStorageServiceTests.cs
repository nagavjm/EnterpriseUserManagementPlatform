using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;
using EnterpriseUserManagement.Infrastructure.Configuration;
using EnterpriseUserManagement.Infrastructure.Persistence;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace EnterpriseUserManagement.UnitTests.Infrastructure;

public class JsonStorageServiceTests : IDisposable
{
    private readonly string _testDirectory;
    private readonly JsonStorageOptions _options;
    private readonly JsonStorageService _sut;

    public JsonStorageServiceTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), $"eump_test_{Guid.NewGuid():N}");
        _options = new JsonStorageOptions
        {
            StorageDirectory = _testDirectory,
            FileName = "test_users.json"
        };

        var optionsWrapper = Options.Create(_options);
        _sut = new JsonStorageService(optionsWrapper, NullLogger<JsonStorageService>.Instance);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            try { Directory.Delete(_testDirectory, recursive: true); } catch { /* ignore */ }
        }
    }

    [Fact]
    public async Task EnsureInitializedAsync_WhenFileDoesNotExist_ShouldCreateDirectoryAndSeedSampleUsers()
    {
        // Act
        await _sut.EnsureInitializedAsync();

        // Assert
        var filePath = _options.GetFilePath();
        Assert.True(File.Exists(filePath));

        var users = await _sut.ReadUsersAsync();
        Assert.NotEmpty(users);
        Assert.Contains(users, u => u.Email == "eleanor.vance@enterprise.com");
    }

    [Fact]
    public async Task WriteUsersAsync_ShouldPersistUsersAndCleanUpTempFile()
    {
        // Arrange
        var testUsers = new List<User>
        {
            User.Create("Alan", "Turing", "alan.turing@enterprise.com", Department.Engineering, UserRole.Admin)
        };

        // Act
        await _sut.WriteUsersAsync(testUsers);

        // Assert
        var filePath = _options.GetFilePath();
        var tempFilePath = _options.GetTempFilePath();

        Assert.True(File.Exists(filePath));
        Assert.False(File.Exists(tempFilePath)); // Temp file must be replaced/cleaned up

        var loadedUsers = await _sut.ReadUsersAsync();
        Assert.Single(loadedUsers);
        Assert.Equal("Alan", loadedUsers[0].FirstName);
        Assert.Equal("alan.turing@enterprise.com", loadedUsers[0].Email);
    }

    [Fact]
    public async Task ReadUsersAsync_WhenJsonIsCorrupt_ShouldThrowStorageCorruptedException()
    {
        // Arrange
        Directory.CreateDirectory(_testDirectory);
        var filePath = _options.GetFilePath();
        await File.WriteAllTextAsync(filePath, "INVALID_CORRUPTED_JSON_DATA{{{[[[");

        // Act & Assert
        await Assert.ThrowsAsync<StorageCorruptedException>(() => _sut.ReadUsersAsync());

        // Assert data was NOT silently wiped
        Assert.True(File.Exists(filePath));
        var content = await File.ReadAllTextAsync(filePath);
        Assert.Equal("INVALID_CORRUPTED_JSON_DATA{{{[[[", content);
    }

    [Fact]
    public void CheckHealth_WhenDirectoryWritable_ShouldReturnTrue()
    {
        // Act
        var isHealthy = _sut.CheckHealth();

        // Assert
        Assert.True(isHealthy);
    }
}

