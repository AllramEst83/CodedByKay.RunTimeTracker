using MasterTemplate.Pages;
using System.Collections.ObjectModel;

namespace MasterTemplate
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(MainPage), typeof(MainPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));

            Shell.SetBackgroundColor(this, Color.FromHex("#f8a5c2"));
            Shell.SetForegroundColor(this, Color.FromHex("#303952"));

            BindingContext = this;
        }
    }
}
