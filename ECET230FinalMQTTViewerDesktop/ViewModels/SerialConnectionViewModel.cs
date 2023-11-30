using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECET230FinalMQTTViewerDesktop.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace ECET230FinalMQTTViewerDesktop.ViewModels
{
    [INotifyPropertyChanged]
    public partial class SerialConnectionViewModel
    {
        [ObservableProperty]
        private string _dataPortOpenCloseButtonText = "Open";

        [ObservableProperty]
        private string _dataPortName = "COM1";

        partial void OnDataPortNameChanged(string value)
        {
           App.dataSerialConnection.comPortName = value;
        }

        public string[] DataPortNames
        {
            get
            {
                return App.dataSerialConnection.comPortNames;
            }
        }

        [ObservableProperty]
        private string _debugPortOpenCloseButtonText = "Open";

        [ObservableProperty]
        private string _debugPortName = "COM1";

        partial void OnDebugPortNameChanged(string value)
        {
            App.debugSerialConnection.comPortName = value;
        }

        public string[] DebugPortNames
        {
            get
            {
                return App.debugSerialConnection.comPortNames;
            }
        }

        [ObservableProperty]
        private string _debugPortReceivedData = "";

        public SerialConnectionViewModel()
        {
            App.debugSerialConnection.DataReceived += DebugSerialConnection_DataReceived;
            App.debugSerialConnection.baudRate = 9600;
        }

        private void DebugSerialConnection_DataReceived(object sender, DataReceivedEventArgs e)
        {
            DebugPortReceivedData += (e.data);
        }

        [RelayCommand]
        void DataPortUpdatePortNames()
        {
            OnPropertyChanged(nameof(DataPortNames));
        }

        [RelayCommand]
        void DebugPortUpdatePortNames()
        {
            OnPropertyChanged(nameof(DebugPortNames));
        }

        [RelayCommand]
        void DebugPortOpenClose()
        {
            if (App.debugSerialConnection.comPortIsOpen)
            {
                App.debugSerialConnection.CloseComPort();
                DebugPortOpenCloseButtonText = "Open";
            }
            else
            {
                App.debugSerialConnection.OpenComPort();
                DebugPortOpenCloseButtonText = "Close";
            }
        }

        [RelayCommand]
        void DataPortOpenClose()
        {
            if(App.dataSerialConnection.comPortIsOpen)
            {
                App.dataSerialConnection.CloseComPort();
                DataPortOpenCloseButtonText = "Open";
            }
            else
            {
                App.dataSerialConnection.OpenComPort();
                App.screenDataModel.RequestScreenDataFromDevice();
                DataPortOpenCloseButtonText = "Close";
            }
        }

        [RelayCommand]
        void RefreshComPorts()
        {
            OnPropertyChanged(nameof(DataPortNames));
            OnPropertyChanged(nameof(DebugPortNames));
        }



    }
}
