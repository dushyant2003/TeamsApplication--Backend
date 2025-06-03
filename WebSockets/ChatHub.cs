using Microsoft.AspNetCore.SignalR;
using TeamsApplicationServer.Model;
using TeamsApplicationServer.Repository;

namespace TeamsApplicationServer.WebSockets
{
    public class ChatHub : Hub
    {
        private static readonly Dictionary<string, string> UserConnectionMap = new();

        private readonly IUserRepository _userRepository;

        private readonly IChatRepository _chatRepository;

        private readonly IGroupRepository _groupRepository;
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User?.Identity?.Name ?? Context.ConnectionId;

            Console.WriteLine($"User {userName} connected with connection ID {Context.ConnectionId}");
            UserConnectionMap[userName] = Context.ConnectionId;

            // Load undelivered messages for the user
            var undeliveredMessages = await _chatRepository.GetUndeliveredMessagesAsync(userName);
            if (undeliveredMessages.Any())
            {
                foreach (var message in undeliveredMessages)
                {
                    var sendMessageModel = new SendMessageModel
                    {
                        ReceiverId = userName,
                        SenderName = message["senderName"].AsString(),
                        SenderId = message["senderId"].AsString(),
                        Message = message["message"].AsString(),
                        Type = message["type"].AsString(),
                        TimeStamp = message["timestamp"].AsLong()
                    };
                    // Remove the undelivered message after sending

                    await Clients.Client(Context.ConnectionId).SendAsync("ReceiveMessage", sendMessageModel);

                    await _chatRepository.DeleteUndeliveredMessageAsync(userName);
                }
            }

            await base.OnConnectedAsync();
        }

        public ChatHub(IUserRepository userRepository, IChatRepository chatRepository, IGroupRepository groupRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;
            _groupRepository = groupRepository;

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userName = Context.User?.Identity?.Name ?? Context.ConnectionId;

            UserConnectionMap.Remove(userName);

            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(SendMessageModel messageRequest)
        {
            if (messageRequest == null || string.IsNullOrEmpty(messageRequest.ReceiverName) || string.IsNullOrEmpty(messageRequest.Message))
            {
                return;
            }
            if (UserConnectionMap.TryGetValue(messageRequest.ReceiverName, out var connectionId))
            { 
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", messageRequest);
            }
            else
            {
                await _chatRepository.SaveUndeliveredMessageAsync(messageRequest.ReceiverId,messageRequest);
            }
        }

        public async Task<GroupDetailsModel> CreateGroup(string groupName, string username)
        {
            var groupId = Guid.NewGuid().ToString();

            await _groupRepository.CreateGroup(groupName, username, groupId);

            var groupDetails = await _groupRepository.GetGroupByUUID(groupId);

            var groupDetailsModel = new GroupDetailsModel
            {
                GroupId = groupDetails["PK"].AsString().Substring(6),       // to remove group: prefix
                GroupName = groupDetails["groupName"].AsString(),
                CreatedBy = groupDetails["createdBy"].AsString(),
                CreatedAt = groupDetails["createdAt"].AsLong()
            };

           return groupDetailsModel;
           
        }

      
        public async Task SendMessageToGroup(SendMessageModel messageRequest)
        {
            string groupId = messageRequest.ReceiverId;
            if (messageRequest == null || string.IsNullOrEmpty(messageRequest.Message))
            {
                return;
            }
            var groupDetails = await _groupRepository.GetGroupByUUID(groupId);
            if (groupDetails == null)
            {
                throw new Exception($"Group with ID {groupId} does not exist.");
            }
            var members = await _groupRepository.GetGroupMembers(groupId);
            foreach (var member in members)
            {
                // get the userName from the member document
                var userName = member["userName"].AsString();
                var userId = userName.Replace(" ", "_").ToLower();
                if (UserConnectionMap.TryGetValue(userName, out var connectionId))
                {
                    await Clients.Client(connectionId).SendAsync("ReceiveGroupMessage", messageRequest);
                }
                else
                {
                    await _chatRepository.SaveUndeliveredMessageAsync(userId,messageRequest);
                }
            }
        }

        public async Task AddMemberToGroup(string groupId, string userId)
        {
            try{
                await _groupRepository.AddMember(groupId, userId);
                var groupDetails = await _groupRepository.GetGroupByUUID(groupId);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error adding member to group: {ex.Message}");
            }
        }

        public async Task<List<string>> SearchUsers(string searchText)
        {
            var users = await _userRepository.SearchUsersAsync(searchText);

            return users.Select(u => u["username"].AsString()).ToList();
        }
        

    }
 }
