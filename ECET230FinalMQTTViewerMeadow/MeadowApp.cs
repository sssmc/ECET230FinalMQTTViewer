//System
using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;
using System.Collections.Generic;
using System.IO;
using System.Timers;

using Meadow;
using Meadow.Hardware;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;
using Meadow.Foundation.Sensors.Buttons;


//MQTTnet
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;


//Internal Libs
using MQTTScreenData;
using MQTTSConnectionData;
using ChecksumCalculator;
using System.Security.Cryptography.X509Certificates;

namespace ECET230FinalMQTTViewerMeadow
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;

        //TFT Display
        MicroGraphics graphics;
        Ili9341 display;

        //Serial Port
        ISerialPort serialPort;

        bool haveHeaderData = false;

        int payloadLength = 0;

        private static System.Timers.Timer serialTimeoutTimer;

        //MQTT client
        IMqttClient client;

        //Screen Data
        ScreenData screenData;
        Screen screen;

        PushButton switchPageButton;

        const string dataFileName = "testScreen12.json";

        string dataFilePath = MeadowOS.FileSystem.DataDirectory;

        public override Task Initialize()
    {
        Resolver.Log.Info("Initialize...");

        onboardLed = new RgbPwmLed(
            redPwmPin: Device.Pins.OnboardLedRed,
            greenPwmPin: Device.Pins.OnboardLedGreen,
            bluePwmPin: Device.Pins.OnboardLedBlue,
            CommonType.CommonAnode);

        onboardLed.SetColor(Color.Yellow);

        switchPageButton = new PushButton(Device.Pins.D10, ResistorMode.InternalPullUp, TimeSpan.FromMilliseconds(0.02));
        switchPageButton.Clicked += SwitchPageButton_Clicked;

        //Connect to TFT Display
        Resolver.Log.Info("Connecting to TFT...");

        display = new Ili9341
        (
            spiBus: Device.CreateSpiBus(),
            chipSelectPin: Device.Pins.D11,
            dcPin: Device.Pins.D14,
            resetPin: Device.Pins.D15,
            width: 240, height: 320
        );

        graphics = new MicroGraphics(display)
        {
            IgnoreOutOfBoundsPixels = true,
            CurrentFont = new Font12x16()
        };

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

        IndicatorData[][] indicators = new IndicatorData[2][];

        indicators[0] = new IndicatorData[] { tempIndicator, humIndicator };
        indicators[1] = new IndicatorData[] { random1Indicator, random2Indicator, tempIndicator, humIndicator };

        ScreenData defaultScreenData = new ScreenData(testConnection, indicators);

        //File loading

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

        //Create Screen object
        screen = new Screen(screenData, graphics);

        screen.drawScreen();

        //Connect to Wifi
        connectToWifi();

        //Create Serial Port
        serialPort = Device.PlatformOS.GetSerialPortName("Com1")
                                        .CreateSerialPort(baudRate: 4800,
                                        readBufferSize: 2048);
        serialPort.Open();
        serialPort.DataReceived += SerialPort_MessageReceived;

        serialTimeoutTimer = new Timer(6000);

        serialTimeoutTimer.Elapsed += OnSerialTimeout;


        onboardLed.SetColor(Color.Green);

        return base.Initialize();
        }

        private void OnSerialTimeout(object sender, ElapsedEventArgs e)
        {
            Console.WriteLine("Serial Timeout");
            haveHeaderData = false;
            payloadLength = 0;
            serialPort.ClearReceiveBuffer();
            serialTimeoutTimer.Stop();
        }

        private void SerialPort_MessageReceived(object sender, SerialDataReceivedEventArgs e)
        {
            
            if(serialTimeoutTimer.Enabled == false)
            {
                serialTimeoutTimer.Start();
            }
            else
            {
                serialTimeoutTimer.Stop();
                serialTimeoutTimer.Start();
            }

            if (serialPort.BytesToRead < 7 && haveHeaderData == false)
            {
                return;
            }
            else if (serialPort.BytesToRead >= 7 && haveHeaderData == false)
            {
                Console.WriteLine("Reading header data...");

                screen.alertText = "Receving Data";
                screen.displayAlert = true;
                screen.drawScreen();

                byte[] response = new byte[7];
                serialPort.Read(response, 0, 7);

                Console.WriteLine("Header received: ");
                string data = Encoding.UTF8.GetString(response, 0, response.Length);
                Console.WriteLine(data);

                if (data.Substring(0, 2) == "##")
                {
                    if(data.Substring(2,1) == "0")
                    {
                        payloadLength = int.Parse(data.Substring(3, 4)) + 4;
                        Console.WriteLine($"Payload Length: {payloadLength}");
                        haveHeaderData = true;
                    }else if(data.Substring(2, 1) == "1")
                    {
                        Console.WriteLine("Data Request Packet Recevied");
                        SendDataPacket();
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
        
            if(haveHeaderData && serialPort.BytesToRead < payloadLength)
            {
                return;
            }
            else if(haveHeaderData && serialPort.BytesToRead >= payloadLength)
            {
                Console.WriteLine("Reading payload data...");
                byte[] dataBytes = new byte[payloadLength];

                serialPort.Read(dataBytes, 0, payloadLength);

                string data = Encoding.UTF8.GetString(dataBytes, 0, dataBytes.Length);
                string payload = data.Substring(0, dataBytes.Length - 4);

                Console.WriteLine("Payload received: ");
                Console.WriteLine(payload);

                Console.Write("Checksum Received: ");
                int checksumReceived = int.Parse(data.Substring(data.Length - 4, 4));
                Console.WriteLine(checksumReceived.ToString());

                Console.Write("Checksum Calculated: ");
                int calculatedChecksum = ChecksumCalculator.ChecksumCalculator.CalculateChecksum(data.Substring(0, data.Length - 4));
                Console.WriteLine(calculatedChecksum.ToString());

                if (calculatedChecksum == checksumReceived)
                {
                    Console.WriteLine("Checksums Match");

                    haveHeaderData = false;
                    payloadLength = 0;
                    serialPort.ClearReceiveBuffer();
                    serialTimeoutTimer.Stop();

                    Console.WriteLine("Updating Screen...");
                    ScreenData newScreenData = new ScreenData();
                    newScreenData = JsonSerializer.Deserialize<ScreenData>(payload);
                    screen.screenData = newScreenData;
                    screen.drawScreen();
                    Console.WriteLine("Screen Updated");
                    Console.WriteLine("Writing data to file...");

                    try {
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
                    if(client == null || newScreenData.Connection.WifiSSID != screenData.Connection.WifiSSID || newScreenData.Connection.WifiPassword != screenData.Connection.WifiPassword)
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

        private void SendDataPacket()
        {
            Console.WriteLine("Sending Screen Data...");
            string payload = JsonSerializer.Serialize(screenData);
            string packet = "##0";
            packet += payload.Length.ToString("0000");
            packet += payload;
            packet += ChecksumCalculator.ChecksumCalculator.CalculateChecksum(payload).ToString("0000");
            packet += "\n";
            serialPort.Write(Encoding.UTF8.GetBytes(packet.ToCharArray()));
            Console.WriteLine("Screen Data Sent");
        }

        private void SwitchPageButton_Clicked(object sender, EventArgs e)
        {
            screen.currentScreen++;
            if (screen.currentScreen > 1)
            {
                screen.currentScreen = 0;
            }
            screen.drawScreen();
        }

        private async void disconnectFromWifi()
        {
            //Disconnect from Wifi
            Resolver.Log.Info($"Disconnecting from Wifi SSID: {screen.screenData.Connection.WifiSSID}");

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                await wifi.Disconnect(false);
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Disconnect from Wifi: : {ex.Message}");
            }
        }

        private async void connectToWifi()
        {
            //Connect to Wifi
            Resolver.Log.Info($"Connecting to Wifi SSID: {screen.screenData.Connection.WifiSSID}");

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                await wifi.Connect(screen.screenData.Connection.WifiSSID, screen.screenData.Connection.WifiPassword, TimeSpan.FromSeconds(45),reconnection:Meadow.Gateway.WiFi.ReconnectionType.Automatic);
                wifi.NetworkConnected += Wifi_NetworkConnected;
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect to Wifi: : {ex.Message}");
            }
        }
        private void Wifi_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs args)
        {
          
            Console.WriteLine("Connected to Wifi with:");
            Console.WriteLine($"IP Address: {networkAdapter.IpAddress}.");
            Console.WriteLine($"Subnet mask: {networkAdapter.SubnetMask}");
            Console.WriteLine($"Gateway: {networkAdapter.Gateway}");

            MQTT_Connect(screenData.Connection);
        }

        private async Task MQTT_Connect(ConnectionData connection)
        {
            Console.WriteLine("Connecting to MQTT server...");

            MqttFactory mqttFactory = new MqttFactory();
            MqttClientOptions mqttClientOptions = (MqttClientOptions)new MqttClientOptionsBuilder()
                                    .WithClientId(Guid.NewGuid().ToString())
                                    .WithTcpServer(connection.MQTTHost, connection.MQTTPort)
                                    .WithClientId(connection.MQTTClientId)
                                    .WithCredentials(connection.MQTTUsername, connection.MQTTPassword)
                                    .WithCleanSession()
                                    .Build();

            if (client != null && client.IsConnected)
            {
                client.DisconnectAsync();
            }
            else
            {
                client = mqttFactory.CreateMqttClient();

                client.UseConnectedHandler(Client_ConnectedAsync);
                client.UseDisconnectedHandler(Client_DisconnectedAsync);

                await client.ConnectAsync(mqttClientOptions);
            }

            

            
            
            
        }

        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected to MQTT server");

            List<MqttTopicFilter> topicFilters = new List<MqttTopicFilter>();
            List<string> topicsSubscribed = new List<string>();

            foreach (IndicatorData[] indicators in screen.screenData.Indicators)
            {
                foreach(IndicatorData indicator in indicators)
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

            Console.WriteLine("Subscribing to Filters...");
            await client.SubscribeAsync(topicFilters.ToArray());
            client.UseApplicationMessageReceivedHandler(Client_ApplicationMessageReceivedHandler);
        }

        private async Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected from MQTT server");
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

        private Task Client_ApplicationMessageReceivedHandler(MqttApplicationMessageReceivedEventArgs e)
        {
            Console.WriteLine($"Message received on topic {e.ApplicationMessage.Topic}");
            Console.WriteLine($"Message: {Encoding.UTF8.GetString(e.ApplicationMessage.Payload)}");

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