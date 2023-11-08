//System
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

namespace ECET230FinalMQTTViewerMeadow
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        RgbPwmLed onboardLed;

        MicroGraphics graphics;
        Ili9341 display;

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
                chipSelectPin: Device.Pins.D13,
                dcPin: Device.Pins.D14,
                resetPin: Device.Pins.D15,
                width: 240, height: 320
            );

            graphics = new MicroGraphics(display)
            {
                IgnoreOutOfBoundsPixels = true,
                CurrentFont = new Font12x16()
            };

            Resolver.Log.Info("Connecting to Wifi");

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                wifi.Connect("IoT-Security", "B@kery204!", TimeSpan.FromSeconds(45));
                wifi.NetworkConnected += (networkAdapter, networkConnectionEventArgs) =>
                {
                    Console.WriteLine("Connected to Wifi with:");
                    Console.WriteLine($"IP Address: {networkAdapter.IpAddress}.");
                    Console.WriteLine($"Subnet mask: {networkAdapter.SubnetMask}");
                    Console.WriteLine($"Gateway: {networkAdapter.Gateway}");
                };
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Failed to Connect to Wifi: : {ex.Message}");
            }

            return base.Initialize();
        }

        public override Task Run()
        {
            Resolver.Log.Info("Run...");

            graphics.Clear();

            graphics.Rotation = RotationType._270Degrees;

            graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
            graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Color.Blue, false);
            graphics.DrawText(5, 5, "Meadow F7", Color.White);

            graphics.Show();

            return base.Run();
        }
       
    }
}