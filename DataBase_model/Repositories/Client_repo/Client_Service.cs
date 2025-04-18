using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Law_Model.Static_file.Static_datas;

namespace DATA.Repositories.Client_repo
{
    public class Client_Service : Repository<Client>, IClient_Service
    {
        private readonly AplicationDB _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public Client_Service(AplicationDB db, UserManager<ApplicationUser> userManager) : base(db)
        {
            _db = db;
            _userManager=userManager;
        }
        public async Task<IdentityResult> CreateClientAsync(Client client, string password)
        {
            // Create the ApplicationUser first
            var user = new ApplicationUser
            {
                UserName = client.User.Email,
                Email = client.User.Email,
                FirstName = client.User.FirstName,
                LastName = client.User.LastName,
                Role = UserRole.Client
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Add to Client role
                await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());

                // Create the Client record linked to this user
                client.UserId = user.Id;
                client.User = null; // Don't need to add the User again as it's already in the DB

                _db.Clients.AddAsync(client);
                
            }

            return result;
        }

        public async Task UpdateClient(Client client)
        {
            throw new NotImplementedException();
        }
    }
}
