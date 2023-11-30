using System;
using System.Collections.Generic;
using System.Text;

using MQTTScreenData;

using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using MQTTnet.Client;

namespace ECET230FinalMQTTViewerMeadow
{
    internal class Screen
    {
        public ScreenData screenData { get; set; }

        public string[][] indicatorValues { get; set; }

        public MicroGraphics graphics { get; set; }

        public int currentScreen { get; set; }

        public bool displayAlert { get; set; }

        public string alertText { get; set; }

        public string wifiStatusText { get; set; }

        public string mqttStatusText { get; set; }


        private IFont smallFont = new Font8x12();

        private IFont mediumFont = new Font12x16();

        private IFont largeFont = new Font12x20();

        public Screen(ScreenData screenData, MicroGraphics graphics)
        {
            this.screenData = screenData;
            this.graphics = graphics;
            this.currentScreen = 1;
            this.indicatorValues = new string[screenData.Indicators.GetLength(0)][];
            for(int i = 0; i < screenData.Indicators.GetLength(0); i++)
            {
                indicatorValues[i] = new string[screenData.Indicators[i].Length];
            }
            this.displayAlert = false;
            this.alertText = "";
        }

        public void drawScreen()
        {
            graphics.Clear();

            if (displayAlert)
            {
                graphics.CurrentFont = largeFont;
                graphics.DrawText(5, 120, alertText);
            }
            else
            {

                //Draw Indicators
                graphics.CurrentFont = mediumFont;
                for (int i = 0; i < screenData.Indicators[currentScreen].Length; i++)
                {
                    if (indicatorValues[currentScreen][i] != null)
                    {
                        graphics.DrawText(5, i * 30, $"{screenData.Indicators[currentScreen][i].Name}: {indicatorValues[currentScreen][i]}");
                    }
                    else
                    {
                        graphics.DrawText(5, i * 30, $"{screenData.Indicators[currentScreen][i].Name}: ----");
                    }
                }
                //Draw WiFi and MQTT connection status in smaller font
                graphics.CurrentFont = smallFont;
                graphics.DrawText(5, 210, $"{wifiStatusText}");
                graphics.DrawText(5, 230, $"{mqttStatusText}");
            }

            

            graphics.Show();
        }

        public void updateIndicatorValue(string topic, string value)
        {
            for(int i = 0; i < screenData.Indicators.GetLength(0); i++)
            {
                for(int j = 0; j < screenData.Indicators[i].Length; j++)
                {
                    if (screenData.Indicators[i][j].Topic == topic)
                    {
                        indicatorValues[i][j] = value;
                    }
                }
            }

            drawScreen();
        }
    }
}
