using Amazon.DynamoDBv2.DocumentModel;
using TeamsApplicationServer.Model;

namespace TeamsApplicationServer.Repository
{
    public interface IChatRepository
    {
        public Task SaveUndeliveredMessageAsync(string userId,SendMessageModel message);

        public Task<List<Document>> GetUndeliveredMessagesAsync(string userId);
        public Task DeleteUndeliveredMessageAsync(string userId);
    }
}
