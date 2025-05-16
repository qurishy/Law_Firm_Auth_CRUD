using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using Microsoft.EntityFrameworkCore;

namespace DATA.Repositories.Appointment_repo
{
    public class Appointment_Service : Repository<Appointment>, IAppointment_Service
    {
        private readonly AplicationDB _db;
        public Appointment_Service(AplicationDB db) : base(db)
        {
            _db = db;

        }


        //used for getting appointment by case id
        public async Task<Appointment> AppointmentByCaseId(int caseId)
        {
            return await _db.Appointments
                .Include(a => a.Case) // Include the related LegalCase
                .FirstOrDefaultAsync(a => a.CaseId == caseId); // Filter by CaseId


        }




        //used for getting all appointment by user id for client
        public async Task<IEnumerable<Appointment>> GetAllAppointmentsByUserIdClient(string userId)
        {
            return await _db.Appointments
                .Include(a => a.Case)
                .ThenInclude(c => c.Client)
                .Where(a => a.Case.Client.UserId == userId)
                .ToListAsync();
        }



        //used for getting all appointment by user id for lawyer
        public async Task<IEnumerable<Appointment>> GetAllAppointmentsByUserIdLawyer(string userId)
        {
            return await _db.Appointments
                .Include(a => a.Case)
                .ThenInclude(c => c.AssignedLawyer)
                .Where(a => a.Case.AssignedLawyer.UserId == userId)
                .ToListAsync();
        }
        //================================================================================================

        //used for creating appointment
        public async Task CreateAppointment(Appointment appointment)
        {
            try
            {

                await _db.Appointments.AddAsync(appointment);
                await _db.SaveChangesAsync();

            }
            catch (Exception ex)
            {
                throw new Exception("Error creating appointment: " + ex.Message);
            }
        }






        //used for updating appointment
        public async Task update(Appointment appointment)
        {
            try
            {
                var existingAppointment = await _db.Appointments.FirstOrDefaultAsync(a => a.Id == appointment.Id);
                if (existingAppointment == null)
                {
                    throw new Exception("Appointment not found.");
                }

                // Update the properties of the existing appointment
                existingAppointment.Title = appointment.Title;
                existingAppointment.ScheduledTime = appointment.ScheduledTime;
                existingAppointment.Notes = appointment.Notes;
                existingAppointment.CaseId = appointment.CaseId;
                existingAppointment.IsCompleted = appointment.IsCompleted;

                _db.Appointments.Update(existingAppointment);
                await _db.SaveChangesAsync();
                return;
            }
            catch (Exception ex)
            {
                throw new Exception("Error updating appointment: " + ex.Message);
            }
        }



        //used for deleting appointment
        public async Task<bool> DeleteAppointmentAsync(int id)
        {
            if(id == null && id == 0)
            {
                throw new ArgumentNullException(nameof(id), "Id cannot be null or zero.");
            }

            Appointment appointment = await _db.Appointments.FindAsync(id);

            if (appointment != null)
            {
                _db.Appointments.Remove(appointment);
            
                await _db.SaveChangesAsync();

                return true;
               
            }

             return false;
        }
    }
}
