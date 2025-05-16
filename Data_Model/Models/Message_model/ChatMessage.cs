using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law_Model.Models.Message_model
{
    public class ChatMessage
    {
        public int Id { get; set; }

        [Required]
        public string SenderId { get; set; } // Foreign Key to ApplicationUser

        [ForeignKey(nameof(SenderId))]
        public ApplicationUser? Sender { get; set; }

        [Required]
        public string ReceiverId { get; set; } // Foreign Key to ApplicationUser

        [ForeignKey(nameof(ReceiverId))]
        public ApplicationUser? Receiver { get; set; }

        [Required]
        public string Message { get; set; }

        public bool? IsRead { get; set; } = false;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        // For group chats or conversation tracking
        public string ConversationId { get; set; }
    }
   
}
