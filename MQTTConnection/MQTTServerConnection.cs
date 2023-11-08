namespace MQTTServerConnection
{
    public class Connection
    {
        public string Name { get; set; }

        public string ClientId { get; set; }

        public string Host { get; set; }

        public int Port { get; set; }

        public string Username { get; set; }

        public string Password { get; set; }

        public Connection(string name, string clientId, string host, int port, string username, string password)
        {
            Name = name;
            ClientId = clientId;
            Host = host;
            Port = port;
            Username = username;
            Password = password;
        }

        public string getThinkspeakTopicString(string channelNumber, string field)
        {
            return $"channels/{channelNumber}/subscribe/fields/{field}";
        }
        public string[] getValidThinkspeakTopicFields()
        {
            return new string[] { "field1", "field2", "field3", "field4", "field5", "field6", "field7", "field8" };
        }
    }
}

