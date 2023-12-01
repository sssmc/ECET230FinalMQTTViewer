using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ECET230FinalMQTTViewerDesktop.Models;

namespace ECET230FinalMQTTViewerDesktop.ViewModels
{

    /// <summary>
    /// View model for the serial connection view.
    /// </summary>
    [INotifyPropertyChanged]
    public partial class SerialConnectionViewModel
    {
        /// <summary>
        /// Reference to the data serial connection model.
        /// </summary>
        [ObservableProperty]
        private SerialConnectionModel _dataSerialConnection = App.dataSerialConnection;

        /// <summary>
        /// Reference to the debug serial connection model.
        /// </summary>
        [ObservableProperty]
        private SerialConnectionModel _debugSerialConnection = App.debugSerialConnection;

        /// <summary>
        /// Text to display on the data port open/close button.
        /// </summary>
        [ObservableProperty]
        private string _dataPortOpenCloseButtonText = "Open";

        /// <summary>
        /// The name of the data com port.
        /// </summary>
        [ObservableProperty]
        private string _dataPortName = "COM1";

        /// <summary>
        /// Event that is fired when the data port name is changed.
        /// </summary>
        /// <param name="value"></param>
        partial void OnDataPortNameChanged(string value)
        {
           App.dataSerialConnection.comPortName = value;
        }

        /// <summary>
        /// List of all the data com port names.
        /// </summary>
        public string[] DataPortNames
        {
            get
            {
                return App.dataSerialConnection.comPortNames;
            }
        }

        /// <summary>
        /// Text to display on the debug port open/close button.
        /// </summary>
        [ObservableProperty]
        private string _debugPortOpenCloseButtonText = "Open";

        /// <summary>
        /// Name of the debug com port.
        /// </summary>
        [ObservableProperty]
        private string _debugPortName = "COM1";
        
        /// <summary>
        /// Event that is fired when the debug port name is changed.
        /// </summary>
        /// <param name="value"></param>
        partial void OnDebugPortNameChanged(string value)
        {
            App.debugSerialConnection.comPortName = value;
        }

        /// <summary>
        /// List of all the debug com port names.
        /// </summary>
        public string[] DebugPortNames
        {
            get
            {
                return App.debugSerialConnection.comPortNames;
            }
        }

        /// <summary>
        /// Text to display the data received from the debug serial connection.
        /// </summary>
        [ObservableProperty]
        private string _debugPortReceivedData = "";
        public SerialConnectionViewModel()
        {
            App.debugSerialConnection.DataReceived += DebugSerialConnection_DataReceived;
            App.debugSerialConnection.BaudRate = 9600;
        }


        /// <summary>
        /// Event that is fired when data is received from the debug serial connection.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DebugSerialConnection_DataReceived(object sender, DataReceivedEventArgs e)
        {
            DebugPortReceivedData += (e.data);
        }

        /// <summary>
        /// Relay command to update the data port names.
        /// </summary>
        [RelayCommand]
        void DataPortUpdatePortNames()
        {
            OnPropertyChanged(nameof(DataPortNames));
        }

        /// <summary>
        /// Relay command to update the debug port names.
        /// </summary>
        [RelayCommand]
        void DebugPortUpdatePortNames()
        {
            OnPropertyChanged(nameof(DebugPortNames));
        }

        /// <summary>
        /// Relay command to open/close the debug serial connection.
        /// </summary>
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

        /// <summary>
        /// Relay command to open/close the data serial connection.
        /// </summary>
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

        /// <summary>
        /// Relay command to refresh all the com port names.
        /// </summary>
        [RelayCommand]
        void RefreshComPorts()
        {
            OnPropertyChanged(nameof(DataPortNames));
            OnPropertyChanged(nameof(DebugPortNames));
        }



    }
}
