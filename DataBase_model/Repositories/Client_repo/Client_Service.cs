using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

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


        //we are creating a client and adding it to the database getting the user from the registration page
        public async Task CreateClientAsync(ApplicationUser user, string address = "america", string PhoneNumber = "123456789", DateTime dateTime = default)
        {



            if (user != null)
            {
                // Add to Client role
                // await _userManager.AddToRoleAsync(user, UserRole.Client.ToString());
                var client = new Client();
                // Create the Client record linked to this user
                client.UserId = user.Id;
                client.User=user;
                client.Address = address;
                client.PhoneNumber = PhoneNumber;
                client.DateOfBirth = dateTime; // Set to current date for example
                client.User = user; // Don't need to add the User again as it's already in the DB

                await _db.Clients.AddAsync(client);
                await _db.SaveChangesAsync();

            }


        }


        //we are getting the client from the database with the case id and case details
        public async Task<Client> GetClientByCaseIdAsync(int caseId)
        {
            try
            {
                // Option 2: Query from Case side (if you want to get specifically the assigned client)
                var legalCase = await _db.LegalCases
                    .Include(c => c.Client)
                        .ThenInclude(l => l.User) // Include the lawyer's user details
                    .FirstOrDefaultAsync(c => c.Id == caseId);

                return legalCase?.Client;

            }
            catch (Exception ex)
            {

                throw;
            }
        }


        //we are getting the client from the database with the user id and user details
        public async Task<Client> GetClientUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }



            try
            {
                return await _db.Clients
                    .Include(p => p.User) // Include related ApplicationUser
                    .Include(p => p.Cases)
                    .FirstOrDefaultAsync(p => p.UserId == userId);
            }
            catch (Exception ex)
            {

                throw;
            }
        }

        //we are updating the client in the database with the new cleient model
        public async Task UpdateClientAsync(Client client)
        {
            if (client == null)
            {
                throw new ArgumentNullException(nameof(client), "Client cannot be null");
            }

            var existingClient = await _db.Clients.FirstOrDefaultAsync(c => c.Id == client.Id);

            if (existingClient == null)
            {
                throw new InvalidOperationException($"Client with ID {client.Id} does not exist");
            }

            // Update properties
            existingClient.Address = client.Address;
            existingClient.PhoneNumber = client.PhoneNumber;
            existingClient.DateOfBirth = client.DateOfBirth;

            // Save changes to the database
            _db.Clients.Update(existingClient);
            await _db.SaveChangesAsync();
        }
    }
}
