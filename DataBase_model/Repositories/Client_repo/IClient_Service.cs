using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Client_repo
{
    public interface IClient_Service : IRepository<Client>
    {
        Task<ApplicationUser> CreateClientAsync(Client client, string password);
     
        Task UpdateClient(Client client);

    }
}
