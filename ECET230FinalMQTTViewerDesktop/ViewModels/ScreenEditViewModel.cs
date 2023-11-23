using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECET230FinalMQTTViewerDesktop.Models;
using MQTTScreenData;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECET230FinalMQTTViewerDesktop.ViewModels
{
    partial class ScreenEditViewModel : ObservableValidator
    {

        public ScreenDataModel _screenDataModel { get; set; }


        [ObservableProperty]
        [Required]
        [Range(0, int.MaxValue)]
        public int _currentScreenIndex;

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
                string[] indicatorNames = new string[_screenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex).Length];
                for(int i = 0; i < _screenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex).Length; i++)
                {
                    indicatorNames[i] = _screenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex)[i].Name;
                }
                return indicatorNames;
            }
        }

        public string CurrentIndicatorName
        {
            get
            {
                return _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Name;
            }
            set
            {

                _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Name = value;
                
            }
        }

        public string CurrentIndicatorTopic
        {
            get
            {
                return _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Topic;
            }
            set
            {

                _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Topic = value;
            }
        }

        public string CurrentIndicatorType
        {
            get
            {
                return _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Type;
            }
            set
            {

                _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].Type = value;
            }
        }

        public int CurrentIndicatorMax
        {
            get
            {
                return _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].MaxValue;
            }
            set
            {
                _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].MaxValue = value;
            }
        }

        public int CurrentIndicatorMin
        {
            get
            {
                return _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].MinValue;
            }
            set
            {
                 _screenDataModel.GetIndicatorsAtScreenIndex(Math.Clamp(CurrentScreenIndex, 0, int.MaxValue))[Math.Clamp(CurrentIndicatorIndex, 0, int.MaxValue)].MinValue = value;
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

        public List<IndicatorGroup> IndicatorGroups { get; set; } = new List<IndicatorGroup>();


        public ScreenEditViewModel()
        {
            _screenDataModel = App.screenDataModel;
            _currentScreenIndex = 0;
            _screenDataModel.ScreenDataUpdated += _screenDataModel_ScreenDataUpdated;
            _screenDataModel.RequestScreenDataFromDevice();
    }

        private void _screenDataModel_ScreenDataUpdated(object sender, EventArgs e)
        {
            OnPropertyChanged(nameof(CurrentIndicatorName));
            OnPropertyChanged(nameof(CurrentIndicatorTopic));
            OnPropertyChanged(nameof(CurrentIndicatorType));
            OnPropertyChanged(nameof(CurrentIndicatorMax));
            OnPropertyChanged(nameof(CurrentIndicatorMin));
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
            OnPropertyChanged(nameof(ScreenNames));
            OnPropertyChanged(nameof(WiFiSSID));
            OnPropertyChanged(nameof(WiFiPassword));
            OnPropertyChanged(nameof(MQTTBrokerAddress));
            OnPropertyChanged(nameof(MQTTBrokerPort));
            OnPropertyChanged(nameof(MQTTBrokerUsername));
            OnPropertyChanged(nameof(MQTTBrokerPassword));
            OnPropertyChanged(nameof(MQTTClientID));

            
            IndicatorGroups.Clear();
            for(int i = 0; i < _screenDataModel.ScreenCount; i++)
            {
                IndicatorGroups.Add(new IndicatorGroup($"Screen {i + 1}", _screenDataModel.GetIndicatorsAtScreenIndex(i).ToList()));
            }

            OnPropertyChanged(nameof(IndicatorGroups));
            

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

        [RelayCommand]
        void RequestScreenData()
        {
            _screenDataModel.RequestScreenDataFromDevice();
        }

        [RelayCommand]
        void AddIndicator()
        {
            _screenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, _screenDataModel
                            .GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                            .Append(new IndicatorData("New Indicator", "New Indicator", "New Indicator", "numeric", 100, 0))
                            .ToArray());
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
        }

        [RelayCommand]
        void RemoveIndicator()
        {
            List<IndicatorData> indicators = _screenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                                                             .ToList<IndicatorData>();
            indicators.RemoveAt(CurrentIndicatorIndex);
            _screenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, indicators.ToArray());
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
        }

        [RelayCommand]
        void AddScreen()
        {
            _screenDataModel.AddScreen();
            OnPropertyChanged(nameof(ScreenNames));
        }

        [RelayCommand]
        void RemoveScreen()
        {
            _screenDataModel.RemoveScreen(CurrentScreenIndex);
            OnPropertyChanged(nameof(ScreenNames));
        }

        public class IndicatorGroup : List<IndicatorData>
        {
            public string Name { get; private set; }

            public IndicatorGroup(string name, List<IndicatorData> indicators) : base(indicators)
            {
                Name = name;
            }
        }
    }
}
