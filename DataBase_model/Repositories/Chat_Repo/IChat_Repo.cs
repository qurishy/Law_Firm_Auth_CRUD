using DataAccess.Repositories;
using Law_Model.Models.Message_model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DATA.Repositories.Chat_Repo
{
    public interface IChat_Repo : IRepository<ChatMessage>
    {
        public Task<IEnumerable<ChatMessage>> GetConversation(string userId1, string userId2);

        public Task<IEnumerable<ChatMessage>> GetUserConversations(string userId);

        public Task<IEnumerable<ChatMessage>> GetGroupMessages(string groupName);

        public Task MarkMessagesAsRead(List<int> messageIds);

        abstract string GenerateConversationId(string userId1, string userId2);




    }
}
