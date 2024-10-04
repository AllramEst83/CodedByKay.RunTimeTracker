using Android.Content;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using MasterTemplate;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Compatibility;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform.Compatibility;

[assembly: ExportRenderer(typeof(AppShell), typeof(MasterTemplate.Platforms.Android.AppShellRenderer))]
namespace MasterTemplate.Platforms.Android
{
    public class AppShellRenderer : ShellRenderer
    {
        public AppShellRenderer(Context context)
            : base(context)
        {
        }

        protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
        {
            return new MarginedTabBarAppearance(this, shellItem);
        }
    }

    public class MarginedTabBarAppearance : ShellBottomNavViewAppearanceTracker
    {
        public MarginedTabBarAppearance(IShellContext shellContext, ShellItem shellItem)
            : base(shellContext, shellItem)
        {
        }

        public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
        {
            base.SetAppearance(bottomView, appearance);

            // Adjust the bottom padding to add space
            int bottomPaddingInDp = 15;
            int topPaddingInDp = 12;

            float density = bottomView?.Context?.Resources?.DisplayMetrics?.Density ?? 1f;
            int bottomPaddingInPx = (int)(bottomPaddingInDp * density);
            int topPaddingInPx = (int)(topPaddingInDp * density);

            bottomView?.SetPadding(
                bottomView.PaddingLeft,
                topPaddingInPx,
                bottomView.PaddingRight,
                bottomPaddingInPx
            );
        }
    }
}
