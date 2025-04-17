using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law_Model.Models
{
    public class Client
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign Key to ApplicationUser

        public ApplicationUser User { get; set; }

        [Required]
        public string Address { get; set; }


        public string? PhoneNumber { get; set; }

        public DateTime DateOfBirth { get; set; }


        // Navigation properties
        public ICollection<LegalCase> Cases { get; set; }
      
    }
}

