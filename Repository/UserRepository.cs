using Amazon.DynamoDBv2.DocumentModel;
using Microsoft.AspNetCore.Razor.TagHelpers;
using TeamsApplicationServer.Helpers;

namespace TeamsApplicationServer.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly Table _table;

        public UserRepository()
        {
            _table = DynamoDbHelper.GetTable();
        }
        public async Task<Document> GetOrCreateUserAsync(string userId, string username, string email)
        {
            string pk = $"user:{userId}";

            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, pk);
            var search = _table.Query(queryFilter);

            var docs = await search.GetNextSetAsync();
            if (docs.Any())
            {
                return docs.First();
            }

            var newUser = new Document
            {
                ["PK"] = pk,
                ["SK"] = "userdata",
                ["username"] = username,
                ["email"] = email
            };

            await _table.PutItemAsync(newUser);

            var userIndex = new Document
            {
                ["PK"] = "users",
                ["SK"] = pk,
                ["username"] = username
            };
            await _table.PutItemAsync(userIndex);

            return newUser;
        }
        public async Task<List<Document>> SearchUsersAsync(string searchText)
        {
            string pk = "users";
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, pk);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, $"user:{searchText.ToLower()}");
            var search = _table.Query(queryFilter);
            var docs = await search.GetNextSetAsync();
            return docs;
        }
    }
}
