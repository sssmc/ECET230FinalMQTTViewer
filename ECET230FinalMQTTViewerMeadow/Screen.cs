using MQTTScreenData;
using Meadow.Foundation.Graphics;

namespace ECET230FinalMQTTViewerMeadow
{
    /// <summary>
    /// Methods and properties needed to display data on a screen.
    /// </summary>
    internal class Screen
    {
        /// <summary>
        /// The screen data for the screen.
        /// </summary>
        public ScreenData screenData { get; set; }

        /// <summary>
        /// An 2D array of the current values of the indicators sorted by screen.
        /// </summary>
        public string[][] indicatorValues { get; set; }

        /// <summary>
        /// The MicroGraphics object used to draw on the TFT screen.
        /// </summary>
        public MicroGraphics graphics { get; set; }

        /// <summary>
        /// Index of the current displayed screen.
        /// </summary>
        public int currentScreen { get; set; }

        /// <summary>
        /// Should an alert be displayed on the screen.
        /// </summary>
        public bool displayAlert { get; set; }

        /// <summary>
        /// Text to be displayed in the alert.
        /// </summary>
        public string alertText { get; set; }

        /// <summary>
        /// Status of the WiFi connection.
        /// </summary>
        public string wifiStatusText { get; set; }

        /// <summary>
        /// Status of the MQTT connection.
        /// </summary>
        public string mqttStatusText { get; set; }


        //Fonts for the screen
        private IFont smallFont = new Font8x12();

        private IFont mediumFont = new Font12x16();

        private IFont largeFont = new Font12x20();

        /// <summary>
        /// Creates a new screen object.
        /// </summary>
        /// <param name="screenData">The screen data for the screen</param>
        /// <param name="graphics">The mirco graphics object for the TFT screen</param>
        public Screen(ScreenData screenData, MicroGraphics graphics)
        {
            this.screenData = screenData;
            this.graphics = graphics;
            this.currentScreen = 1;

            //Create the indicator values array
            this.indicatorValues = new string[screenData.Indicators.GetLength(0)][];
            for(int i = 0; i < screenData.Indicators.GetLength(0); i++)
            {
                indicatorValues[i] = new string[screenData.Indicators[i].Length];
            }

            this.displayAlert = false;
            this.alertText = "";
        }

        /// <summary>
        /// Draws the screen.
        /// </summary>
        public void drawScreen()
        {
            //Clear the screen
            graphics.Clear();

            //Draw the alert if needed
            if (displayAlert)
            {
                graphics.CurrentFont = largeFont;
                graphics.DrawText(5, 120, alertText);
            }
            else
            {

                //Draw Indicators on the current screen
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

            //Draw the screen
            graphics.Show();
        }

        /// <summary>
        /// Updates the correct indicator value based on the MQTT topic.
        /// </summary>
        /// <param name="topic">The MQTT topic</param>
        /// <param name="value">The data received on the topic</param>
        public void updateIndicatorValue(string topic, string value)
        {

            //For each indicator on the screen
            for(int i = 0; i < screenData.Indicators.GetLength(0); i++)
            {
                for(int j = 0; j < screenData.Indicators[i].Length; j++)
                {
                    //Check if incoming topic matches the indicator topic
                    if (screenData.Indicators[i][j].Topic == topic)
                    {
                        //Update the indicator value
                        indicatorValues[i][j] = value;
                    }
                }
            }

            //Redraw the screen
            drawScreen();
        }
    }
}
