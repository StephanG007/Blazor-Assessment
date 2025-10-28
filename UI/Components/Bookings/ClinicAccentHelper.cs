namespace UI.Components.Bookings;

public static class ClinicAccentHelper
{
    private const int AccentVariantCount = 4;
    private const string DefaultAccentClass = "clinic-card--accent-1";

    public static string GetAccentClass(int clinicId)
    {
        if (clinicId <= 0)
        {
            return DefaultAccentClass;
        }

        var accentIndex = ((clinicId - 1) % AccentVariantCount + AccentVariantCount) % AccentVariantCount;
        return $"clinic-card--accent-{accentIndex + 1}";
    }
}
