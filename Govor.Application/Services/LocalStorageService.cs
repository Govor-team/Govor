using Govor.Application.Interfaces;
namespace Govor.Application.Services;

public class LocalStorageService : IStorageService
{
    private readonly string _storagePath;
    
    public LocalStorageService(string contentRootPath)
    {
        _storagePath = Path.Combine(contentRootPath, "uploads");
        
        if (!Directory.Exists(_storagePath))
        {
            Directory.CreateDirectory(_storagePath);
        }
    }
    
    public async Task<string> SaveAsync(byte[] data, string fileName)
    {
        if(data is null || data.Length == 0) 
            throw new ArgumentException("Invalid file", nameof(data));
        
        if(fileName is null || fileName.Length == 0)
            throw new ArgumentException("Invalid file name", nameof(fileName));
        
        var date = DateTime.UtcNow.ToString("yyyy/MM");
        
        var folder = Path.Combine(_storagePath, date);
        Directory.CreateDirectory(folder);
        
        var uniqueFileName = $"{Guid.NewGuid()}_{Path.GetFileName(fileName)}";
        var fullPath = Path.Combine(folder, uniqueFileName);
        
        await using var stream = new FileStream(fullPath, FileMode.Create);
        stream.WriteAsync(data, 0, data.Length);
        
        return Path.Combine(date, uniqueFileName);
    }

    public async Task<Stream> LoadAsync(string url)
    {
        var filePath = Path.Combine(_storagePath, url); 

        if (!File.Exists(filePath))
            throw new FileNotFoundException("File not found.", filePath);

        return new FileStream(filePath, FileMode.Open, FileAccess.Read);
    }

    public async Task RemoveAsync(string url)
    {
        var path = Path.Combine(_storagePath, url);
        
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        
        await Task.CompletedTask;
    }
}
