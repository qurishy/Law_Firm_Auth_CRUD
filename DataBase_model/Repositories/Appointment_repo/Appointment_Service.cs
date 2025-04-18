using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Appointment_repo
{
    public class AppointmentService : Repository<Appointment>, IAppointment_Service
    {
        private readonly AplicationDB _db;
        public AppointmentService(AplicationDB db) : base(db)
        {
            _db = db;
            
        }

        public async Task Save()
        {
          await  _db.SaveChangesAsync();
            
        }

        public Task update(Appointment appointment)
        {
            throw new NotImplementedException();
        }
    }
}
