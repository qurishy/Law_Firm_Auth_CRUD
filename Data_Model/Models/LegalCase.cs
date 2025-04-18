﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using static Law_Model.Static_file.Static_datas;

namespace Law_Model.Models
{
    public class LegalCase
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        public string Description { get; set; }

        public CaseType Type { get; set; }

        public CaseStatus Status { get; set; }

        public DateTime OpenDate { get; set; } = DateTime.UtcNow;

        public DateTime? CloseDate { get; set; }

        

        // Relationships
        public int ClientId { get; set; }
        public Client Client { get; set; }

        public int? AssignedLawyerId { get; set; }
        public Personnel AssignedLawyer { get; set; }


        // Make it possible to have a team assigned to a case
        public ICollection<Documented> Documents { get; set; }
     
        public ICollection<Appointment> Appointments { get; set; }
       
    }
}
