namespace ECET230FinalMQTTViewerDesktop.Views;

public partial class ScreenEditView : ContentPage
{
	public ScreenEditView()
	{
		InitializeComponent();

		collection.SetBinding(ItemsView.ItemsSourceProperty, "IndicatorGroups");
	}
}