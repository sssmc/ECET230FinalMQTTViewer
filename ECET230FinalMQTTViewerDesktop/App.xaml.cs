namespace ECET230FinalMQTTViewerDesktop
{
    public partial class App : Application
    {

        public static ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel dataSerialConnection;
        public static ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel debugSerialConnection;
        public static ECET230FinalMQTTViewerDesktop.Models.ScreenDataModel screenDataModel;

        public App()
        {
            dataSerialConnection = new ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel();

            debugSerialConnection = new ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel();

            screenDataModel = new ECET230FinalMQTTViewerDesktop.Models.ScreenDataModel();

            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}