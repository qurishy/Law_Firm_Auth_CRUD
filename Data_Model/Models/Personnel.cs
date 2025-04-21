using System.ComponentModel.DataAnnotations;

namespace Law_Model.Models
{
    public class Personnel
    {

        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } // Foreign Key to ApplicationUser

        public ApplicationUser User { get; set; }

        [Required]
        public string Position { get; set; } // e.g., "Lawyer", "Paralegal"

        public string Department { get; set; }

        public DateTime HireDate { get; set; }

        // Navigation properties
        public ICollection<LegalCase> AssignedCases { get; set; }


    }
}
