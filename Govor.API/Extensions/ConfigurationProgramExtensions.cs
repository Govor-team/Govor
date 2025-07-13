using Govor.API.Services.AdminsStuff.Interfaces;
using Govor.API.Services.Authentication.Interfaces;
using Govor.Application.Infrastructure.AdminsStuff;
using Govor.Application.Infrastructure.Extensions;
using Govor.Application.Infrastructure.Validators;
using Govor.Application.Interfaces;
using Govor.Application.Interfaces.Authentication;
using Govor.Application.Interfaces.Friends;
using Govor.Application.Interfaces.Infrastructure.Extensions;
using Govor.Application.Interfaces.Medias;
using Govor.Application.Interfaces.Messages;
using Govor.Application.Services;
using Govor.Application.Services.Authentication;
using Govor.Application.Services.Friends;
using Govor.Application.Services.Messages;
using Govor.Core.Infrastructure.Extensions;
using Govor.Core.Infrastructure.Validators;
using Govor.Core.Models;
using Govor.Core.Models.Messages;
using Govor.Core.Models.Users;
using Govor.Core.Repositories.Admins;
using Govor.Core.Repositories.Friendships;
using Govor.Core.Repositories.Groups;
using Govor.Core.Repositories.Invaites;
using Govor.Core.Repositories.MediasAttachments;
using Govor.Core.Repositories.Messages;
using Govor.Core.Repositories.PrivateChats;
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
        services.AddScoped<IUsernameValidator, UsernameValidator>();
        
        // Friends services 
        services.AddScoped<IFriendshipService, FriendshipService>();
        services.AddScoped<IFriendRequestCommandService, FriendRequestCommandService>();
        services.AddScoped<IFriendRequestQueryService, FriendRequestQueryService>();
        services.AddScoped<IFriendsBlockService, FriendsBlockService>();
        
        services.AddScoped<IStorageService>(sp =>
        {
            var env = sp.GetRequiredService<IWebHostEnvironment>();
            return new LocalStorageService(env.ContentRootPath);
        });
        
        services.AddHttpContextAccessor(); // it's very important for CurrentUserService
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        
        services.AddMemoryCache();
        services.AddScoped<IPingHandlerService, PingHandlerService>();
        
        services.AddScoped<IMessageCommandService, MessageCommandService>();
        services.AddScoped<IVerifyFriendship, VerifyFriendship>();
        services.AddScoped<IUserGroupsService, UserGroupsService>();
        services.AddScoped<IMessagesLoader, MessagesLoader>();
        services.AddScoped<IMediaService, MediaService>();
        
        // Auto Mapper 
        services.AddAutoMapper(typeof(MappingProfile));
    }

    public static void AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUsersRepository, UsersRepository>();
        services.AddScoped<IMessagesRepository, MessagesRepository>();
        services.AddScoped<IInvitesRepository, InvitesRepository>();
        services.AddScoped<IAdminsRepository, AdminsRepository>();
        services.AddScoped<IMediaAttachmentsRepository, MediaAttachmentsRepository>();
        services.AddScoped<IFriendshipsRepository, FriendshipsRepository>();
        services.AddScoped<IPrivateChatsRepository, PrivateChatsRepository>();
        services.AddScoped<IGroupsRepository, GroupRepository>();
        
        // other
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IObjectValidator<User>, UserValidator>();
        services.AddScoped<IObjectValidator<Message>, MessageValidator>();
        services.AddScoped<IObjectValidator<MediaAttachments>, MediaAttachmentsValidator>();
        services.AddScoped<IObjectValidator<Admin>, AdminValidator>();
        services.AddScoped<IObjectValidator<Invitation>, InvitationValidator>();
        services.AddScoped<IObjectValidator<Friendship>, FriendshipValidator>();
        services.AddScoped<IObjectValidator<PrivateChat>, PrivateChatValidator>();
        services.AddScoped<IObjectValidator<ChatGroup>, ChatGroupValidator>();
    }

    public static void AddGovorDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        var useMySql = configuration.GetValue<bool>("UseMySql");

        if (useMySql)
        {
            services.AddDbContext<GovorDbContext>(options =>
            {
                options
                    .UseMySql(
                        configuration.GetConnectionString(nameof(GovorDbContext)),
                        new MySqlServerVersion(new Version(8, 0, 21))
                    );
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }
        else
        {
            services.AddDbContext<GovorDbContext>(options =>
            {
                options.UseNpgsql(configuration.GetConnectionString(nameof(GovorDbContext)));
                options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            });
        }
    }


}