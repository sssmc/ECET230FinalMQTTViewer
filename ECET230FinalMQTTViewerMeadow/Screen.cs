using System;
using System.Collections.Generic;
using System.Text;

using MQTTScreenData;

using Meadow.Foundation.Graphics;

namespace ECET230FinalMQTTViewerMeadow
{
    internal class Screen
    {
        public ScreenData screenData { get; set; }

        public string[][] indicatorValues { get; set; }

        public MicroGraphics graphics { get; set; }

        public int currentScreen { get; set; }

        public bool displayAlert { get; set; }

        public string alertText { get; set;}

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
                graphics.DrawText(5, 120, alertText);
            }
            else
            {
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
