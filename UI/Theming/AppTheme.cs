using MudBlazor;

namespace UI.Theming;

public static class AppTheme
{
    public static MudTheme LightTheme { get; } = new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = Colors.Blue.Darken3,
            Secondary = Colors.DeepPurple.Accent2,
            Background = Colors.Gray.Lighten5,
            Surface = Colors.Shades.White,
            AppbarBackground = Colors.Shades.White,
            AppbarText = Colors.Gray.Darken4
        },
        LayoutProperties = new LayoutProperties
        {
            DefaultBorderRadius = "10px"
        }
    };
}
