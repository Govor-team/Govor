namespace Govor.Core.Models;

public class MediaFile
{
    public Guid Id { get; set; }
    public string Url { get; set; }
    public MediaType MediaType { get; set; }
    public string MineType { get; set; }
    public DateTime DateCreated { get; set; }
}