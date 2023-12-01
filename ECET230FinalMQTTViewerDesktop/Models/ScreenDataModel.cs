using MQTTScreenData;
using MQTTSConnectionData;

using System.Text.Json;

namespace ECET230FinalMQTTViewerDesktop.Models
{
    /// <summary>
    /// Model for the screen data behivior.
    /// </summary>
    public partial class ScreenDataModel
    {
        public ScreenData _screenData { get; set;}

        private int _currentScreen;

        /// <summary>
        /// Number of screens in the screen data.
        /// </summary>
        public int ScreenCount
        {
            get
            {
                return _screenData.Indicators.GetLength(0);
            }
        }

        /// <summary>
        /// Reference to the serial connection model.
        /// </summary>
        private SerialConnectionModel _serialConnectionModel;

        /// <summary>
        /// Event that is fired when the screen data is updated.
        /// </summary>
        public EventHandler ScreenDataUpdated { get; set; }

        public ScreenDataModel()
        {

            //Default Data to display before data is received from the device.
            ConnectionData testConnection; testConnection = new ConnectionData("Connection",
                                            "ClientID",
                                            "BrokerHost",
                                            1883,
                                            "MQTT username",
                                            "MQTT password",
                                            "WiFi SSID",
                                            "WiFi password");

            IndicatorData tempIndicator = new IndicatorData("Indicator1", "Test", "Topic", "numeric", 100, 0);
            IndicatorData humIndicator = new IndicatorData("Indicator2", "Test", "Topic", "numeric", 100, 0);

            IndicatorData random1Indicator = new IndicatorData("Indicator3", "Test", "Topic", "numeric", 10, 0);
            IndicatorData random2Indicator = new IndicatorData("Indicator4", "Test", "Topic", "numeric", 10, 0);

            IndicatorData[][] indicators = new IndicatorData[2][];

            indicators[0] = new IndicatorData[] { tempIndicator, humIndicator };
            indicators[1] = new IndicatorData[] { random1Indicator, random2Indicator, tempIndicator, humIndicator };

            ScreenData defaultScreenData = new ScreenData(testConnection, indicators);
            //End Default Data

            //Use the default data until data is received from the device.
            _screenData = defaultScreenData;

            //Reference to the data serial connection model.
            _serialConnectionModel = App.dataSerialConnection;

            _serialConnectionModel.DataReceived += _serialConnectionModel_DataReceived;

        }


        /// <summary>
        /// Executes when data is received from the device.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void _serialConnectionModel_DataReceived(object sender, DataReceivedEventArgs e)
        {
            SetScreenData(e.data);
        }

        /// <summary>
        /// The MQTT and WiFi connection data for the device.
        /// </summary>
        public ConnectionData Connection
        {
            get
            {
                return _screenData.Connection;
            }
            set
            {
                _screenData.Connection = value;
            }
        }

        /// <summary>
        /// 2D array of indicators
        /// </summary>
        public IndicatorData[][] Indicators
        {
            get
            {
                return _screenData.Indicators;
            }
            set
            {
                _screenData.Indicators = value;
            }
        }

        /// <summary>
        /// Returns array of indicators at the screen index.
        /// </summary>
        /// <param name="index">Screen Index</param>
        /// <returns></returns>
        public IndicatorData[] GetIndicatorsAtScreenIndex(int index)
        {
            if(index == -1)
            {
                return new IndicatorData[0];
            }else if (index < _screenData.Indicators.GetLength(0))
            {
                return _screenData.Indicators[index];
            }
            else { return null; }
        }

        /// <summary>
        /// Returns list of indicators at the screen index.
        /// </summary>
        /// <param name="index">Screen Index</param>
        /// <returns></returns>
        public List<IndicatorData> GetIndicatorsAtScreenIndexAsList(int index)
        {
            if (index == -1)
            {
                return new List<IndicatorData>();
            }
            else if (index < _screenData.Indicators.GetLength(0))
            {
                return _screenData.Indicators[index].ToList();
            }
            else { return null; }
        }

