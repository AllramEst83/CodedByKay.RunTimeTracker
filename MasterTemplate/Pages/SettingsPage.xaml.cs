using AndroidX.Lifecycle;
using MasterTemplate.ViewModels;

namespace MasterTemplate.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel? viewModel;

    public SettingsPage()
	{
		InitializeComponent();

        if (Application.Current?.Handler?.MauiContext?.Services is not null)
        {
            viewModel = Application.Current.Handler.MauiContext.Services.GetService<SettingsViewModel>();
            if (viewModel is null)
            {
                throw new InvalidOperationException("SettingsViewModel service not found.");
            }
        }
        else
        {
            throw new InvalidOperationException("Unable to access services.");
        }

        this.BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // Called when the page is about to become visible
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Called when the page is no longer visible
    }
}