using Amazon.DynamoDBv2.DocumentModel;

namespace TeamsApplicationServer.Repository
{
    public interface IGroupRepository
    {
        public Task<List<Document>> GetGroupMembers(string groupId);
        public Task<Document> GetGroupByUUID(string groupUUID);
        public Task CreateGroup(string groupName, string userId, string groupUUID);
        public Task AddMember(string groupId, string userId);
        public Task RemoveMember(string groupId, string userId);
        public Task<List<Document>> GetGroupsByUserId(string userId);
    }
}
