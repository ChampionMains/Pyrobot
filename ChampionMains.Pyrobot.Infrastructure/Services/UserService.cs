using System;
using System.Data.Entity;
using System.Security.Claims;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Data;

namespace Hallam.RedditRankedFlairs.Services
{
    public class UserService : IUserService
    {
        protected readonly IUnitOfWork UnitOfWork;
        protected readonly IRoleService Roles;

        public UserService(IUnitOfWork unitOfWork, IRoleService roleService)
        {
            UnitOfWork = unitOfWork;
            Roles = roleService;
        }

        public Task<bool> AddAsync(User user)
        {
            UnitOfWork.Users.Add(user);
            return UnitOfWork.SaveChangesAsync().ContinueWith(x => x.Result > 0);
        }

        public async Task<User> CreateAsync(string name)
        {
            var user = new User {Name = name};

            if (await Roles.IsAdminAsync(name))
            {
                user.IsAdmin = true;
            }

            UnitOfWork.Users.Add(user);
            if (await UnitOfWork.SaveChangesAsync() == 0)
            {
                throw new InvalidOperationException("Error saving user to data store.");
            }
            return user;
        }

        public Task<ClaimsIdentity> CreateIdentityAsync(User user, string authenticationType, ClaimsIdentity externalIdentity)
        {
            var identity = new ClaimsIdentity(authenticationType, ClaimTypes.Name, ClaimTypes.Role);

            identity.AddClaims(externalIdentity.Claims);
            identity.AddClaim(new Claim(ClaimTypes.Name, user.Name, ClaimValueTypes.String));
            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.Id.ToString(), ClaimValueTypes.String));

            if (user.IsAdmin)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "admin", ClaimValueTypes.String));
            }

            if (externalIdentity.HasClaim(claim => claim.Type == "urn:reddit:moderator_of"))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "moderator", ClaimValueTypes.String));
            }

            if (user.IsBanned)
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, "banned", ClaimValueTypes.String));
            }

            return Task.FromResult(identity);
        }

        public Task<User> FindAsync(string userName)
        {
            return UnitOfWork.Users.FirstOrDefaultAsync(user => user.Name == userName);
        }

        public Task<User> FindAsync(int userId)
        {
            return Task.Run(() => UnitOfWork.Users.Find(userId));
        }

        public Task<bool> RemoveAsync(int userId)
        {
            var entity = new User {Id = userId};
            UnitOfWork.Users.Attach(entity);
            UnitOfWork.Users.Remove(entity);
            return UnitOfWork.SaveChangesAsync().ContinueWith(x => x.Result > 0);
        }
    }
}