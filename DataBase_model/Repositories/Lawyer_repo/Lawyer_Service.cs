using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
        public async Task CreatePersonnelAsync(ApplicationUser user, string Position, string Department)
        {
            if (user != null)
            {
                // Add to Client role
                await _userManager.AddToRoleAsync(user, UserRole.Lawyer.ToString());
                Personnel lawyer = new Personnel();
                // Create the Client record linked to this user
                lawyer.UserId = user.Id;
                lawyer.User = user;
                lawyer.Position = Position;
                lawyer.Department = Department;
                lawyer.HireDate = DateTime.UtcNow; // Set to current date for example
                // Don't need to add the User again as it's already in the DB

                await _db.Personnel.AddAsync(lawyer);
                await _db.SaveChangesAsync();

            }


        }


        public async Task<Personnel?> GetPersonnelUserByIdAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                throw new ArgumentException("User ID cannot be null or empty", nameof(userId));
            }

            try
            {
                return await _db.Personnel
                    .Include(p => p.User) // Include related ApplicationUser

                    .FirstOrDefaultAsync(p => p.UserId == userId);
            }
            catch (Exception ex)
            {

                throw;
            }
        }


        public async Task<Personnel?> GetPersonnelByCaseIdAsync(int caseId)
        {
            try
            {
                // Option 2: Query from Case side (if you want to get specifically the assigned lawyer)
                var legalCase = await _db.LegalCases
                    .Include(c => c.AssignedLawyer)
                        .ThenInclude(l => l.User) // Include the lawyer's user details
                    .FirstOrDefaultAsync(c => c.Id == caseId);

                return legalCase?.AssignedLawyer;

            }
            catch (Exception ex)
            {

                throw;
            }
        }

        public async Task UpdatePersonnel(Personnel personnel)
        {
            if (personnel == null)
            {
                throw new ArgumentNullException(nameof(personnel));
            }

            try
            {
                _db.Personnel.Update(personnel);
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                throw;
            }

        }

        public async Task Save()
        {
            await _db.SaveChangesAsync();
        }


    }
}
