﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.IO.Ports;
using System.Text;

namespace ECET230FinalMQTTViewerDesktop.Models
{
    public partial class SerialConnectionModel : ObservableObject
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
                if(!_serialPort.IsOpen)
                {
                    _serialPort.PortName = value;
                }
                
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

        public event EventHandler ComPortOpened;

        public event EventHandler ComPortClosed;

        public SerialConnectionModel(bool useReadLine)
        {
            _serialPort = new SerialPort();
            _comPortIsOpen = false;
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
            _comPortIsOpen = false;
            _baudRate = 4800;
            _serialPort.BaudRate = _baudRate;
            _serialPort.DataReceived += SerialPort_DataReceivedReadLine;
        }

        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            DataReceivedEventArgs args = new DataReceivedEventArgs();
            try
            {

                byte[] buffer;
                _serialPort.Read(buffer = new byte[_serialPort.BytesToRead], 0, _serialPort.BytesToRead);
                args.data = Encoding.ASCII.GetString(buffer);
                DataReceived?.Invoke(this, args);
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
                args.data = _serialPort.ReadLine();
                DataReceived?.Invoke(this, args);
            }
            catch(System.OperationCanceledException) {
                //Serial port was closed while reading
            }
            
            
        }

        public void OpenComPort()
        {
            _serialPort.PortName = comPortName;
            _serialPort.Open();
            _comPortIsOpen = true;
            ComPortOpened?.Invoke(this, new EventArgs());
        }

        public void CloseComPort()
        {
            _serialPort.Close();
            _comPortIsOpen = false;
            ComPortClosed?.Invoke(this, new EventArgs());
        }
        public void WriteLine(string data)
        {
            if(_serialPort.IsOpen) {
                _serialPort.WriteLine(data);
            }
            
        }
    }

    public class DataReceivedEventArgs : EventArgs
    {
        public string data { get; set; }
    }

}