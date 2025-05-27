using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2;
using Amazon;

namespace TeamsApplicationServer.Helpers
{
    public class DynamoDbHelper
    {
        private static readonly AmazonDynamoDBClient _client;
        private static readonly Table _table;

        static DynamoDbHelper()
        {
            _client = new AmazonDynamoDBClient(RegionEndpoint.APSouth1);

            _table = new TableBuilder(_client, "ChatAppDB")
                            .AddHashKey("PK", DynamoDBEntryType.String)
                            .AddRangeKey("SK", DynamoDBEntryType.String)
                            .Build();
        }
        public static Table GetTable()
        {
            return _table;
        }

    }
}
