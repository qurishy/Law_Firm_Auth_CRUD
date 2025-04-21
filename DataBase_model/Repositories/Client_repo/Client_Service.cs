using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
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
        public async Task CreateClientAsync(ApplicationUser user, string address ="america", string PhoneNumber = "123456789" , DateTime dateTime = default)
        {
         
           

            if (user != null)
            {
                // Add to Client role
                await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
                 var client = new Client();
                // Create the Client record linked to this user
                client.UserId = user.Id;
                client.Address = address;
                client.PhoneNumber = PhoneNumber;
                client.DateOfBirth = dateTime; // Set to current date for example
                client.User = user; // Don't need to add the User again as it's already in the DB

                await _db.Clients.AddAsync(client);
                await _db.SaveChangesAsync();

            }

            
        }

     

        public Task<Client> GetClientByCaseIdAsync(int caseId)
        {
            throw new NotImplementedException();
        }


        public async Task UpdateClientAsync(Client client)
        {
            throw new NotImplementedException();
        }
    }
}
