using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Lawyer_repo
{
    public interface ILawyer_Service : IRepository<Personnel>
    {
        Task<ApplicationUser> CreatePersonnelAsync(Personnel personnel, string password);

        Task<Personnel> GetPersonnelByCaseIdAsync(int caseId);


        Task UpdatePersonnel(Personnel personnel);

        Task Save();



    }
}
