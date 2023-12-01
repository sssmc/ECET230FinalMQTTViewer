using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Ports;
using System.Text;

namespace ECET230FinalMQTTViewerDesktop.Models
{

    /// <summary>
    /// Model for the serial connection behavior.
    /// </summary>
    public partial class SerialConnectionModel : ObservableObject
    {
        /// <summary>
        /// Serial port object.
        /// </summary>
        private SerialPort _serialPort;

        /// <summary>
        /// Status of the serial port.
        /// </summary>
        [ObservableProperty]
        private bool _comPortIsOpen;

        /// <summary>
        /// Text to display for the serial port status.
        /// </summary>
        [ObservableProperty]
        private string _connectionStatusText;

        /// <summary>
        /// All current com port names.
        public string[] comPortNames
        {
            get
            {
                return SerialPort.GetPortNames();
            }
        }


        /// <summary>
        /// Current Baud Rate.
        /// </summary>
        public int BaudRate
        {
            get
            {
                return _serialPort.BaudRate;
            }
            set
            {
                if(!_serialPort.IsOpen)
                {
                    _serialPort.BaudRate = value;
                }    
                
            }
        }

        /// <summary>
        /// Current com port name.
        /// </summary>
        public string comPortName
        {
            get
            {
                return _serialPort.PortName;
            }
            set
            {
                if(!_serialPort.IsOpen && value != null)
                {
                    _serialPort.PortName = value;
                }
                
            }
        }

        /// <summary>
        /// Serial port data received event.
        /// </summary>
        public event EventHandler<DataReceivedEventArgs> DataReceived;

        /// <summary>
        /// Com port opened event.
        /// </summary>
        public event EventHandler ComPortOpened;

        /// <summary>
        /// Com port closed event.
        /// </summary>
        public event EventHandler ComPortClosed;

        /// <summary>
        /// Creates a new serial connection model.
        /// </summary>
        /// <param name="useReadLine">Use readline to read from the serial port or read all bytes in the buffer</param>
        public SerialConnectionModel(bool useReadLine)
        {
            //Create the serial port object.
            _serialPort = new SerialPort();

            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            BaudRate = 4800;

            if (useReadLine)
            {
                _serialPort.DataReceived += SerialPort_DataReceivedReadLine;
            }
            else
            {
                _serialPort.DataReceived += SerialPort_DataReceived;
            }
        }

        /// <summary>
        /// Creates a new serial connection model.
        /// </summary>
        public SerialConnectionModel()
        {
            //Create the serial port object.
            _serialPort = new SerialPort();

            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            BaudRate = 4800;

            _serialPort.DataReceived += SerialPort_DataReceivedReadLine;
        }

        /// <summary>
        /// Executes when data is received from the serial port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            try
            {
                ConnectionStatusText = "Serial Port Receiving Data...";

                //Read all bytes in the buffer.
                byte[] buffer;
                _serialPort.Read(buffer = new byte[_serialPort.BytesToRead], 0, _serialPort.BytesToRead);

                //Convert the bytes to a string.
                args.data = Encoding.ASCII.GetString(buffer);

                DataReceived?.Invoke(this, args);
                ConnectionStatusText = "Serial Port Data Received";
            }
            catch
            {
                //Serial port was closed while reading
            }
        }

        /// <summary>
        /// Executes when data is received from the serial port.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void SerialPort_DataReceivedReadLine(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            try {
                ConnectionStatusText = "Serial Port Receiving Data...";

                //Read a line from the serial port.
                args.data = _serialPort.ReadLine();

                DataReceived?.Invoke(this, args);
                ConnectionStatusText = "Serial Port Data Received";
            }
            catch(System.OperationCanceledException) {
                //Serial port was closed while reading
            }
            
            
        }

        /// <summary>
        /// Opens the serial port.
        /// </summary>
        /// <returns>If the opening was sucessfull</returns>
        public bool OpenComPort()
        {
            try
            {
                _serialPort.Open();
                ComPortIsOpen = true;
                ConnectionStatusText = "Serial Port Connected";
                ComPortOpened?.Invoke(this, new EventArgs());
                return true;
            }
            catch
            {
                ComPortIsOpen = false;
                ConnectionStatusText = "Error Opening Serial Port";
                return false;
            }
        }

        /// <summary>
        /// Closes the serial port.
        /// </summary>
        /// <returns>If the closing was sucessfull</returns>
        public bool CloseComPort()
        {
            _serialPort.Close();
            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            ComPortClosed?.Invoke(this, new EventArgs());
            return true;
        }

        /// <summary>
        /// Writes a line to the serial port.
        /// </summary>
        /// <param name="data">Data to write</param>
        public void WriteLine(string data)
        {
            ConnectionStatusText = "Serial Port Sending Data...";
            if (_serialPort.IsOpen) {
                try
                {
                    _serialPort.WriteLine(data);
                    ConnectionStatusText = "Serial Port Data Sent";
                }
                catch
                {
                    ConnectionStatusText = "Error Sending Data";
                }
            }
            else
            {
                ConnectionStatusText = "Serial Port Not Connected";
            }
            

        }
    }

    /// <summary>
    /// Class for the data received event.
    /// </summary>
    public class DataReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Data received from the serial port.
        /// </summary>
        public string data { get; set; }
    }

}