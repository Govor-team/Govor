using Govor.Application.Infrastructure.AdminsStuff;
using Govor.Contracts.DTOs;
using Govor.Contracts.Requests;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[Route("api/admin/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]
public class InviteUserController : Controller
{
    private readonly IInvitationGetter _invitationGetter;
    private readonly IInvitationGenerator _invitationGenerator;
    private readonly ILogger<InviteUserController> _logger;
    
    public InviteUserController(IInvitationGenerator invitationGenerator,
        IInvitationGetter invitationGetter, 
        ILogger<InviteUserController> logger)
    {
        _invitationGenerator = invitationGenerator;
        _logger = logger;
        _invitationGetter = invitationGetter;
    }
    
    [HttpPost("[action]")]
    public async Task<IActionResult> Invitation([FromBody] CreateInvitationRequest createInvitation)
    {
        try
        {
            var result = await _invitationGenerator.GenerateInvitationCode(createInvitation.EndDate,
                createInvitation.MaxParticipants, 
                createInvitation.IsAdmin,
                createInvitation.Description);
       
            return Ok(result);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest($"An error occured: {e.Message}");
        }
    }
    
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllActiveInvitations()
    {
        try
        {
            _logger.LogInformation("Getting all active invitations by administrator");

            var read = await _invitationGetter.GetAllAsync();
            var result = read.Where(x => x.IsActive).ToList();
            
            List<InvitationResponses> dtos = new List<InvitationResponses>();
            
            foreach (var inv in result)
            {
                dtos.Add(new InvitationResponses(){
                    Id = inv.Id,
                    Description = inv.Description,
                    IsAdmin = inv.IsAdmin, 
                    MaxParticipants = inv.MaxParticipants,
                    Code = inv.Code, 
                    CreatedAt = inv.DateCreated,
                    EndAt = inv.EndDate,
                    IsActive = inv.IsActive,
                    ParticipantCount = inv.Users.Count,
                });
            }
            
            return Ok(dtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest($"An error occured: {e.Message}");
        }
    }
    
    [HttpGet]
    public async Task<IActionResult> GetAllInvitations()
    {
        try
        {
            _logger.LogInformation("Getting all invitations by administrator");
            var read = await _invitationGetter.GetAllAsync();

            List<InvitationResponses> dtos = new List<InvitationResponses>();
            
            foreach (var inv in read)
            {
                dtos.Add(new InvitationResponses(){
                    Id = inv.Id,
                    Description = inv.Description,
                    IsAdmin = inv.IsAdmin, 
                    MaxParticipants = inv.MaxParticipants,
                    Code = inv.Code, 
                    CreatedAt = inv.DateCreated,
                    EndAt = inv.EndDate,
                    IsActive = inv.IsActive,
                    ParticipantCount = inv.Users.Count,
                });
            }
            
            return Ok(dtos);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest($"An error occured: {e.Message}");
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetInvitationById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting invitations {id} by administrator");
            var read = await _invitationGetter.FindByIdAsync(id);
            
            if(read.IsFailure)
                return NotFound(read.Error);
            var dto = read.Value;
            
            var response = new InvitationResponses(){
                Id = dto.Id,
                Description = dto.Description,
                IsAdmin = dto.IsAdmin, 
                MaxParticipants = dto.MaxParticipants,
                Code = dto.Code, 
                CreatedAt = dto.DateCreated,
                EndAt = dto.EndDate,
                IsActive = dto.IsActive,
                ParticipantCount = dto.Users.Count,
            };
            
            return Ok(response);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest($"An error occured: {e.Message}");
        }
    }
}