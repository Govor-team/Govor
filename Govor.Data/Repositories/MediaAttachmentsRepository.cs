using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models.Messages;
using Govor.Core.Repositories.MediasAttachments;
using Govor.Data.Repositories.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace Govor.Data.Repositories;

public class MediaAttachmentsRepository : IMediaAttachmentsRepository
{
    public IObjectValidator<MediaAttachments> _validator;
    public GovorDbContext _context;

    public MediaAttachmentsRepository(GovorDbContext context, IObjectValidator<MediaAttachments> validator)
    {
        _context = context;
        _validator = validator;
    }
    
    public async Task<List<MediaAttachments>> GetAllAsync()
    {
        return await _context.MediaAttachments
            .AsNoTracking()
            .Include(ma => ma.Message)
            .AsSplitQuery()
            .ToListOrThrowIfEmpty(new NotFoundException("No media attachments found."));
    }

    public async Task<List<MediaAttachments>> GetAllByMessageId(Guid messageId)
    {
        return await _context.MediaAttachments
            .AsNoTracking()
            .Include(ma => ma.Message)
            .AsSplitQuery()
            .Where(m => m.MessageId == messageId)
            .ToListOrThrowIfEmpty(new NotFoundByKeyException<Guid>(messageId, "No media attachments found by given message Id"));
    }

    public async Task<MediaAttachments> FindByIdAsync(Guid id)
    {
        return await _context.MediaAttachments
            .AsNoTracking()
            .Include(ma => ma.Message)
            .AsSplitQuery()
            .FirstOrDefaultAsync(m => m.Id == id)
            ?? throw new NotFoundByKeyException<Guid>(id, "No media attachments found by given Id");
    }
    
    public async Task AddAsync(MediaAttachments mediaAttachments)
    {
        try
        {
            _validator.Validate(mediaAttachments);

            _context.MediaAttachments.Add(mediaAttachments);
            await _context.SaveChangesAsync();
        }
        catch (InvalidObjectException<Message> ex)
        {
            throw new AdditionException("Attachments with given data invalid", ex);
        }
        catch (Exception ex)
        {
            throw new AdditionException("Cannot add Attachments", ex);
        }
    }

    public async Task UpdateAsync(MediaAttachments attachments)
    {
        try
        {
            _validator.Validate(attachments);

            var rowsAffected = await _context.MediaAttachments
                .Where(m => m.Id == attachments.Id)
                .ExecuteUpdateAsync(u => u
                    .SetProperty(m => m.MessageId, attachments.MessageId)
                    .SetProperty(m => m.Message, attachments.Message)
                    .SetProperty(m => m.MediaFileId, attachments.MediaFileId)
                );

            if (rowsAffected == 0)
                throw new UpdateException($"Not found attachments by given id {attachments.Id}");
        }
        catch (Exception ex)
        {
            throw new UpdateException($"Error when updating the attachments {attachments.Id}", ex);
        }
    }

    public async Task RemoveAsync(Guid Id)
    {
        try
        {
            var result = await FindByIdAsync(Id);

            _context.MediaAttachments.Remove(result);
            await _context.SaveChangesAsync();
        }
        catch (NotFoundByKeyException<Guid> ex)
        {
            throw new RemoveException($"Not found attachments by given id {Id}", ex);
        }
        catch (Exception ex)
        {
            throw new RemoveException("Error when removing the attachments", ex);
        }
    }

    public bool Exist(Guid id)
    {
        return _context.MediaAttachments.Any(e => e.Id == id);
    }

    public bool Exist(MediaAttachments attachments)
    {
        _validator.Validate(attachments);
        
        return _context.MediaAttachments.Any(
            e => e.Id == attachments.Id &&
            e.MessageId == attachments.MessageId &&
            e.MediaFileId == attachments.MediaFileId
            );
    }
}