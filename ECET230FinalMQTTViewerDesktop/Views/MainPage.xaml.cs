using System.IO.Ports;

namespace ECET230FinalMQTTViewerDesktop
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        SerialPort serialPort;
        bool comPortIsOpen;

        public MainPage()
        {
            comPortIsOpen = false;

            serialPort = new SerialPort();

            InitializeComponent();

            comPortPicker.ItemsSource = SerialPort.GetPortNames();
            Loaded += MainPage_Loaded;

        }

        private void MainPage_Loaded(object sender, EventArgs e)
        {
            //Set Serial Port Perameters
            serialPort.BaudRate = 4800;
            serialPort.ReceivedBytesThreshold = 1;
            serialPort.DataReceived += SerialPort_DataRecevied;
        }

        private void SerialPort_DataRecevied(object sender, SerialDataReceivedEventArgs e)
        {
            //When we receive a line from the serial port
            string newPacket = serialPort.ReadLine();
            Console.WriteLine(newPacket);
            MainThread.BeginInvokeOnMainThread(MyMainThreadCode);
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

            int checksum = 0;

            char[] chars = new char[packet.Length];

            chars = entry.Text.ToCharArray();

            foreach(char c in chars)
            {
                checksum += c;
            }

            checksum %= 1000;

            packet += checksum.ToString("0000");

            Console.WriteLine($"Packet out: {packet}");
            serialPort.WriteLine(packet);
        }

        private void comPortStartButton_Clicked(object sender, EventArgs e)
        {
            //If we click the com port button
            if (comPortIsOpen)
            {
                //Close the port
                comPortIsOpen = false;
                serialPort.Close();
                comPortStartButton.Text = "Open";
            }
            else
            {
                //Open the port
                comPortIsOpen = true;
                serialPort.PortName = comPortPicker.SelectedItem.ToString();

                serialPort.Open();

                comPortStartButton.Text = "Close";
            }
        }
    }
}