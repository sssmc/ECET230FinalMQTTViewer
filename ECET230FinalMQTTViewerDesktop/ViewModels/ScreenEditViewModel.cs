using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ECET230FinalMQTTViewerDesktop.Models;
using MQTTScreenData;

namespace ECET230FinalMQTTViewerDesktop.ViewModels
{
    partial class ScreenEditViewModel : ObservableValidator
    {

        [ObservableProperty]
        private ScreenDataModel _screenDataModel;

        [ObservableProperty]
        public int _currentScreenIndex;

        [ObservableProperty]
        private int _currentIndicatorIndex;

        [ObservableProperty]
        private IndicatorData _selectedIndicator;

        public string[] ScreenNames
        {
            get
            {
                string[] screenNames = new string[ScreenDataModel.ScreenCount];
                for (int i = 0; i < ScreenDataModel.ScreenCount; i++)
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
                string[] indicatorNames = new string[ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex).Length];
                for (int i = 0; i < ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex).Length; i++)
                {
                    indicatorNames[i] = ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex)[i].Name;
                }
                return indicatorNames;
            }
        }

        public List<IndicatorData> CurrentScreenIndicators
        {
            get
            {
                return ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex).ToList();
            }
        }

        public ScreenEditViewModel()
        {
            ScreenDataModel = App.screenDataModel;
            CurrentScreenIndex = 0;
            ScreenDataModel.ScreenDataUpdated += _screenDataModel_ScreenDataUpdated;
            ScreenDataModel.RequestScreenDataFromDevice();
        }

        private void _screenDataModel_ScreenDataUpdated(object sender, EventArgs e)
        {    
            OnPropertyChanged(nameof(ScreenNames));
            OnPropertyChanged(nameof(CurrentScreenIndicators));
            OnPropertyChanged(nameof(ScreenDataModel)); 
            CurrentScreenIndex = 0; 
            SelectedIndicator = null;
        }

        partial void OnCurrentScreenIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
            OnPropertyChanged(nameof(CurrentScreenIndicators));

        }

        [RelayCommand]
        void ProgramScreenData()
        {
            ScreenDataModel.SendScreenDataToDevice();
        }

        [RelayCommand]
        void RequestScreenData()
        {
            ScreenDataModel.RequestScreenDataFromDevice();
        }

        [RelayCommand]
        void AddIndicator()
        {
            ScreenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, ScreenDataModel
                            .GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                            .Append(new IndicatorData("New Indicator", "New Indicator", "New Indicator", "numeric", 100, 0))
                            .ToArray());
            OnPropertyChanged(nameof(CurrentScreenIndicators));

        }

        [RelayCommand]
        void RemoveIndicator()
        {
            List<IndicatorData> indicators = ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                                                             .ToList<IndicatorData>();

            indicators.Remove(SelectedIndicator);

            ScreenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, indicators.ToArray());

            OnPropertyChanged(nameof(CurrentScreenIndicators));
        }

        [RelayCommand]
        void AddScreen()
        {
            ScreenDataModel.AddScreen();
            OnPropertyChanged(nameof(ScreenNames));
        }

        [RelayCommand]
        void RemoveScreen()
        {
            ScreenDataModel.RemoveScreen(CurrentScreenIndex);
            OnPropertyChanged(nameof(ScreenNames));
        }
    }
}
