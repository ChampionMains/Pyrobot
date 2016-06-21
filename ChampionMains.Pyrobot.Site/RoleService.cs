using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Hallam.RedditRankedFlairs.Services;

namespace Hallam.RedditRankedFlairs
{
    public class RoleService : IRoleService
    {
        private readonly ICollection<string> _admins; 

        public RoleService()
        {
            _admins = ConfigurationManager
                .AppSettings["security.admins"].Split(',').ToList();
        }

        public Task<bool> IsAdminAsync(string name)
        {
            return Task.FromResult(_admins.Contains(name, StringComparer.OrdinalIgnoreCase));
        }
    }
}