using AutoMapper;
using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.Core.DTOs;
using Govor.Core.Repositories.Invaites;
using Govor.Core.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class InviteUserController : Controller
{
    private readonly IInvitesRepository _repository;
    private readonly IInvitationGenerator _invitationGenerator;
    private readonly ILogger<InviteUserController> _logger;
    
    public InviteUserController(IInvitationGenerator invitationGenerator,
        IInvitesRepository repository, 
        ILogger<InviteUserController> logger)
    {
        _invitationGenerator = invitationGenerator;
        _logger = logger;
        _repository = repository;
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

    [HttpGet]
    public async Task<IActionResult> GetAllInvitations()
    {
        try
        {
            _logger.LogInformation("Getting all invitations by administrator");
            var read = await _repository.GetAllAsync();

            List<InvitationDto> dto = new List<InvitationDto>();
            
            foreach (var inv in read)
            {
                dto.Add(new InvitationDto(){
                    Id = inv.Id,
                    Description = inv.Description,
                    IsAdmin = inv.IsAdmin, 
                    MaxParticipants = inv.MaxParticipants,
                    Code = inv.Code, 
                    CreatedAt = inv.DateCreated,
                    EndAt = inv.EndDate,
                    IsActive = inv.IsActive,
                });
            }
            
            return Ok(dto);
        }
        catch (Exception e)
        {
            _logger.LogError(e, e.Message);
            return BadRequest($"An error occured: {e.Message}");
        }
    }
}