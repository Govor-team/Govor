using Govor.Domain;

namespace Govor.Application.Exceptions.InvitesService;

public class InviteLinkInvalidException(string inviteCode) : GovorCoreException($"Invite link invalid: {inviteCode}");