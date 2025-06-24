using Govor.API.Services;
using Govor.API.Services.AdminsStuff;
using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.API.Services.Authentication;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Repositories.Admins;
using Govor.Core.Repositories.Invaites;
using Govor.Core.Repositories.MediasAttachments;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.Users;
using Govor.Data;
using Govor.Data.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Govor.API.Extensions;

public static class ConfigurationProgramExtensions
{
    public static void AddServices(this IServiceCollection services)
    {
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IAccountService, AuthService>();
        services.AddScoped<IUsersAdministration, UsersService>();
        services.AddScoped<IInvitesService, InvitesService>();
        services.AddScoped<IInvitationGenerator, InvitationGenerator>();
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IMessagesRepository, MessagesRepository>();
        services.AddScoped<IInvitesRepository, InvitesRepository>();
        services.AddScoped<IAdminsRepository, AdminsRepository>();
        services.AddScoped<IMediaAttachmentsRepository, MediaAttachmentsRepository>();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IObjectValidator<User>, UserValidator>();
        services.AddScoped<IObjectValidator<Message>, MessageValidator>();
        services.AddScoped<IObjectValidator<MediaAttachments>, MediaAttachmentsValidator>();
        services.AddScoped<IObjectValidator<Admin>, AdminValidator>();
        services.AddScoped<IObjectValidator<Invitation>, InvitationValidator>();
    }

    public static void AddGovorDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<GovorDbContext>(
            options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(GovorDbContext)));
            }
        );
    }
}