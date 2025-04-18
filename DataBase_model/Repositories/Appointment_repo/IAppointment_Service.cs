using DataAccess.Repositories;
using Law_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Appointment_repo
{
    public interface IAppointment_Service : IRepository<Appointment>
    {
         Task update(Appointment appointment);
         Task Save();
    }
}
