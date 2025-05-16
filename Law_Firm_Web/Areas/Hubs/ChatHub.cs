using DataAccess.Data;
using Law_Model.Models;
using Law_Model.Models.Message_model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Law_Firm_Web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly DATA.Repositories.Chat_Repo.IChat_Repo _chatRepo;

        public ChatHub(
            UserManager<ApplicationUser> userManager,
            DATA.Repositories.Chat_Repo.IChat_Repo chatRepo)
        {
            _userManager = userManager;
            _chatRepo = chatRepo;
        }

        // Send a direct message to a specific user
        public async Task SendMessage(string message, string receiverId)
        {
            var senderId = Context.UserIdentifier;

            // Get sender name
            var sender = await _userManager.FindByIdAsync(senderId);
            var senderName = $"{sender.FirstName} {sender.LastName}";

            // Store the message in database
            var chatMessage = new ChatMessage
            {
                SenderId = senderId,
                ReceiverId = receiverId,
                Message = message,
                SentAt = DateTime.Now,
                IsRead = false,
                ConversationId = GenerateConversationId(senderId, receiverId)
            };

            await _chatRepo.Add(chatMessage);

            // Send to the sender (for all connections)
            await Clients.User(senderId).SendAsync("ReceiveMessage", senderId, senderName, message);

            // Send to the receiver (for all connections)
            await Clients.User(receiverId).SendAsync("ReceiveMessage", senderId, senderName, message);
        }

        // Send a message to a group
        public async Task SendMessageToGroup(string groupName, string message)
        {
            var senderId = Context.UserIdentifier;

            // Get sender name
            var sender = await _userManager.FindByIdAsync(senderId);
            var senderName = $"{sender.FirstName} {sender.LastName}";

            // Send to all users in the group
            await Clients.Group(groupName).SendAsync("ReceiveGroupMessage", senderId, senderName, groupName, message);
        }

        // Join a chat group
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);

            var userId = Context.UserIdentifier;
            var user = await _userManager.FindByIdAsync(userId);
            var userName = $"{user.FirstName} {user.LastName}";

            await Clients.Group(groupName).SendAsync("UserJoined", userId, userName, groupName);
        }

        // Leave a chat group
        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);

            var userId = Context.UserIdentifier;
            var user = await _userManager.FindByIdAsync(userId);
            var userName = $"{user.FirstName} {user.LastName}";

            await Clients.Group(groupName).SendAsync("UserLeft", userId, userName, groupName);
        }

        // Map user to connection ID on connect
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
        }

        // Helper to generate consistent conversation IDs
        private string GenerateConversationId(string userId1, string userId2)
        {
            // Generate a consistent conversation ID regardless of sender/receiver order
            var orderedIds = new[] { userId1, userId2 }.OrderBy(id => id).ToArray();
            return $"conversation_{orderedIds[0]}_{orderedIds[1]}";
        }
    }
}