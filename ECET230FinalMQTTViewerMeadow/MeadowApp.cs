//System
using System;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

using Meadow;
using Meadow.Devices;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Leds;
using Meadow.Peripherals.Displays;
using Meadow.Peripherals.Leds;


//MQTTnet
using MQTTnet.Client;
using MQTTnet.Client.Options;
using MQTTnet;
using MQTTnet.Client.Subscribing;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using Meadow.Hardware;
using System.IO.Ports;

//Internal Libs
using MQTTScreenData;
using MQTTSConnectionData;
using System.IO;

namespace ECET230FinalMQTTViewerMeadow
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;

        MicroGraphics graphics;
        Ili9341 display;

        ISerialMessagePort serialPort;

        IMqttClient client;

        /*-------Testing Data-------*/
        ConnectionData testConnection;

        ScreenData testScreen;

        string temp = "";

        string hum = "";

        string random1 = "";

        string random2 = "";
        /*--------------------------*/

        Screen screen;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            onboardLed = new RgbPwmLed(
                redPwmPin: Device.Pins.OnboardLedRed,
                greenPwmPin: Device.Pins.OnboardLedGreen,
                bluePwmPin: Device.Pins.OnboardLedBlue,
                CommonType.CommonAnode);

            Resolver.Log.Info("Create display driver instance");

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

            Resolver.Log.Info("Connecting to Wifi");

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                wifi.Connect("iPhone 13 mini (3)", "tbkQ-jxyE-i5I5-Ce8M", TimeSpan.FromSeconds(45));
                wifi.NetworkConnected += Wifi_NetworkConnected;
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect to Wifi: : {ex.Message}");
            }

            serialPort = Device.CreateSerialMessagePort(
                Device.PlatformOS.GetSerialPortName("COM1"),
                suffixDelimiter: Encoding.UTF8.GetBytes("\n"),
                preserveDelimiter: true);

            serialPort.MessageReceived += SerialPort_MessageReceived;
            serialPort.BaudRate = 115200;
            serialPort.Open();  

            /*-------Testing Data-------*/
            testConnection = new ConnectionData("ThingSpeak",
                                                "FDkPCxA2KTkHMgANKik6NgI",
                                                "mqtt3.thingspeak.com",
                                                1883,
                                                "FDkPCxA2KTkHMgANKik6NgI",
                                                "lRBFHoyhV9ruKuh0sy7s0QXm");

            IndicatorData tempIndicator = new IndicatorData("Temperature", "Temperature", "channels/2328115/subscribe/fields/field1", "numeric", 100, 0);
            IndicatorData humIndicator = new IndicatorData("Humidity", "Humidity", "channels/2328115/subscribe/fields/field2", "numeric", 100, 0);

            IndicatorData random1Indicator = new IndicatorData("Random1", "Random1", "channels/2328115/subscribe/fields/field3", "numeric", 10, 0);
            IndicatorData random2Indicator = new IndicatorData("Random2", "Random2", "channels/2328115/subscribe/fields/field4", "numeric", 10, 0);

            IndicatorData[][] indicators = new IndicatorData[2][];

            indicators[0] = new IndicatorData[] { tempIndicator, humIndicator };
            indicators[1] = new IndicatorData[] { random1Indicator, random2Indicator };

            testScreen = new ScreenData(testConnection, indicators);

            string jsonString = JsonSerializer.Serialize(testScreen);

            Console.WriteLine(jsonString);

            /*--------------------------*/

            screen = new Screen(testScreen, graphics);

            screen.drawScreen();

            //File loading

            string filePath = MeadowOS.FileSystem.DataDirectory;

            string fileName = "testScreen1.json";

            if (File.Exists(filePath + "/" + fileName))
            {
                Console.WriteLine("File Found!");

                try
                {
                    // Open the text file using a stream reader.
                    using (var sr = new StreamReader(filePath + "/" + fileName))
                    {
                        // Read the stream as a string, and write the string to the console.
                        string file = sr.ReadToEnd();
                        Console.WriteLine(file);
                        testScreen = new ScreenData();
                        testScreen = JsonSerializer.Deserialize<ScreenData>(file);
                    }
                }
                catch (IOException e)
                {
                    Console.WriteLine("The file could not be read:");
                    Console.WriteLine(e.Message);
                }

            }
            else
            {
                try
                {
                    Console.WriteLine("File not found, creating file");
                    using (var fs = File.CreateText(Path.Combine(filePath, fileName)))
                    {
                        fs.WriteLine(jsonString);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }

            return base.Initialize();
        }

        private async void Wifi_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs args)
        {
          
            Console.WriteLine("Connected to Wifi with:");
            Console.WriteLine($"IP Address: {networkAdapter.IpAddress}.");
            Console.WriteLine($"Subnet mask: {networkAdapter.SubnetMask}");
            Console.WriteLine($"Gateway: {networkAdapter.Gateway}");

            await MQTT_Connect(testScreen.Connection);
        }

        private async Task MQTT_Connect(ConnectionData connection)
        {
            Console.WriteLine("Connecting to MQTT server...");

            MqttFactory mqttFactory = new MqttFactory();
            MqttClientOptions mqttClientOptions = (MqttClientOptions)new MqttClientOptionsBuilder()
                                    .WithClientId(Guid.NewGuid().ToString())
                                    .WithTcpServer(connection.Host, connection.Port)
                                    .WithClientId(connection.ClientId)
                                    .WithCredentials(connection.Username, connection.Password)
                                    .WithCleanSession()
                                    .Build();

            client = mqttFactory.CreateMqttClient();
            
            client.UseConnectedHandler(Client_ConnectedAsync);
            client.UseDisconnectedHandler(Client_DisconnectedAsync);
            await client.ConnectAsync(mqttClientOptions);
        }

        private async Task Client_ConnectedAsync(MqttClientConnectedEventArgs e)
        {
            Console.WriteLine("Connected to MQTT server");
            var topicFilter = new MqttTopicFilterBuilder()
                                .WithTopic("channels/2328115/subscribe/fields/+")
                                .Build();
            await client.SubscribeAsync(topicFilter);
            client.UseApplicationMessageReceivedHandler(Client_ApplicationMessageReceivedHandler);
        }

        private async Task Client_DisconnectedAsync(MqttClientDisconnectedEventArgs e)
        {
            Console.WriteLine("Disconnected from MQTT server");
            await Task.Delay(TimeSpan.FromSeconds(5));
            try
            {
                await MQTT_Connect(testConnection);
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

            if(e.ApplicationMessage.Topic == "channels/2328115/subscribe/fields/field1")
            {
                temp = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            }
            else if (e.ApplicationMessage.Topic == "channels/2328115/subscribe/fields/field2")
            {
                hum = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            }else if(e.ApplicationMessage.Topic == "channels/2328115/subscribe/fields/field3")
            {
                random1 = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            }else if(e.ApplicationMessage.Topic == "channels/2328115/subscribe/fields/field4")
            {
                random2 = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
            }
           
            screen.updateIndicatorValue(e.ApplicationMessage.Topic, Encoding.UTF8.GetString(e.ApplicationMessage.Payload));

            return Task.CompletedTask;
        }

        void SerialPort_MessageReceived(object sender, SerialMessageData e)
        {
            
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            

            return base.Run();
        }
       
    }
}