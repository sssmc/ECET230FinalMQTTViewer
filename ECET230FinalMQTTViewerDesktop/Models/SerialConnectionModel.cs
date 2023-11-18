using System.IO.Ports;

namespace ECET230FinalMQTTViewerDesktop.Models
{
    public class SerialConnectionModel
    {
        private SerialPort _serialPort;

        private bool _comPortIsOpen;
        private int _baudRate;

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
                _serialPort.BaudRate = value;
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
                _serialPort.PortName = value;
            }
        }

        public bool comPortIsOpen
        {
            get
            {
                return _comPortIsOpen;
            }
        }

        public event EventHandler<DataReceivedEventArgs> DataReceived;

        public SerialConnectionModel()
        {
            _serialPort = new SerialPort();
            _comPortIsOpen = false;
            _baudRate = 9600;
            _serialPort.BaudRate = _baudRate;
            _serialPort.DataReceived += SerialPort_DataReceived;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            args.data = _serialPort.ReadLine();
            DataReceived?.Invoke(this, args);
        }

        public void OpenComPort()
        {
            _serialPort.PortName = comPortName;
            _serialPort.Open();
            _comPortIsOpen = true;
        }

        public void CloseComPort()
        {
            _serialPort.Close();
            _comPortIsOpen = false;
        }
        public void WriteLine(string data)
        {
            _serialPort.WriteLine(data);
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string data { get; set; }
    }

}
