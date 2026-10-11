using System.Windows;

namespace SmartDoor.App.Themes;

// Ganti tema saat aplikasi jalan: Terang / Gelap / Custom.
public static class ThemeHelper
{
    public static void Apply(string name)
    {
        var file = name switch
        {
            "Gelap" => "Gelap",
            "Custom" => "Custom",
            _ => "Terang",
        };
        var dict = new ResourceDictionary
        {
            Source = new Uri($"Themes/{file}.xaml", UriKind.Relative)
        };
        var merged = Application.Current.Resources.MergedDictionaries;
        merged.Clear();
        merged.Add(dict);
    }
}
