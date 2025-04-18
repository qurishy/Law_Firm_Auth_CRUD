using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Lawyer_repo
{
    public interface ILawyer_Service : IRepository<Personnel>
    {
        Task<IdentityResult> CreatePersonnelAsync(Personnel personnel, string password);
        
        
        Task UpdatePersonnel(Personnel personnel);

        Task Save();
     


    }
}
