using Govor.Core.Models;

namespace Govor.Core.Infrastructure.Validators;

public class FriendshipValidator : IObjectValidator<Friendship>
{
    public void Validate(Friendship friendship)
    {
        try
        {
            if(friendship is null)
                throw new ArgumentNullException(nameof(friendship));
            if(friendship.Id == Guid.Empty)
                throw new ArgumentException("Id cannot be empty", nameof(friendship.Id));
            if(friendship.AddresseeId == Guid.Empty)
                throw new ArgumentException("Addresses cannot be empty", nameof(friendship.AddresseeId));
            if(friendship.RequesterId == Guid.Empty)
                throw new ArgumentException("Requester cannot be empty", nameof(friendship.RequesterId));
        }
        catch (Exception e)
        {
            throw new InvalidObjectException<Friendship>(e);
        }
    }

    public bool TryValidate(Friendship friendship)
    {
        try
        {
            Validate(friendship);
            return true;
        }
        catch
        {
            return false;
        }
    }
}