using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Client_repo
{
    public interface IClient_Service : IRepository<Client>
    {
        Task CreateClientAsync(ApplicationUser user, string address, string PhoneNumber, DateTime dateTime);

        Task<Client> GetClientByCaseIdAsync(int caseId);

        Task<Client> GetClientUserByIdAsync(string userId);

        Task UpdateClientAsync(Client client);


    }
}
