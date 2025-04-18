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

namespace DATA.Repositories.Lawyer_repo
{
    public class Lawyer_Service : Repository<Personnel>, ILawyer_Service
    {
        private readonly AplicationDB _db;
        private readonly UserManager<ApplicationUser> _userManager;

        public Lawyer_Service(AplicationDB db, UserManager<ApplicationUser> userManager) : base(db)
        {
            _db = db;
            _userManager=userManager;
        }

        //we are creating a new personnel
        public async Task<IdentityResult> CreatePersonnelAsync(Personnel personnel, string password)
        {
            var user = new ApplicationUser
            {
                UserName = personnel.User.Email,
                Email = personnel.User.Email,
                FirstName = personnel.User.FirstName,
                LastName = personnel.User.LastName,
                Role = UserRole.Lawyer //  role is appropriate
            };

            var result = await _userManager.CreateAsync(user, password);

            if (result.Succeeded)
            {
                // Add to appropriate role
                await _userManager.AddToRoleAsync(user, user.Role.ToString());

                // Create the Personnel record linked to this user
                personnel.UserId = user.Id;
                personnel.User = user; // Don't need to add the User again as it's already in the DB

                
                _db.Personnel.Add(personnel);
                
            }

            return result;

           
        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();

        }

        public async Task UpdatePersonnel(Personnel personnel)
        {
            throw new NotImplementedException();
        }

    }
}
