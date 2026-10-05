namespace LabConnectPortal.Api.Infrastructure.Storage;

public class FileStorageSettings
{
    public string StorageRoot { get; set; } = "uploads";
    public string RootPath { get; set; } = "center-profiles";
    public long MaxFileSizeBytes { get; set; } = 5 * 1024 * 1024;
    public string[] AllowedExtensions { get; set; } = ["jpg", "jpeg", "png", "webp", "svg"];
}
