using DataAccess.Repositories;
using Law_Model.Models;

namespace DATA.Repositories.Appointment_repo
{
    public interface IAppointment_Service : IRepository<Appointment>
    {
        Task update(Appointment appointment);
        Task Save();
    }
}
