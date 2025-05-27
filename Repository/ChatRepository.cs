using Amazon.DynamoDBv2.DocumentModel;
using TeamsApplicationServer.Helpers;
using TeamsApplicationServer.Model;

namespace TeamsApplicationServer.Repository
{
    public class ChatRepository: IChatRepository
    {
        private readonly Table _table;
        public ChatRepository()
        {
            _table = DynamoDbHelper.GetTable();
        }


        public async Task SaveUndeliveredMessageAsync(SendMessageModel message)
        {
            var userId = message.ReceiverId;
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
    }
}
