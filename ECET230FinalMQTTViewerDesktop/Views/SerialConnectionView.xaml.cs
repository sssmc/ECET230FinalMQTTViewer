using ECET230FinalMQTTViewerDesktop.Models;

namespace ECET230FinalMQTTViewerDesktop.Views;

public partial class SerialConnectionView : ContentPage
{
	public SerialConnectionView()
	{
		InitializeComponent();

		App.debugSerialConnection.DataReceived += DataSerialConnection_DataReceived;
	}

    private void DataSerialConnection_DataReceived(object sender, DataReceivedEventArgs e)
    {
		if(autoScrollCheckbox.IsChecked == true)
		{
           MainThread.BeginInvokeOnMainThread(ScrollView_Scroll);  
        }
    }

	private async void ScrollView_Scroll()
	{
        await scrollView.ScrollToAsync(0,1000000, true);
    }
}