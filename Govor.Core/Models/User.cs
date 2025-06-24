using System.ComponentModel.DataAnnotations;

namespace Govor.Core.Models;

public class User
{
    public Guid Id {get; set;}
    public string Username {get; set;} 
    public string Description {get; set;} 
    public string PasswordHash {get; set;}
    public Guid IconId {get; set;} 
    public DateOnly CreatedOn {get; set;}
    public DateTime WasOnline {get; set;}
    public Guid InviteId {get; set;}
    public Invitation? Invite { get; set; }
}