namespace Govor.Application.Interfaces;

public interface IStorageService
{
    Task<string> SaveAsync(byte[] data, string fileName);
    Task<Stream> LoadAsync(string url);
    Task RemoveAsync(string url);
}