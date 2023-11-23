using MQTTScreenData;
using MQTTSConnectionData;
using System.Text.Json;

namespace ECET230FinalMQTTViewerDesktop.Models
{
    public partial class ScreenDataModel
    {
        private ScreenData _screenData;

        private int _currentScreen;

        public int ScreenCount
        {
            get
            {
                return _screenData.Indicators.GetLength(0);
            }
        }

        private SerialConnectionModel _serialConnectionModel { get; set; }

        public EventHandler ScreenDataUpdated { get; set; }

        public ScreenDataModel()
        {

            //Test Data!!!
            ConnectionData testConnection; testConnection = new ConnectionData("ThingSpeak",
                                            "FDkPCxA2KTkHMgANKik6NgI",
                                            "broker.mqtt-dashboard.com",
                                            1883,
                                            "FDkPCxA2KTkHMgANKik6NgI",
                                            "lRBFHoyhV9ruKuh0sy7s0QXm",
                                            "IoT-Security",
                                            "B@kery204!");

            IndicatorData tempIndicator = new IndicatorData("Temperature", "Temperature", "channels/2328115/subscribe/fields/field1", "numeric", 100, 0);
            IndicatorData humIndicator = new IndicatorData("Humidity", "Humidity", "channels/2328115/subscribe/fields/field2", "numeric", 100, 0);

            IndicatorData random1Indicator = new IndicatorData("Random1", "Random1", "channels/2328115/subscribe/fields/field3", "numeric", 10, 0);
            IndicatorData random2Indicator = new IndicatorData("Random2", "Random2", "channels/2328115/subscribe/fields/field4", "numeric", 10, 0);

            IndicatorData[][] indicators = new IndicatorData[2][];

            indicators[0] = new IndicatorData[] { tempIndicator, humIndicator };
            indicators[1] = new IndicatorData[] { random1Indicator, random2Indicator, tempIndicator, humIndicator };

            ScreenData defaultScreenData = new ScreenData(testConnection, indicators);
            //End Test Data!!!

            _screenData = defaultScreenData;
            _serialConnectionModel = App.dataSerialConnection;

            _serialConnectionModel.DataReceived += _serialConnectionModel_DataReceived;

        }

        private void _serialConnectionModel_DataReceived(object sender, DataReceivedEventArgs e)
        {
            SetScreenData(e.data);
        }

        public IndicatorData[] IndicatorsAtScreenIndex(int index)
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

        public bool SetScreenData(string rawData)
        {
            if(rawData.StartsWith("##"))
            {
                if(rawData.Substring(2, 1) == "0")
                {
                    int payloadLength;
                    if(int.TryParse(rawData.Substring(3, 4), out payloadLength))
                    {
                        string jsonPayload = rawData.Substring(7, payloadLength);

                        string checksum = rawData.Substring(7 + payloadLength, 4);

                        if (ChecksumCalculator.ChecksumCalculator.CalculateChecksum(jsonPayload).ToString("0000") == checksum)
                        {
                            _screenData = JsonSerializer.Deserialize<ScreenData>(jsonPayload);
                            ScreenDataUpdated?.Invoke(this, new System.EventArgs());
                            return true;
                        }
                        else { return false; }
                    }
                    else { return false; }
                }
                else if(rawData.Substring(2, 1) == "1")
                {
                    SendScreenDataToDevice();
                    return true;
                }
                else { return false; }
            }
            else { return false; }
        }

        public void SendScreenDataToDevice()
        {
            string payload = JsonSerializer.Serialize<ScreenData>(_screenData);

            string packet = $"##0{payload.Length.ToString("0000")}{payload}{ChecksumCalculator.ChecksumCalculator.CalculateChecksum(payload).ToString("0000")}";
            
            _serialConnectionModel.WriteLine(packet);
        }

        public void RequestScreenDataFromDevice()
        {
            string packet = "##100000000";
            _serialConnectionModel.WriteLine(packet);
        }




    }
}
