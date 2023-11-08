using MQTTServerConnection;

namespace MQTTScreen
{
    public class Screen
    {
        
        public Connection Connection { get; set;}

        public List<Indicator> Indicators { get; set; }

        public string Name { get; set; }

    }

    public class Indicator
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Topic { get; set; }

        public string Type { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }



    }

}