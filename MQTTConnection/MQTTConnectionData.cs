namespace MQTTSConnectionData
{

    /// <summary>
    /// A data class that represents the data needed to connect to an MQTT broker and a WiFi network.
    /// </summary>
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

        /// <param name="name">A name for the connection</param>
        /// <param name="mqttClientId">MQTT client ID</param>
        /// <param name="mqttHost">MQTT broker hostname</param>
        /// <param name="mqttPort">MQTT broker port</param>
        /// <param name="mqttUsername">MQTT username</param>
        /// <param name="mqttPassword">MQTT password</param>
        /// <param name="wifiSSID">WiFi SSID(username)</param>
        /// <param name="wifiPassword">WiFi password</param>
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

        /// <summary>
        /// Creates a topic string for a ThingSpeak channel and field.
        /// </summary>
        /// <param name="channelNumber">The channel number of the Thingspeak channel</param>
        /// <param name="field">Field in the Thingspeak Channel</param>
        public string getThinkspeakTopicString(string channelNumber, string field)
        {
            return $"channels/{channelNumber}/subscribe/fields/{field}";
        }

        /// <summary>
        /// Returns an array of valid fields for a ThingSpeak channel.
        /// </summary>
        public string[] getValidThinkspeakTopicFields()
        {
            return new string[] { "field1", "field2", "field3", "field4", "field5", "field6", "field7", "field8" };
        }
    }
}

