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
        public override async Task OnConnectedAsync()
        {
            string userName = Context.User?.Identity?.Name ?? Context.ConnectionId;

            Console.WriteLine($"User {userName} connected with connection ID {Context.ConnectionId}");
            UserConnectionMap[userName] = Context.ConnectionId;

            await base.OnConnectedAsync();
        }

        public ChatHub(IUserRepository userRepository, IChatRepository chatRepository)
        {
            _userRepository = userRepository;
            _chatRepository = chatRepository;

        }
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            string userName = Context.User?.Identity?.Name ?? Context.ConnectionId;

            UserConnectionMap.Remove(userName);

            await base.OnDisconnectedAsync(exception);
        }



        public async Task SendMessage(SendMessageModel messageRequest)
        {

            Console.WriteLine(messageRequest.ToString());

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
                await _chatRepository.SaveUndeliveredMessageAsync(messageRequest);
            }
        }

        public async Task<List<string>> SearchUsers(string searchText)
        {
            var users = await _userRepository.SearchUsersAsync(searchText);

            return users.Select(u => u["username"].AsString()).ToList();
        }
        public async Task SendPrivateMessage(string toUser, string message)
        {
            if (UserConnectionMap.TryGetValue(toUser, out var connectionId))
            {
                await Clients.Client(connectionId).SendAsync("ReceiveMessage", message);
            }
        }


    }
 }
