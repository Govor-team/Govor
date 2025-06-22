using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
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
            .ToListOrThrowIfEmpty(new NotFoundException("No media attachments found."));
    }

    public Task<List<MediaAttachments>> GetAllByMessageId(Guid messageId)
    {
        throw new NotImplementedException();
    }

    public Task AddAsync(MediaAttachments mediaAttachments)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAsync(MediaAttachments mediaAttachments)
    {
        throw new NotImplementedException();
    }

    public Task RemoveAsync(Guid Id)
    {
        throw new NotImplementedException();
    }

    public bool Exists(Guid id)
    {
        throw new NotImplementedException();
    }

    public bool Exists(MediaAttachments attachments)
    {
        throw new NotImplementedException();
    }
}