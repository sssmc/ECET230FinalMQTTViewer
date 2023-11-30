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
        private SerialConnectionModel _dataSerialConnection = App.dataSerialConnection;

        [ObservableProperty]
        private SerialConnectionModel _debugSerialConnection = App.debugSerialConnection;

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
            App.debugSerialConnection.BaudRate = 9600;
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
            if (App.debugSerialConnection.ComPortIsOpen)
            {

                if (App.debugSerialConnection.CloseComPort())
                {
                    DebugPortOpenCloseButtonText = "Open";
                }else
                {
                    DebugPortOpenCloseButtonText = "Close";
                }
                
            }
            else
            {
                if(App.debugSerialConnection.OpenComPort())
                {
                    DebugPortOpenCloseButtonText = "Close";
                }else
                {
                    DebugPortOpenCloseButtonText = "Open";
                }
            }
        }

        [RelayCommand]
        void DataPortOpenClose()
        {
            if(App.dataSerialConnection.ComPortIsOpen)
            {

               if(App.dataSerialConnection.CloseComPort())
                {
                    DataPortOpenCloseButtonText = "Open";
                }else
                {
                    DataPortOpenCloseButtonText = "Close";
                }
            }
            else
            {
                if(App.dataSerialConnection.OpenComPort())
                {
                    DataPortOpenCloseButtonText = "Close";
                    App.screenDataModel.RequestScreenDataFromDevice();
                }
                else
                {
                    DataPortOpenCloseButtonText = "Open";
                }
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
