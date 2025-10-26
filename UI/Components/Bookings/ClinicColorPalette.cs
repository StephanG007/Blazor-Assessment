using System;

namespace UI.Components.Bookings;

internal static class ClinicColorPalette
{
    private static readonly ClinicColorScheme[] Palette =
    {
        new("rgba(103, 80, 164, 0.24)", "rgba(103, 80, 164, 0.28)", "rgba(103, 80, 164, 0.55)"),
        new("rgba(58, 121, 180, 0.24)", "rgba(58, 121, 180, 0.28)", "rgba(58, 121, 180, 0.55)"),
        new("rgba(39, 125, 161, 0.24)", "rgba(39, 125, 161, 0.30)", "rgba(39, 125, 161, 0.60)"),
        new("rgba(120, 94, 240, 0.24)", "rgba(120, 94, 240, 0.28)", "rgba(120, 94, 240, 0.55)"),
        new("rgba(82, 121, 111, 0.24)", "rgba(82, 121, 111, 0.28)", "rgba(82, 121, 111, 0.55)"),
        new("rgba(136, 84, 108, 0.24)", "rgba(136, 84, 108, 0.28)", "rgba(136, 84, 108, 0.55)"),
        new("rgba(96, 108, 56, 0.24)", "rgba(96, 108, 56, 0.28)", "rgba(96, 108, 56, 0.55)"),
        new("rgba(52, 116, 123, 0.24)", "rgba(52, 116, 123, 0.28)", "rgba(52, 116, 123, 0.55)"),
    };

    public static ClinicColorScheme GetScheme(int clinicId)
    {
        if (Palette.Length == 0)
        {
            return ClinicColorScheme.Default;
        }

        var index = Math.Abs(clinicId) % Palette.Length;
        return Palette[index];
    }
}

internal sealed record ClinicColorScheme(string ShadowColor, string BorderColor, string AccentColor)
{
    public static ClinicColorScheme Default { get; } =
        new("rgba(103, 80, 164, 0.24)", "rgba(103, 80, 164, 0.28)", "rgba(103, 80, 164, 0.55)");

    public string ToCssVariables(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
        {
            prefix = "clinic";
        }

        return $"--{prefix}-shadow-color:{ShadowColor};--{prefix}-border-color:{BorderColor};--{prefix}-accent-color:{AccentColor};";
    }
}
