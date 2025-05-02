using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Appointment_repo
{
    public interface IAppointment_Service : IRepository<Appointment>
    {


        Task CreateAppointment(Appointment appointment);

        Task<Appointment> AppointmentByCaseId(int casseId);

        Task<IEnumerable<Appointment>> GetAllAppointmentsByUserIdClient(string userId);

        Task<IEnumerable<Appointment>> GetAllAppointmentsByUserIdLawyer(string userId);

        Task <bool> DeleteAppointmentAsync(int id);

        Task update(Appointment appointment);



    }
}
