using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law_Model.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public DateTime ScheduledTime { get; set; }
        public string Notes { get; set; }

        public int CaseId { get; set; }
        public LegalCase Case { get; set; }
    }
}
