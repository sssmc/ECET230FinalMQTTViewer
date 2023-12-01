using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using MQTTSConnectionData;

namespace MQTTScreenData
{

    /// <summary>
    /// A data class that repeents the indicators and connection data for the meadow device.
    /// </summary>
    public class ScreenData
    {

        /// <summary>
        /// The MQTT and WiFi connection data for the device.
        /// </summary>
        public ConnectionData Connection { get; set; }

        public IndicatorData[][] Indicators { get; set; }

        public ScreenData()
        {

        }

        /// <param name="name"> Name of the Screen Data</param>
        /// <param name="connection">The MQTT and WiFi connection data for the device</param>
        public ScreenData(string name, ConnectionData connection)
        {
            Connection = connection;
            Indicators = new IndicatorData[10][];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="connection">The MQTT and WiFi connection data for the device</param>
        /// <param name="indicators">The indicators to be displayed</param>
        public ScreenData(ConnectionData connection, IndicatorData[][] indicators)
        {
            Connection = connection;
            Indicators = indicators;
        }

    }

    /// <summary>
    /// A data class that represents an indicator to be displayed on a screen.
    /// </summary>
    public partial class IndicatorData : ObservableObject
    {

        /// <summary>
        /// Name of the indicator
        /// </summary>
        [ObservableProperty]
        private string _name;

        /// <summary>
        /// A short description of the indicator
        /// </summary>
        [ObservableProperty]
        private string _description;

        /// <summary>
        /// The MQTT topic to subscribe to for the indicator
        /// </summary>
        [ObservableProperty]
        private string _topic;

        /// <summary>
        /// What type of indicator to display
        /// <list type="bullet">
        /// <listheader>Valid Types</listheader>
        ///<item>gauge</item>
        ///<item>bar</item>
        ///<item>numeric</item>
        /// </list>
        ///     
        /// </summary>
        [ObservableProperty]
        private string _type;

        /// <summary>
        /// The max nurmerical value of the data if applicable
        /// </summary>
        [ObservableProperty]
        private int _maxValue;

        /// <summary>
        /// The min nurmerical value of the data if applicable
        /// </summary>
        [ObservableProperty]
        private int _minValue;

        public IndicatorData(string name, string description, string topic, string type, int maxValue, int minValue)
        {
            Name = name;
            Description = description;
            Topic = topic;
            Type = type;
            MaxValue = maxValue;
            MinValue = minValue;
        }

        public IndicatorData()
        {
            Name = "";
            Description = "";
            Topic = "";
            Type = "";
        }

        /// <summary>
        /// Returns a list of the valid indicator types.
        /// </summary> 
        public string[] getValidTypes()
        {
            return new string[] { "gauge", "bar", "numeric" };
        }



    }

}

