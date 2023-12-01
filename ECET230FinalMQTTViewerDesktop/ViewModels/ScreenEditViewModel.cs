using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using ECET230FinalMQTTViewerDesktop.Models;
using MQTTScreenData;

namespace ECET230FinalMQTTViewerDesktop.ViewModels
{

    /// <summary>
    /// View model for the screen edit view.
    /// </summary>
    partial class ScreenEditViewModel : ObservableValidator
    {

        /// <summary>
        /// Reference to the screen data model.
        /// </summary>
        [ObservableProperty]
        private ScreenDataModel _screenDataModel;

        /// <summary>
        /// Currently selected screen index.
        /// </summary>
        [ObservableProperty]
        public int _currentScreenIndex;

        /// <summary>
        /// The currently selected indicator.
        /// </summary>
        [ObservableProperty]
        private IndicatorData _selectedIndicator;

        /// <summary>
        /// Reference to the data serial connection model.
        /// </summary>
        [ObservableProperty]
        private SerialConnectionModel _serialConnectionModel = App.dataSerialConnection;

        /// <summary>
        /// A list of all the screen names.
        public string[] ScreenNames
        {
            get
            {
                string[] screenNames = new string[ScreenDataModel.ScreenCount];
                for (int i = 0; i < ScreenDataModel.ScreenCount; i++)
                {
                    //Generate a screen name from the screen index.
                    screenNames[i] = $"Screen {i + 1}";
                }
                return screenNames;
            }
        }

        /// <summary>
        /// The names of the indicators on the current screen.
        /// </summary>
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

        /// <summary>
        /// The indicators on the current screen.
        /// </summary>
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

        /// <summary>
        /// Executes when the screen data is updated.
        /// </summary>
        private void _screenDataModel_ScreenDataUpdated(object sender, EventArgs e)
        {    
            OnPropertyChanged(nameof(ScreenNames));
            OnPropertyChanged(nameof(CurrentScreenIndicators));
            OnPropertyChanged(nameof(ScreenDataModel)); 
            CurrentScreenIndex = 0; 
            SelectedIndicator = null;
        }

        /// <summary>
        /// Executes when the current screen index is changed.
        /// </summary>
        /// <param name="oldValue"></param>
        /// <param name="newValue"></param>
        partial void OnCurrentScreenIndexChanged(int oldValue, int newValue)
        {
            OnPropertyChanged(nameof(CurrentScreenIndicatorNames));
            OnPropertyChanged(nameof(CurrentScreenIndicators));

        }

        /// <summary>
        /// Relay command for programming the screen data to the device.
        /// </summary>
        [RelayCommand]
        void ProgramScreenData()
        {
            ScreenDataModel.SendScreenDataToDevice();
        }

        /// <summary>
        /// Relay command for requesting the screen data from the device.
        /// </summary>
        [RelayCommand]
        void RequestScreenData()
        {
            ScreenDataModel.RequestScreenDataFromDevice();
        }

        /// <summary>
        /// Relay command for adding the selected indicator to the current screen.
        /// </summary>
        [RelayCommand]
        void AddIndicator()
        {
            ScreenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, ScreenDataModel
                            .GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                            .Append(new IndicatorData("New Indicator", "New Indicator", "New Indicator", "numeric", 100, 0))
                            .ToArray());
            OnPropertyChanged(nameof(CurrentScreenIndicators));

        }

        /// <summary>
        /// Relay command for removing the selected indicator from the current screen.
        /// </summary>
        [RelayCommand]
        void RemoveIndicator()
        {
            List<IndicatorData> indicators = ScreenDataModel.GetIndicatorsAtScreenIndex(CurrentScreenIndex)
                                                             .ToList<IndicatorData>();

            indicators.Remove(SelectedIndicator);

            ScreenDataModel.SetIndicatorsAtScreenIndex(CurrentScreenIndex, indicators.ToArray());

            OnPropertyChanged(nameof(CurrentScreenIndicators));
        }

        /// <summary>
        /// Relay command to add a new screen.
        /// </summary>
        [RelayCommand]
        void AddScreen()
        {
            ScreenDataModel.AddScreen();
            OnPropertyChanged(nameof(ScreenNames));
        }

        /// <summary>
        /// Relay command to remove the current screen.
        /// </summary>
        [RelayCommand]
        void RemoveScreen()
        {
            ScreenDataModel.RemoveScreen(CurrentScreenIndex);
            OnPropertyChanged(nameof(ScreenNames));
        }
    }
}
