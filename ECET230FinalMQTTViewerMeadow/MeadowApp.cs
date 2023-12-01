//System
using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Timers;


//Meadow
using Meadow;
using Meadow.Hardware;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Leds;
using Meadow.Foundation.Sensors.Buttons;


//MQTTnet
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;


//Internal Libraries
using MQTTScreenData;
using MQTTSConnectionData;

namespace ECET230FinalMQTTViewerMeadow
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //Onboard LED
        RgbPwmLed onboardLed;

        //TFT Display
        MicroGraphics graphics;
        Ili9341 display;

        //Switch Page Button
        PushButton switchPageButton;

        //Serial Port
        ISerialPort serialPort;

        //Serial Data variables
        bool haveHeaderData = false;
        int payloadLength = 0;

        //Timer for serial timeout
        private static System.Timers.Timer serialTimeoutTimer;

        //MQTT client
        IMqttClient client;

        //Screen Data
        ScreenData screenData;
        Screen screen;

        //The file of the current screen data file
        const string dataFileName = "testScreen13.json";

        //File path for the screen data file
        string dataFilePath = MeadowOS.FileSystem.DataDirectory;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            //Initialize Onboard LED
            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            //Set LED to yellow while initializing
            onboardLed.SetColor(Color.Yellow);

            //Initialize Switch Page Button
            switchPageButton = new PushButton(Device.Pins.D10, ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(0.02));
            switchPageButton.Clicked += SwitchPageButton_Clicked;

            //Connect to TFT Display
            Resolver.Log.Info("Connecting to TFT...");

            //Create display object
            display = new Ili9341
            (
                spiBus: Device.CreateSpiBus(),
                chipSelectPin: Device.Pins.D11,
                dcPin: Device.Pins.D14,
                resetPin: Device.Pins.D15,
                width: 240, height: 320
            );

            //Create graphics object
            graphics = new MicroGraphics(display)
            {
                IgnoreOutOfBoundsPixels = true,
                CurrentFont = new Font12x16()
            };

            //Set the rotation of the display
            graphics.Rotation = RotationType._90Degrees;

            //Default Screen Data if no file is found
            ConnectionData testConnection; testConnection = new ConnectionData("ThingSpeak",
                                                "FDkPCxA2KTkHMgANKik6NgI",
                                                "broker.mqtt-dashboard.com",
                                                1883,
                                                "FDkPCxA2KTkHMgANKik6NgI",
                                                "lRBFHoyhV9ruKuh0sy7s0QXm",
                                                "White Rabbit",
                                                "2511560A7196");

            IndicatorData tempIndicator = new IndicatorData("Temperature", "Temperature", "channels/2328115/subscribe/fields/field1", "numeric", 100, 0);
            IndicatorData humIndicator = new IndicatorData("Humidity", "Humidity", "channels/2328115/subscribe/fields/field2", "numeric", 100, 0);

            IndicatorData random1Indicator = new IndicatorData("Random1", "Random1", "channels/2328115/subscribe/fields/field3", "numeric", 10, 0);
            IndicatorData random2Indicator = new IndicatorData("Random2", "Random2", "channels/2328115/subscribe/fields/field4", "numeric", 10, 0);

            IndicatorData[][] indicators = new IndicatorData[3][];

            indicators[0] = new IndicatorData[] { tempIndicator, humIndicator };
            indicators[1] = new IndicatorData[] { random1Indicator, random2Indicator, tempIndicator, humIndicator };
            indicators[2] = new IndicatorData[] { random2Indicator, random1Indicator };

            ScreenData defaultScreenData = new ScreenData(testConnection, indicators);

            //Try to load the data file
            Console.WriteLine("Loading Data File...");

            //Check if already file exists
            if (File.Exists(dataFilePath + "/" + dataFileName))
            {
                Console.WriteLine("File Found");

                try
                {
                    // Open the text file using a stream reader.
                    using (var sr = new StreamReader(dataFilePath + "/" + dataFileName))
                    {
                        // Read the stream as a string, and write the string to the console.
                        string file = sr.ReadToEnd();
                        Console.WriteLine(file);

                        //Deserialize JSON from file to the screenData object
                        screenData = new ScreenData();
                        screenData = JsonSerializer.Deserialize<ScreenData>(file);

                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                    Console.WriteLine("Using default data");

                    //Use default data if file fails to load
                    screenData = new ScreenData();
                    screenData = defaultScreenData;
                }

            }
            //If file does not exist
            else
            {
                try
                {
                    Console.WriteLine("File not found, creating file using default data");

                    //Create file using default data
                    using (var fs = File.CreateText(Path.Combine(dataFilePath, dataFileName)))
                    {
                        fs.WriteLine(JsonSerializer.Serialize(defaultScreenData));

                        screenData = new ScreenData();
                        screenData = defaultScreenData;
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed to create file");
                    Console.WriteLine(ex.Message);
                    Console.WriteLine("Using default data");
                    screenData = new ScreenData();
                    screenData = defaultScreenData;
                }
            }

            //Create Screen object using the screen data from the file
            screen = new Screen(screenData, graphics);

            //Set connection status texts
            screen.mqttStatusText = "MQTT waiting for WiFi";
            screen.wifiStatusText = "Connecting to WiFi";
            screen.drawScreen();

            //Connect to Wifi
            connectToWifi();

            //Create Serial Port
            serialPort = Device.PlatformOS.GetSerialPortName("Com1")
                                            .CreateSerialPort(baudRate: 4800,
                                            readBufferSize: 2048);
            serialPort.Open();
            serialPort.DataReceived += SerialPort_MessageReceived;

            //Initialize Serial Timeout Timer
            serialTimeoutTimer = new Timer(6000);
            serialTimeoutTimer.Elapsed += OnSerialTimeout;
            
            //Set LED to green when done initializing
            onboardLed.SetColor(Color.Green);

            return base.Initialize();
        }

        /// <summary>
        /// Executes when the serial port times out.
        /// </summary>
        private void OnSerialTimeout(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Serial Timeout");
            haveHeaderData = false;
            payloadLength = 0;
            serialPort.ClearReceiveBuffer();
            serialTimeoutTimer.Stop();
        }

        /// <summary>
        /// Executes when a message is received on the serial port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_MessageReceived(object sender, SerialDataReceivedEventArgs e)
        {

            //Start or restart the serial timeout timer
            if (serialTimeoutTimer.Enabled == false)
            {
                serialTimeoutTimer.Start();
            }
            else
            {
                serialTimeoutTimer.Stop();
                serialTimeoutTimer.Start();
            }

            //If we have not received enough bytes to read the header
            if (serialPort.BytesToRead < 7 && haveHeaderData == false)
            {
                return;
            }
            //If we have received enough bytes to read the header
            else if (serialPort.BytesToRead >= 7 && haveHeaderData == false)
            {
                Console.WriteLine("Reading header data...");

                //Display alert on screen
                screen.alertText = "Receving Data";
                screen.displayAlert = true;
                screen.drawScreen();

                //Read the header from the serial port
                byte[] response = new byte[7];
                serialPort.Read(response, 0, 7);

                //Convert the header to a string
                string data = Encoding.UTF8.GetString(response, 0, response.Length);
                Console.WriteLine("Header received: ");
                Console.WriteLine(data);

                //Check if the first two characters are "##"
                if (data.Substring(0, 2) == "##")
                {
                    //If we have received a data packet
                    if (data.Substring(2, 1) == "0")
                    {
                        //Parse the payload length from the header
                        payloadLength = int.Parse(data.Substring(3, 4)) + 4;
                        Console.WriteLine($"Payload Length: {payloadLength}");

                        //Set the haveHeaderData flag
                        haveHeaderData = true;
                    }
                    //We have received a data request packet
                    else if (data.Substring(2, 1) == "1")
                    {
                        Console.WriteLine("Data Request Packet Recevied");
                        screen.alertText = "Sending Data";
                        screen.drawScreen();

                        //Send the screen data to the meadow board
                        SendDataPacket();
                        screen.displayAlert = false;
                        haveHeaderData = false;
                        payloadLength = 0;
                        serialPort.ClearReceiveBuffer();
                        serialTimeoutTimer.Stop();
                        screen.displayAlert = false;
                        screen.drawScreen();
                        return;
                    }

                }
                else
                {
                    Console.WriteLine("Header Error");
                    serialTimeoutTimer.Stop();
                    screen.displayAlert = false;
                    screen.drawScreen();
                    return;
                }
            }

            //If we have received the headere but not received enough bytes to read the payload
            if (haveHeaderData && serialPort.BytesToRead < payloadLength)
            {
                return;
            }
            //If we have received the header and enough bytes to read the payload
            else if (haveHeaderData && serialPort.BytesToRead >= payloadLength)
            {
                Console.WriteLine("Reading payload data...");

                //Read the payload from the serial port
                byte[] dataBytes = new byte[payloadLength];
                serialPort.Read(dataBytes, 0, payloadLength);

                //Convert the payload to a string
                string data = Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);

                //Parse the payload without the checksum
                string payload = data.Substring(0, dataBytes.Length - 4);

                Console.WriteLine("Payload received: ");
                Console.WriteLine(payload);

                //Parse the checksum from the payload
                int checksumReceived = int.Parse(data.Substring(data.Length - 4, 4));
                Console.Write("Checksum Received: ");
                Console.WriteLine(checksumReceived.ToString());

                //Calculate the checksum of the payload
                int calculatedChecksum = ChecksumCalculator.ChecksumCalculator.CalculateChecksum(data.Substring(0, data.Length - 4));
                Console.Write("Checksum Calculated: ");
                Console.WriteLine(calculatedChecksum.ToString());

                //If the checksums match
                if (calculatedChecksum == checksumReceived)
                {
                    Console.WriteLine("Checksums Match");

                    haveHeaderData = false;
                    payloadLength = 0;
                    serialPort.ClearReceiveBuffer();
                    serialTimeoutTimer.Stop();
                    screen.mqttStatusText = "Updating MQTT Info";
                    screen.drawScreen();
                    Console.WriteLine("Updating Screen...");

                    //Deserialize the payload to a ScreenData object
                    ScreenData newScreenData = new ScreenData();
                    newScreenData = JsonSerializer.Deserialize<ScreenData>(payload);

                    //Save the wifi status text from the old screen data
                    string oldWifiStatus = screen.wifiStatusText;

                    //Create a new screen object with the new screen data
                    screen = new Screen(newScreenData, graphics);

                    screen.wifiStatusText = oldWifiStatus;

                    screen.drawScreen();
                    Console.WriteLine("Screen Updated");
                    Console.WriteLine("Writing data to file...");

                    //Write the new screen data to the data file
                    try
                    {
                        using (var fs = File.CreateText(Path.Combine(dataFilePath, dataFileName)))
                        {
                            fs.WriteLine(JsonSerializer.Serialize(newScreenData));
                        }
                    }
                    catch
                    {
                        Console.WriteLine("Failed to write to data file");
                    }

                    Console.WriteLine("File Written");


                    //Restart the meadow board if the wifi connnecting info has changed
                    if (client == null || newScreenData.Connection.WifiSSID != screenData.Connection.WifiSSID || newScreenData.Connection.WifiPassword != screenData.Connection.WifiPassword)
                    {
                        Console.WriteLine("Wifi Info Changed, Restarting...");
                        //Disconnect from Wifi

                        //Restart the board
                        Device.WatchdogEnable(TimeSpan.FromMilliseconds(1));
                    }
                    else
                    {
                        //Diconected from MQTT server so that we can reconnect with new info
                        client.DisconnectAsync();

                        screen.displayAlert = false;
                        screen.drawScreen();
                    }

                }
                else
                {
                    Console.WriteLine("Checksums Do Not Match");
                    haveHeaderData = false;
                    payloadLength = 0;
                    serialPort.ClearReceiveBuffer();
                    serialTimeoutTimer.Stop();
                    screen.displayAlert = false;
                    screen.drawScreen();
                    return;
                }
            }
            else
            {
                Console.WriteLine("Serial Error");
                haveHeaderData = false;
                payloadLength = 0;
                serialPort.ClearReceiveBuffer();
                screen.displayAlert = false;
                screen.drawScreen();
                return;
            }

        }

        /// <summary>
        /// Sends the screen data to the meadow board.
        /// </summary>
        private void SendDataPacket()
        {
            Console.WriteLine("Sending Screen Data...");

            //Serialize the screen data to a string
            string payload = JsonSerializer.Serialize(screen.screenData);

            //Create the data packet
            string packet = "##0";
            packet += payload.Length.ToString("0000");
            packet += payload;
            packet += ChecksumCalculator.ChecksumCalculator.CalculateChecksum(payload).ToString("0000");
            packet += "\n";

            //Send the data packet to the meadow board
            serialPort.Write(Encoding.UTF8.GetBytes(packet.ToCharArray()));
            Console.WriteLine("Screen Data Sent");
        }

        /// <summary>
        /// Executes when the switch page button is clicked.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SwitchPageButton_Clicked(object sender, EventArgs e)
        {
            //Switch to the next screen
            screen.currentScreen++;
            if (screen.currentScreen > screen.screenData.Indicators.GetLength(0) - 1)
            {
                screen.currentScreen = 0;
            }

            //Redraw the screen
            screen.drawScreen();
        }

        /// <summary>
        /// Connects to the wifi network with the connection data from the screen data.
        /// </summary>
        private async void connectToWifi()
        {
            //Connect to Wifi
            Resolver.Log.Info($"Connecting to Wifi SSID: {screen.screenData.Connection.WifiSSID}");
            screen.wifiStatusText = "Connecting to WiFi";
            screen.drawScreen();

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                await wifi.Connect(screen.screenData.Connection.WifiSSID, screen.screenData.Connection.WifiPassword, TimeSpan.FromSeconds(45), reconnection: Meadow.Gateway.WiFi.ReconnectionType.Automatic);
                wifi.NetworkConnected += Wifi_NetworkConnected;
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect to Wifi: : {ex.Message}");
                screen.wifiStatusText = "Failed to Connect to WiFi";
                screen.drawScreen();
            }
        }

        /// <summary>
        /// Executes when the meadow board connects to the wifi network.
        /// </summary>
        /// <param name="networkAdapter"></param>
        /// <param name="args"></param>
        private void Wifi_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs args)
        {

            screen.wifiStatusText = "Connected to WiFi";
            screen.drawScreen();
            Console.WriteLine("Connected to Wifi with:");
            Console.WriteLine($"IP Address: {networkAdapter.IpAddress}.");
            Console.WriteLine($"Subnet mask: {networkAdapter.SubnetMask}");
            Console.WriteLine($"Gateway: {networkAdapter.Gateway}");

            //Connect to MQTT server
            MQTT_Connect(screenData.Connection);
        }

        /// <summary>
        /// Connects to the MQTT broker.
        /// </summary>
        /// <param name="connection">The connection information</param>
        private async Task MQTT_Connect(ConnectionData connection)
        {
            Console.WriteLine("Connecting to MQTT server...");
            screen.mqttStatusText = "Connecting to MQTT";
            screen.drawScreen();

            //Create MQTT client
            MqttFactory mqttFactory = new MqttFactory();
            MqttClientOptions mqttClientOptions = (MqttClientOptions)new MqttClientOptionsBuilder()
                                    .WithClientId(Guid.NewGuid().ToString())
                                    .WithTcpServer(connection.MQTTHost, connection.MQTTPort)
                                    .WithClientId(connection.MQTTClientId)
                                    .WithCredentials(connection.MQTTUsername, connection.MQTTPassword)
                                    .WithCleanSession()
                                    .Build();

            //Disconnect from MQTT server if already connected
            if (client != null && client.IsConnected)
            {
                client.DisconnectAsync();
            }
            else
            {
                //Connect to MQTT server
                client = mqttFactory.CreateMqttClient();

                client.UseConnectedHandler(Client_ConnectedAsync);
                client.UseDisconnectedHandler(Client_DisconnectedAsync);

                await client.ConnectAsync(mqttClientOptions);
            }

        }

        /// <summary>
        /// Executes when the meadow board connects to the MQTT broker.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected to MQTT server");

            screen.mqttStatusText = "Connected to MQTT";
            screen.drawScreen();

            List<MqttTopicFilter> topicFilters = new List<MqttTopicFilter>();
            List<string> topicsSubscribed = new List<string>();

            //Subscribe to all topics in the screen data check for duplicates
            foreach (IndicatorData[] indicators in screen.screenData.Indicators)
            {
                foreach (IndicatorData indicator in indicators)
                {
                    if (!topicsSubscribed.Contains(indicator.Topic))
                    {
                        Console.WriteLine($"Subscribing to: {indicator.Topic}");
                        MqttTopicFilter objAdd = new MqttTopicFilter();
                        objAdd.Topic = indicator.Topic;
                        topicsSubscribed.Add(indicator.Topic);
                        topicFilters.Add(objAdd);
                    }
                    else
                    {
                        Console.WriteLine($"Duplicate Topic: {indicator.Topic}");
                    }
                }
            }

            //Subscribe to topics
            Console.WriteLine("Subscribing to Filters...");
            await client.SubscribeAsync(topicFilters.ToArray());
            client.UseApplicationMessageReceivedHandler(Client_ApplicationMessageReceivedHandler);
        }

        /// <summary>
        /// Executes when the meadow board disconnects from the MQTT broker.
        /// </summary>
        private async Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected from MQTT server");
            screen.mqttStatusText = "Disconnected from MQTT";
            screen.drawScreen();

            //Attempt to reconnect to MQTT server every 5 seconds
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await MQTT_Connect(screenData.Connection);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Reconnect failed {ex.Message}");
            }
        }

        /// <summary>
        /// Executes when a message is received from the MQTT broker.
        /// </summary>
        /// <param name="e"></param>
        /// <returns></returns>
        private Task Client_ApplicationMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine($"Message received on topic {e.ApplicationMessage.Topic}");
            Console.WriteLine($"Message: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

            //Update the indicators with the incoming data and topic
            screen.updateIndicatorValue(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");
            return base.Run();
        }

    }
}