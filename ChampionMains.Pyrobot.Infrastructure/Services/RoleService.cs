using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChampionMains.Pyrobot.Services
{
    public class RoleService
    {
        private readonly ICollection<string> _admins; 

        public RoleService(ICollection<string> admins)
        {
            _admins = admins;
        }

        public Task<bool> IsAdminAsync(string name)
        {
            return Task.FromResult(_admins.Contains(name, StringComparer.OrdinalIgnoreCase));
        }
    }
}