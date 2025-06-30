using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.Contracts.DTOs;
using Govor.Contracts.Requests;
using Govor.Core.Repositories.Invaites;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Govor.API.Controllers.AdminStuff;

[Route("api/[controller]")]
[ApiController]
//[Authorize(Roles = "Admin")]
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
    
    [HttpGet("[action]")]
    public async Task<IActionResult> GetAllActiveInvitations()
    {
        try
        {
            _logger.LogInformation("Getting all active invitations by administrator");

            var read = await _repository.GetAllAsync();
            var result = read.Where(x => x.IsActive == true).ToList();
            
            List<InvitationDto> dtos = new List<InvitationDto>();
            
            foreach (var inv in result)
            {
                dtos.Add(new InvitationDto(){
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
            var read = await _repository.GetAllAsync();

            List<InvitationDto> dtos = new List<InvitationDto>();
            
            foreach (var inv in read)
            {
                dtos.Add(new InvitationDto(){
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
            var read = await _repository.FindByIdAsync(id);
            
            var response = new InvitationDto(){
                    Id = read.Id,
                    Description = read.Description,
                    IsAdmin = read.IsAdmin, 
                    MaxParticipants = read.MaxParticipants,
                    Code = read.Code, 
                    CreatedAt = read.DateCreated,
                    EndAt = read.EndDate,
                    IsActive = read.IsActive,
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