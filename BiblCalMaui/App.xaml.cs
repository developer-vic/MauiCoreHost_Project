namespace BiblCalMaui;

public partial class App : Application
{
	public App()
	{
		InitializeComponent();
		
		// Force light mode on all platforms
		UserAppTheme = AppTheme.Light;
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(new AppShell());
		
		// Ensure light mode is applied to the window
		UserAppTheme = AppTheme.Light;
		
		return window;
	}
}