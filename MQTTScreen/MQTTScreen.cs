using MQTTServerConnection;

namespace MQTTScreen
{
    public class Screen
    {
        
        public Connection Connection { get; set;}

        public List<Indicator> Indicators { get; set; }

        public string Name { get; set; }

        public Screen(string name, Connection connection)
        {
            Name = name;
            Connection = connection;
            Indicators = new List<Indicator>();
        }

        public Screen(string name, Connection connection, List<Indicator> indicators)
        {
            Name = name;
            Connection = connection;
            Indicators = indicators;
        }

    }

    public class Indicator
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Topic { get; set; }

        public string Type { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public Indicator(string name, string description, string topic, string type, int maxValue, int minValue)
        {
            Name = name;
            Description = description;
            Topic = topic;
            Type = type;
            MaxValue = maxValue;
            MinValue = minValue;
        }

        public string[] getValidTypes()
        {
            return new string[] { "gauge", "bar", "numeric" };
        }



    }

}