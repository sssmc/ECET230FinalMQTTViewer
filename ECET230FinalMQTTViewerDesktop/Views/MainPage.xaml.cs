using System.IO.Ports;
using ChecksumCalculator;

namespace ECET230FinalMQTTViewerDesktop
{
    public partial class MainPage : ContentPage
    {

        public MainPage()
        {
            InitializeComponent();

            comPortPicker.ItemsSource = App.dataSerialConnection.comPortNames;

            Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            //Set Serial Port Perameters
            App.dataSerialConnection.baudRate = 4800;
            App.dataSerialConnection.DataReceived += DataSerialConnection_DataReceived;
        }

        private void DataSerialConnection_DataReceived(object sender, Models.DataReceivedEventArgs e)
        {
            Console.WriteLine(e.data);
        }

        private async void MyMainThreadCode()
        { 

        }

        private void OnCounterClicked(object sender, EventArgs e)
        {
            string packet = "##";
            packet += entry.Text.Length.ToString("0000");
            packet += entry.Text;

            //Calculate Checksum
            packet += ChecksumCalculator.ChecksumCalculator.CalculateChecksum(entry.Text).ToString("0000");

            Console.WriteLine($"Packet out: {packet}");
            App.dataSerialConnection.WriteLine(packet);
        }

        private void comPortStartButton_Clicked(object sender, EventArgs e)
        {
            //If we click the com port button
            if (App.dataSerialConnection.comPortIsOpen)
            {
                //Close the port
                App.dataSerialConnection.CloseComPort();
                comPortStartButton.Text = "Open";
            }
            else
            {
                //Open the port
                App.dataSerialConnection.comPortName = comPortPicker.SelectedItem.ToString();

                App.dataSerialConnection.OpenComPort();

                comPortStartButton.Text = "Close";
            }
        }
    }
}