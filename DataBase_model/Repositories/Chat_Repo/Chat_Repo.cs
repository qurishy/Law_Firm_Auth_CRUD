using DataAccess.Data;
using DataAccess.Repositories;
using Law_Model.Models.Message_model;
using Microsoft.EntityFrameworkCore;

namespace DATA.Repositories.Chat_Repo
{
    public class Chat_Repo :Repository<ChatMessage>, IChat_Repo
    {
        private readonly AplicationDB _context;

        public Chat_Repo(AplicationDB context) : base(context)
        {
            _context = context;
        }



        // Get a conversation between two users
        public async Task<IEnumerable<ChatMessage>> GetConversation(string userId1, string userId2)
        {
            var conversationId = GenerateConversationId(userId1, userId2);

            var messages = await _context.ChatMessages
                .Where(m => m.ConversationId == conversationId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages;
        }

        // Get all conversations for a specific user
        public async Task<IEnumerable<ChatMessage>> GetUserConversations(string userId)
        {
            var conversations = await _context.ChatMessages
                .Where(m => m.SenderId == userId || m.ReceiverId == userId)
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return conversations;
        }


        // Get all messages in a group chat
        public async Task<IEnumerable<ChatMessage>> GetGroupMessages(string groupName)
        {
            var messages = await _context.ChatMessages
                .Where(m => m.ReceiverId == groupName) // Assuming groupName is stored in ReceiverId for group chats
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            return messages;
        }

        // Mark specific messages as read
        public async Task MarkMessagesAsRead(List<int> messageIds)
        {
            var messages = await _context.ChatMessages
                .Where(m => messageIds.Contains(m.Id))
                .ToListAsync();

            foreach (var message in messages)
            {
                message.IsRead = true;
            }

            await _context.SaveChangesAsync();
        }



        public string GenerateConversationId(string userId1, string userId2)
        {
            var orderedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
            return $"conversation_{orderedIds[0]}_{orderedIds[1]}";
        }
    }
    
}
