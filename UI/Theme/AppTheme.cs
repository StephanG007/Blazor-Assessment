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
        }
    };
}
