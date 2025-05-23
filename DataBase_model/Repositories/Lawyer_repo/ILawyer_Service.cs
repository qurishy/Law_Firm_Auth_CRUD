﻿using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Lawyer_repo
{
    public interface ILawyer_Service : IRepository<Personnel>
    {
        Task CreatePersonnelAsync(ApplicationUser user, string Position, string Department);

        Task<Personnel?> GetPersonnelByCaseIdAsync(int caseId);

        Task<Personnel?> GetPersonnelUserByIdAsync(string userId);

        Task<IEnumerable<Personnel>> GetAllLawyersAsync();
        Task UpdatePersonnel(Personnel personnel);





    }
}
