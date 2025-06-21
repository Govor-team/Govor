using Microsoft.AspNetCore.Http; // для IFormFile

namespace Govor.API.Services;

public interface IStorageService
{
    Task<string> SaveAsync(byte[] data, string fileName);
    Task<Stream> LoadAsync(string url);
    Task RemoveAsync(string url);
}