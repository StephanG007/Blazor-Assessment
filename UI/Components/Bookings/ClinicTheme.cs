using System;

namespace UI.Components.Bookings;

internal static class ClinicTheme
{
    private const int ThemeCount = 4;

    public static string GetThemeClass(int clinicId)
    {
        var normalized = Math.Abs(clinicId);

        if (normalized == 0)
        {
            return "clinic-theme--1";
        }

        var index = normalized % ThemeCount;
        if (index == 0)
        {
            index = ThemeCount;
        }

        return $"clinic-theme--{index}";
    }
}
