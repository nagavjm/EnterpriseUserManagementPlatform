namespace EnterpriseUserManagement.Infrastructure.Configuration;

public class JsonStorageOptions
{
    public const string SectionName = "JsonStorage";

    /// <summary>
    /// The directory where the JSON file is stored.
    /// Can be relative to the application base directory or an absolute path (e.g. for Docker / Kubernetes PVC mounts).
    /// </summary>
    public string StorageDirectory { get; set; } = "data";

    /// <summary>
    /// The name of the JSON storage file. Default is 'users.json'.
    /// </summary>
    public string FileName { get; set; } = "users.json";

    /// <summary>
    /// Resolves the absolute path to the target JSON persistence file.
    /// </summary>
    public string GetFilePath()
    {
        var directory = Path.IsPathRooted(StorageDirectory)
            ? StorageDirectory
            : Path.Combine(AppContext.BaseDirectory, StorageDirectory);

        return Path.Combine(directory, FileName);
    }

    /// <summary>
    /// Resolves the absolute path to the temporary JSON file used for atomic safe-writes.
    /// </summary>
    public string GetTempFilePath()
    {
        return $"{GetFilePath()}.tmp";
    }
}

