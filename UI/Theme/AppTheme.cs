using MudBlazor;

namespace UI.Theme;

public static class AppTheme
{
    public static MudTheme Default { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#6750A4",
            Secondary = "#5E89FF",
            Tertiary = "#E45C9C",
            Info = "#3B8BEB",
            Success = "#4CAF50",
            Warning = "#F4A261",
            Error = "#E4572E",
            Background = "#F6F7FB",
            Surface = "#FFFFFF",
            SurfaceVariant = "#F1ECFF",
            AppbarBackground = "#FFFFFF",
            AppbarText = "#1C1B1F",
            DrawerBackground = "linear-gradient(180deg, #563B8D 0%, #201F45 100%)",
            DrawerText = "#F8F9FF",
            ActionDefault = "#6750A4",
            ActionDisabledBackground = "rgba(103, 80, 164, 0.08)",
            ActionDisabled = "rgba(28, 27, 31, 0.38)"
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "18px",
            DrawerWidthLeft = "280px"
        },
        Typography = new Typography
        {
            Default = new Default
            {
                FontFamily = new[] { "'Inter'", "'Segoe UI'", "sans-serif" },
                FontSize = "0.95rem"
            },
            H5 = new H5 { FontWeight = 600, LetterSpacing = "0.01em" },
            H6 = new H6 { FontWeight = 600, LetterSpacing = "0.01em" },
            Subtitle1 = new Subtitle1 { FontWeight = 600 },
            Subtitle2 = new Subtitle2 { FontWeight = 500 }
        }
    };
}
