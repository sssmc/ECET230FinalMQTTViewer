using System;
using System.Collections.Generic;
using MQTTSConnectionData;

namespace MQTTScreenData
{
    public class ScreenData
    {

        public ConnectionData Connection { get; set; }

        public IndicatorData[][] Indicators { get; set; }

        public ScreenData()
        {

        }


        public ScreenData(string name, ConnectionData connection)
        {
            Connection = connection;
            Indicators = new IndicatorData[10][];
        }

        public ScreenData(ConnectionData connection, IndicatorData[][] indicators)
        {
            Connection = connection;
            Indicators = indicators;
        }

    }

    public class IndicatorData
    {
        public string Name { get; set; }
        public string Description { get; set; }

        public string Topic { get; set; }

        public string Type { get; set; }

        public int MaxValue { get; set; }

        public int MinValue { get; set; }

        public IndicatorData(string name, string description, string topic, string type, int maxValue, int minValue)
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

