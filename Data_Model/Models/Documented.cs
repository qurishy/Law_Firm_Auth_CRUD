using System.ComponentModel.DataAnnotations;

namespace Law_Model.Models
{
    public class Documented
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        public string Description { get; set; }

        [Required]
        public string FilePath { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public string UploadedById { get; set; }
        public ApplicationUser UploadedBy { get; set; }

        public int CaseId { get; set; }
        public LegalCase Case { get; set; }

        public bool? IsPrivate { get; set; } // Whether client can see it
    }
}
