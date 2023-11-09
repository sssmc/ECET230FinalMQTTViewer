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

        public Screen(ScreenData screenData, MicroGraphics graphics)
        {
            this.screenData = screenData;
            this.graphics = graphics;
            this.currentScreen = 1;
            this.indicatorValues = new string[10][];
            for(int i = 0; i <10; i++)
            {
                indicatorValues[i] = new string[4];
            }
        }

        public void drawScreen()
        {
            graphics.Clear();

            for(int i =0; i<screenData.Indicators[currentScreen].Length; i++) 
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

            graphics.Show();
        }

        public void updateIndicatorValue(string topic, string value)
        {
            for(int i = 0; i < screenData.Indicators[currentScreen].Length; i++)
            {
                for(int j = 0; j< screenData.Indicators.GetLength(0); j++)
                {
                    if (screenData.Indicators[j][i].Topic == topic)
                    {
                        indicatorValues[j][i] = value;
                    }
                }
                
            }

            drawScreen();
        }
    }
}
