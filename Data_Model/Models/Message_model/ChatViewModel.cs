using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Law_Model.Models.Message_model
{
    public class ChatViewModel
    {
        public string CurrentUserId { get; set; }
        public string ReceiverId { get; set; }
        public string? GroupName { get; set; }
        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
        public bool IsGroupChat { get; set; }
    }
}