        /// <summary>
        /// Sets the indicators at the screen index.
        /// </summary>
        /// <param name="index">Screen Index</param>
        /// <param name="indicators">Indicators</param>
        public void SetIndicatorsAtScreenIndex(int index, IndicatorData[] indicators)
        {
            if(index < _screenData.Indicators.GetLength(0))
            {
                _screenData.Indicators[index] = indicators;
            }
        }

        /// <summary>
        /// Adds a new screen to the screen data.
        /// </summary>
        public void AddScreen()
        {
            IndicatorData[][] newIndicators = new IndicatorData[_screenData.Indicators.GetLength(0) + 1][];

            for(int i = 0; i < _screenData.Indicators.GetLength(0); i++)
            {
                newIndicators[i] = _screenData.Indicators[i];
            }

            IndicatorData newIndicator = new IndicatorData("New Indicator", "", "", "numeric", 100, 0);

            newIndicators[_screenData.Indicators.GetLength(0)] = new IndicatorData[] { newIndicator };

            _screenData.Indicators = newIndicators;
        }

        /// <summary>
        /// Removes the screen at the index.
        /// </summary>
        /// <param name="index">Screen Index</param>
        public void RemoveScreen(int index) 
        {
            if(index < _screenData.Indicators.GetLength(0) && _screenData.Indicators.Length != 0 && index != -1)
            {
                IndicatorData[][] newIndicators = new IndicatorData[_screenData.Indicators.GetLength(0) - 1][];

                for(int i = 0; i < index; i++)
                {
                    newIndicators[i] = _screenData.Indicators[i];
                }

                for(int i = index + 1; i < _screenData.Indicators.GetLength(0); i++)
                {
                    newIndicators[i - 1] = _screenData.Indicators[i];
                }

                _screenData.Indicators = newIndicators;
            }
        }

        /// <summary>
        /// Sets the screen data from a raw packet.
        /// </summary>
        /// <param name="rawData">Raw Packet</param>
        public bool SetScreenData(string rawData)
        {

            //Check if the header starts with "##"
            if(rawData.StartsWith("##"))
            {
                //If the if a data packet was received
                if(rawData.Substring(2, 1) == "0")
                {
                    int payloadLength;
                    //Parse the payload length
                    if(int.TryParse(rawData.Substring(3, 4), out payloadLength))
                    {
                        string jsonPayload = rawData.Substring(7, payloadLength);

                        string checksum = rawData.Substring(7 + payloadLength, 4);

                        //Check if the checksum is correct
                        if (ChecksumCalculator.ChecksumCalculator.CalculateChecksum(jsonPayload).ToString("0000") == checksum)
                        {
                            //Deserialize the payload and update the screen data
                            _screenData = JsonSerializer.Deserialize<ScreenData>(jsonPayload);
                            ScreenDataUpdated?.Invoke(this, new System.EventArgs());
                            return true;
                        }
                        else { return false; }
                    }
                    else { return false; }
                }
                //If a screen data request was received`
                else if(rawData.Substring(2, 1) == "1")
                {
                    //Send the screen data to the device
                    SendScreenDataToDevice();
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        /// <summary>
        /// Sends the screen data to the device.
        /// </summary>
        public void SendScreenDataToDevice()
        {
            //Serialize the screen data
            string payload = JsonSerializer.Serialize<ScreenData>(_screenData);

            //Construct the packet
            string packet = $"##0{payload.Length.ToString("0000")}{payload}{ChecksumCalculator.ChecksumCalculator.CalculateChecksum(payload).ToString("0000")}";
            
            //Send the packet to the device
            _serialConnectionModel.WriteLine(packet);
        }

        /// <summary>
        /// Requests the screen data from the device.
        /// </summary>
        public void RequestScreenDataFromDevice()
        {
            //Send the request packet to the device
            string packet = "##100000000";
            _serialConnectionModel.WriteLine(packet);
        }




    }
}
