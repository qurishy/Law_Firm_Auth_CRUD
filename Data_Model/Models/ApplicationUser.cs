using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Law_Model.Static_file.Static_datas;

namespace Law_Model.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        [StringLength(50)]
        public string FirstName { get; set; }

        [Required]
        [StringLength(50)]
        public string LastName { get; set; }

        public UserRole Role { get; set; }

        // Additional useful properties
        public string? ProfilePicture { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Client Client { get; set; }
        public Personnel Personnel { get; set; }
    }

}
