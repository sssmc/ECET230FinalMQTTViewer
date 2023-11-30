using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Ports;
using System.Text;

namespace ECET230FinalMQTTViewerDesktop.Models
{
    public partial class SerialConnectionModel : ObservableObject
    {
        private SerialPort _serialPort;

        [ObservableProperty]
        private bool _comPortIsOpen;

        [ObservableProperty]
        private int _baudRate;

        [ObservableProperty]
        private string _connectionStatusText;

        public string[] comPortNames
        {
            get
            {
                return SerialPort.GetPortNames();
            }
        }

        public int baudRate
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

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public event EventHandler ComPortOpened;

        public event EventHandler ComPortClosed;

        public SerialConnectionModel(bool useReadLine)
        {
            _serialPort = new SerialPort();
            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            _baudRate = 4800;
            _serialPort.BaudRate = _baudRate;
            if (useReadLine)
            {
                _serialPort.DataReceived += SerialPort_DataReceivedReadLine;
            }
            else
            {
                _serialPort.DataReceived += SerialPort_DataReceived;
            }
        }

        public SerialConnectionModel()
        {
            _serialPort = new SerialPort();
            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            _baudRate = 4800;
            _serialPort.BaudRate = _baudRate;
            _serialPort.DataReceived += SerialPort_DataReceivedReadLine;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            try
            {
                ConnectionStatusText = "Serial Port Receiving Data...";
                byte[] buffer;
                _serialPort.Read(buffer = new byte[_serialPort.BytesToRead], 0, _serialPort.BytesToRead);
                args.data = Encoding.ASCII.GetString(buffer);
                DataReceived?.Invoke(this, args);
                ConnectionStatusText = "Serial Port Data Received";
            }
            catch
            {
                //Serial port was closed while reading
            }
        }

        private void SerialPort_DataReceivedReadLine(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            try {
                ConnectionStatusText = "Serial Port Receiving Data...";
                args.data = _serialPort.ReadLine();
                DataReceived?.Invoke(this, args);
                ConnectionStatusText = "Serial Port Data Received";
            }
            catch(System.OperationCanceledException) {
                //Serial port was closed while reading
            }
            
            
        }

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

        public bool CloseComPort()
        {
            _serialPort.Close();
            ComPortIsOpen = false;
            ConnectionStatusText = "Serial Port Not Connected";
            ComPortClosed?.Invoke(this, new EventArgs());
            return true;
        }
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

    public class DataReceivedEventArgs : EventArgs
    {
        public string data { get; set; }
    }

}