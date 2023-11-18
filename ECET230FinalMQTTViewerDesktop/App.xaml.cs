namespace ECET230FinalMQTTViewerDesktop
{
    public partial class App : Application
    {

        public static ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel dataSerialConnection;
        public App()
        {
            dataSerialConnection = new ECET230FinalMQTTViewerDesktop.Models.SerialConnectionModel();

            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}