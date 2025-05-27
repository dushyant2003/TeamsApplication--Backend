using TeamsApplicationServer.Model;

namespace TeamsApplicationServer.Repository
{
    public interface IChatRepository
    {
        public Task SaveUndeliveredMessageAsync(SendMessageModel message);
    }
}
