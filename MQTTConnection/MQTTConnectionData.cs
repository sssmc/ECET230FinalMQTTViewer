namespace MQTTSConnectionData
{
    public class ConnectionData
    {
        public string Name { get; set; }

        public string MQTTClientId { get; set; }

        public string MQTTHost { get; set; }

        public int MQTTPort { get; set; }

        public string MQTTUsername { get; set; }

        public string MQTTPassword { get; set; }

        public string WifiSSID { get; set; }

        public string WifiPassword { get; set; }

        public ConnectionData(string name, string mqttClientId, string mqttHost, int mqttPort, string mqttUsername, string mqttPassword, string wifiSSID, string wifiPassword)
        {
            Name = name;
            MQTTClientId = mqttClientId;
            MQTTHost = mqttHost;
            MQTTPort = mqttPort;
            MQTTUsername = mqttUsername;
            MQTTPassword = mqttPassword;
            WifiSSID = wifiSSID;
            WifiPassword = wifiPassword;
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

