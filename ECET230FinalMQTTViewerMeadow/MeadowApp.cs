﻿//System
using System;
using System.Threading.Tasks;
using System.Text;

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
        /*--------------------------*/

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
                wifi.Connect("White Rabbit", "2511560A7196", TimeSpan.FromSeconds(45));
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

            /*--------------------------*/

            Draw_Screen();

            return base.Initialize();
        }

        private async void Wifi_NetworkConnected(INetworkAdapter networkAdapter, NetworkConnectionEventArgs args)
        {
          
            Console.WriteLine("Connected to Wifi with:");
            Console.WriteLine($"IP Address: {networkAdapter.IpAddress}.");
            Console.WriteLine($"Subnet mask: {networkAdapter.SubnetMask}");
            Console.WriteLine($"Gateway: {networkAdapter.Gateway}");

            await MQTT_Connect(testConnection);
        }

        private async Task MQTT_Connect(ConnectionData connection)
        {
            Console.WriteLine("Connecting to MQTT server...");

            MqttFactory mqttFactory = new MqttFactory();
            MqttClientOptions mqttClientOptions = (MqttClientOptions)new MqttClientOptionsBuilder()
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
            }

            Draw_Screen();

            return Task.CompletedTask;
        }

        private void Draw_Screen()
        {
            graphics.Clear();

            graphics.DrawText(5, 5, "Temperature: " + temp, Color.White);

            graphics.DrawText(5, 30, "Humidity: " + hum, Color.White);

            graphics.Show();
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