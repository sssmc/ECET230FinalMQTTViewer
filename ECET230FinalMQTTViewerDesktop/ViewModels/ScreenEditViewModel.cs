using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECET230FinalMQTTViewerDesktop.Models;
using MQTTScreenData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECET230FinalMQTTViewerDesktop.ViewModels
{
    [INotifyPropertyChanged]
    partial class ScreenEditViewModel
    {

        private ScreenDataModel _screenDataModel;


        [ObservableProperty]
        private int _currentScreenIndex;

        [ObservableProperty]
        private int _currentIndicatorIndex;

        public string [] ScreenNames
        {
            get
            {
                string[] screenNames = new string[_screenDataModel.ScreenCount];
                for (int i = 0; i < _screenDataModel.ScreenCount; i++)
                {
                    screenNames[i] = $"Screen {i + 1}";
                }
                return screenNames;
            }
        }

        public string[] CurrentScreenIndicatorNames
        {
            get
            {
                string[] indicatorNames = new string[_screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex).Length];
                for(int i = 0; i < _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex).Length; i++)
                {
                    indicatorNames[i] = _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[i].Name;
                }
                return indicatorNames;
            }
        }

        public string CurrentIndicatorName
        {
            get
            {
                if(CurrentIndicatorIndex < 0)
                {
                    return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Name;
                }
                return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Name;
            }
            set
            {
                if(CurrentIndicatorIndex < 0)
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Name = value;

                }
                else
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Name = value;
                }
                
            }
        }

        public string CurrentIndicatorTopic
        {
            get
            {
                if(CurrentIndicatorIndex < 0)
                {
                    return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Topic;
                } 
                return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Topic;
            }
            set
            {
                if (CurrentIndicatorIndex < 0)
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Topic = value;
                }
                else
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Topic = value;
                }
            }
        }

        public string CurrentIndicatorType
        {
            get
            {
                if(CurrentIndicatorIndex < 0)
                {
                    return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Type;
                }
                return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Type;
            }
            set
            {
                if (CurrentIndicatorIndex < 0)
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].Type = value;
                }
                else
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].Type = value;
                }
            }
        }

        public int CurrentIndicatorMax
        {
            get
            {
                if(CurrentIndicatorIndex < 0)
                {
                    return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].MaxValue;
                }
                return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].MaxValue;
            }
            set
            {
                if (CurrentIndicatorIndex < 0)
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].MaxValue = value;
                }
                else
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].MaxValue = value;
                }
            }
        }

        public int CurrentIndicatorMin
        {
            get
            {
                if(CurrentIndicatorIndex < 0)
                {
                    return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].MinValue;
                }
                return _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].MinValue;
            }
            set
            {
                if (CurrentIndicatorIndex < 0)
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[0].MinValue = value;
                }
                else
                {
                    _screenDataModel.IndicatorsAtScreenIndex(CurrentScreenIndex)[CurrentIndicatorIndex].MinValue = value;
                }
            }
        }

        public string WiFiSSID
        {
            get
            {
                return _screenDataModel.Connection.WifiSSID;
            }
            set
            {
                _screenDataModel.Connection.WifiSSID = value;
            }
        }

        public string WiFiPassword
        {
            get
            {
                return _screenDataModel.Connection.WifiPassword;
            }
            set
            {
                _screenDataModel.Connection.WifiPassword = value;
            }
        }

        public string MQTTBrokerAddress
        {
            get
            {
                return _screenDataModel.Connection.MQTTHost;
            }
            set
            {
                _screenDataModel.Connection.MQTTHost = value;
            }
        }

        public int MQTTBrokerPort
        {
            get
            {
                return _screenDataModel.Connection.MQTTPort;
            }
            set
            {
                _screenDataModel.Connection.MQTTPort = value;
            }
        }

        public string MQTTBrokerUsername
        {
            get
            {
                return _screenDataModel.Connection.MQTTUsername;
            }
            set
            {
                _screenDataModel.Connection.MQTTUsername = value;
            }
        }

        public string MQTTBrokerPassword
        {
            get
            {
                return _screenDataModel.Connection.MQTTPassword;
            }
            set
            {
                _screenDataModel.Connection.MQTTPassword = value;
            }
        }

        public string MQTTClientID
        {
            get
            {
                return _screenDataModel.Connection.MQTTClientId;
            }
            set
            {
                _screenDataModel.Connection.MQTTClientId = value;
            }
        }

        public ScreenEditViewModel()
        {
            _screenDataModel = App.screenDataModel;
            _currentScreenIndex = 0;
            _screenDataModel.RequestScreenDataFromDevice();
            _screenDataModel.ScreenDataUpdated += _screenDataModel_ScreenDataUpdated;
        }

        private void _screenDataModel_ScreenDataUpdated(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentIndicatorName));
            OnPropertyChanged(nameof(CurrentIndicatorTopic));
            OnPropertyChanged(nameof(CurrentIndicatorType));
            OnPropertyChanged(nameof(CurrentIndicatorMax));
            OnPropertyChanged(nameof(CurrentIndicatorMin));
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
        }

        partial void OnCurrentScreenIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
            CurrentIndicatorIndex = 0;
        }

        partial void OnCurrentIndicatorIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CurrentIndicatorName));
            OnPropertyChanged(nameof(CurrentIndicatorTopic));
            OnPropertyChanged(nameof(CurrentIndicatorType));
            OnPropertyChanged(nameof(CurrentIndicatorMax));
            OnPropertyChanged(nameof(CurrentIndicatorMin));
        }

        [RelayCommand]
        void ProgramScreenData()
        {
            _screenDataModel.SendScreenDataToDevice();
        }
    }
}
