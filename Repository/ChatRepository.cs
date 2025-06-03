using Amazon.DynamoDBv2.DocumentModel;
using TeamsApplicationServer.Helpers;
using TeamsApplicationServer.Model;

namespace TeamsApplicationServer.Repository
{
    public class ChatRepository : IChatRepository
    {
        private readonly Table _table;
        public ChatRepository()
        {
            _table = DynamoDbHelper.GetTable();
        }

        public async Task SaveUndeliveredMessageAsync(string userId,SendMessageModel message)
        {
            var senderName = message.SenderName;
            var senderId = message.SenderId;
            var msg = message.Message;
            var type = message.Type;
            var msgTimestamp = message.TimeStamp;
            var currentTimestampMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var expirySeconds = DateTimeOffset.UtcNow.AddDays(15).ToUnixTimeSeconds();

            var document = new Document
            {
                ["PK"] = $"user:{userId}",
                ["SK"] = $"undelivered:{currentTimestampMs}",
                ["message"] = message.Message,
                ["senderName"] = senderName,
                ["senderId"] = senderId,
                ["type"] = type,
                ["timestamp"] = msgTimestamp,
                ["createdAt"] = currentTimestampMs,
                ["expiry"] = expirySeconds
            };
            await _table.PutItemAsync(document);
        }


        public async Task<List<Document>> GetUndeliveredMessagesAsync(string username)
        {
            string userId = username.ToLower().Replace(" ", "_");

            var pk = $"user:{userId}";
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, pk);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, "undelivered:");
            var search = _table.Query(queryFilter);
            var docs = await search.GetNextSetAsync();
            return docs;
        }

        public Task DeleteUndeliveredMessageAsync(string username)
        {
            string userId = username.ToLower().Replace(" ", "_");
            // Delete all undelivered messages for the user
            var pk = $"user:{userId}";
            var queryFilter = new QueryFilter("PK", QueryOperator.Equal, pk);
            queryFilter.AddCondition("SK", QueryOperator.BeginsWith, "undelivered:");
            var search = _table.Query(queryFilter);
            return Task.WhenAll(search.GetNextSetAsync().Result.Select(doc => _table.DeleteItemAsync(doc)));
        }
    }
}