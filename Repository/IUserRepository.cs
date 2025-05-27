using Amazon.DynamoDBv2.DocumentModel;

namespace TeamsApplicationServer.Repository
{
    public interface IUserRepository
    {
        Task<Document> GetOrCreateUserAsync(string userId, string username, string email);
        Task<List<Document>> SearchUsersAsync(string searchText);
    }
}
