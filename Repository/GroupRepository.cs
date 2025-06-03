using Amazon.DynamoDBv2.DocumentModel;
using TeamsApplicationServer.Helpers;

namespace TeamsApplicationServer.Repository
{
    public class GroupRepository: IGroupRepository
    {
        private readonly Table _table;

        public GroupRepository()
        {
            _table = DynamoDbHelper.GetTable();
        }

        public Task CreateGroup(string groupName, string userId, string groupUUID)
        {
            var document = new Document
            {
                ["PK"] = $"group:{groupUUID}",
                ["SK"] = "metadata",
                ["groupName"] = groupName,
                ["createdBy"] = userId,
                ["createdAt"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            return _table.PutItemAsync(document);
        }

        // function to get group details by uuid
        public async Task<Document> GetGroupByUUID(string groupUUID)
        {
            var document = await _table.GetItemAsync($"group:{groupUUID}", "metadata");
            if (document == null)
            {
                throw new Exception($"Group with UUID {groupUUID} does not exist.");
            }
            return document;
        }

        public async Task AddMember(string groupId, string username)
        {
            string userId = username.ToLower().Replace(" ", "_");
            // check if the user already exists in the group
            var existingMember = _table.GetItemAsync($"group:{groupId}", $"member:{userId}").Result;

            if (existingMember != null)
            {
                throw new Exception($"User {userId} is already a member of group {groupId}.");
            }
           
            var document = new Document
            {
                ["PK"] = $"group:{groupId}",
                ["SK"] = $"member:{userId}",
                ["userId"] = userId,
                ["userName"] = username,
                ["addedAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            var groupDocument = _table.GetItemAsync($"group:{groupId}", "metadata").Result;

            if (groupDocument == null)
            {
                throw new Exception($"Group with ID {groupId} does not exist.");
            }
            var groupName = groupDocument["groupName"].AsString();

            var userDocument = new Document
            {
                ["PK"] = $"user:{userId}",
                ["SK"] = $"group:{groupId}",
                ["groupId"] = groupId,
                ["groupName"] = groupName, 
                ["addedAt"] = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            await _table.PutItemAsync(userDocument);
            await  _table.PutItemAsync(document);
        }
        public async Task<List<Document>> GetGroupMembers(string groupId)
        {
            // Adjusted the Query method to use the correct overload with QueryOperationConfig
            var queryConfig = new QueryOperationConfig
            {
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PK = :pk AND begins_with(SK, :sk)",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":pk", $"group:{groupId}" },
                        { ":sk", "member:" }
                    }
                }
            };

            var search = _table.Query(queryConfig);
            var membersData = new List<Document>();
            do
            {
                var page = await search.GetNextSetAsync();
                membersData.AddRange(page);
            } while (!search.IsDone);

            return membersData;
        }

        public async Task RemoveMember(string groupId, string userId)
        {
            var memberDocument = await _table.GetItemAsync($"group:{groupId}", $"member:{userId}");
            if (memberDocument == null)
            {
                throw new Exception($"User {userId} is not a member of group {groupId}.");
            }
            await _table.DeleteItemAsync($"group:{groupId}", $"member:{userId}");

            await _table.DeleteItemAsync($"user:{userId}", $"group:{groupId}");
        }

        public async Task<List<Document>> GetGroupsByUserId(string userId)
        {
            var queryConfig = new QueryOperationConfig
            {
                KeyExpression = new Expression
                {
                    ExpressionStatement = "PK = :pk AND begins_with(SK, :sk)",
                    ExpressionAttributeValues = new Dictionary<string, DynamoDBEntry>
                    {
                        { ":pk", $"user:{userId}" },
                        { ":sk", "group:" }
                    }
                }
            };
            var search = _table.Query(queryConfig);
            var groups = new List<Document>();
            do
            {
                var page = await search.GetNextSetAsync();
                groups.AddRange(page);
            } while (!search.IsDone);
            return groups;
        }
    }
}
