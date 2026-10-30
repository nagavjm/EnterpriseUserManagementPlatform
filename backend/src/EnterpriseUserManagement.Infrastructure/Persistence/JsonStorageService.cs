using System.Text.Json;
using System.Text.Json.Serialization;
using EnterpriseUserManagement.Application.Interfaces;
using EnterpriseUserManagement.Domain.Entities;
using EnterpriseUserManagement.Domain.Enums;
using EnterpriseUserManagement.Domain.Exceptions;
using EnterpriseUserManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EnterpriseUserManagement.Infrastructure.Persistence;

public class JsonStorageService : IJsonStorageService
{
    private readonly JsonStorageOptions _options;
    private readonly ILogger<JsonStorageService> _logger;
    private readonly SemaphoreSlim _fileLock = new(1, 1);

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    public JsonStorageService(
        IOptions<JsonStorageOptions> options,
        ILogger<JsonStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task EnsureInitializedAsync(CancellationToken cancellationToken = default)
    {
        var filePath = _options.GetFilePath();
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            _logger.LogInformation("Creating JSON storage directory: {Directory}", directory);
            Directory.CreateDirectory(directory);
        }

        if (!File.Exists(filePath))
        {
            _logger.LogInformation("JSON storage file not found at {FilePath}. Seeding initial sample enterprise users...", filePath);
            var seedUsers = GetSampleUsers();
            await WriteUsersInternalAsync(seedUsers, cancellationToken);
            _logger.LogInformation("Successfully initialized {FilePath} with {Count} sample users.", filePath, seedUsers.Count);
        }
    }

    public async Task<List<User>> ReadUsersAsync(CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            await EnsureInitializedAsync(cancellationToken);

            var filePath = _options.GetFilePath();
            if (!File.Exists(filePath))
            {
                return new List<User>();
            }

            try
            {
                await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);
                var users = await JsonSerializer.DeserializeAsync<List<User>>(stream, SerializerOptions, cancellationToken);
                return users ?? new List<User>();
            }
            catch (JsonException ex)
            {
                _logger.LogCritical(ex, "FATAL: Corrupted JSON data detected in {FilePath}. Data will NOT be reset.", filePath);
                throw new StorageCorruptedException(
                    filePath, 
                    $"The user data store at '{filePath}' is corrupted and could not be deserialized. Please inspect the file manually.", 
                    ex);
            }
            catch (Exception ex) when (ex is not StorageCorruptedException)
            {
                _logger.LogError(ex, "Failed to read user data from {FilePath}.", filePath);
                throw;
            }
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task WriteUsersAsync(List<User> users, CancellationToken cancellationToken = default)
    {
        await _fileLock.WaitAsync(cancellationToken);
        try
        {
            await WriteUsersInternalAsync(users, cancellationToken);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task WriteUsersInternalAsync(List<User> users, CancellationToken cancellationToken)
    {
        var filePath = _options.GetFilePath();
        var tempFilePath = _options.GetTempFilePath();
        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        try
        {
            // 1. Write the new payload to a temporary file
            await using (var tempStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, useAsync: true))
            {
                await JsonSerializer.SerializeAsync(tempStream, users, SerializerOptions, cancellationToken);
                await tempStream.FlushAsync(cancellationToken);
            }

            // 2. Atomically replace the destination file with the temp file
            File.Move(tempFilePath, filePath, overwrite: true);
            _logger.LogDebug("Successfully persisted {Count} users to {FilePath} via atomic safe swap.", users.Count, filePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during safe atomic write to {FilePath}.", filePath);
            // Clean up temporary file if write failed
            if (File.Exists(tempFilePath))
            {
                try { File.Delete(tempFilePath); } catch { /* ignore temp cleanup error */ }
            }
            throw;
        }
    }

    public bool CheckHealth()
    {
        try
        {
            var filePath = _options.GetFilePath();
            var directory = Path.GetDirectoryName(filePath);

            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            // Check read/write permission
            var probeFile = Path.Combine(directory ?? AppContext.BaseDirectory, $".health_probe_{Guid.NewGuid():N}.tmp");
            File.WriteAllText(probeFile, "health-check");
            File.Delete(probeFile);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "JSON storage health check probe failed.");
            return false;
        }
    }

    private static List<User> GetSampleUsers()
    {
        return new List<User>
        {
            new()
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                FirstName = "Eleanor",
                LastName = "Vance",
                Email = "eleanor.vance@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Admin,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-6),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                FirstName = "Marcus",
                LastName = "Brody",
                Email = "marcus.brody@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-5),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                FirstName = "Sophia",
                LastName = "Chen",
                Email = "sophia.chen@enterprise.com",
                Department = Department.Engineering,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-4),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                FirstName = "David",
                LastName = "Miller",
                Email = "david.miller@enterprise.com",
                Department = Department.HR,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-3),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                FirstName = "Rachel",
                LastName = "Green",
                Email = "rachel.green@enterprise.com",
                Department = Department.HR,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-2),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                FirstName = "Alexander",
                LastName = "Hamilton",
                Email = "alexander.hamilton@enterprise.com",
                Department = Department.Finance,
                Role = UserRole.Manager,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddMonths(-2),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                FirstName = "Jordan",
                LastName = "Belfort",
                Email = "jordan.belfort@enterprise.com",
                Department = Department.Sales,
                Role = UserRole.Employee,
                IsActive = false,
                CreatedDate = DateTime.UtcNow.AddMonths(-1),
                ModifiedDate = DateTime.UtcNow.AddDays(-10)
            },
            new()
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                FirstName = "Amelia",
                LastName = "Earhart",
                Email = "amelia.earhart@enterprise.com",
                Department = Department.Operations,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-20),
                ModifiedDate = null
            },
            new()
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                FirstName = "Lucas",
                LastName = "Scott",
                Email = "lucas.scott@enterprise.com",
                Department = Department.Support,
                Role = UserRole.Employee,
                IsActive = true,
                CreatedDate = DateTime.UtcNow.AddDays(-5),
                ModifiedDate = null
            }
        };
    }
}

