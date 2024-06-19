using System.IO;
using System.Text.Json;
using System.Windows;

namespace bl2_loot_tracker;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly Tracker _tracker;
    
    public MainWindow()
    {
        InitializeComponent();
        Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
        _tracker = new Tracker(settings.Token, settings.SeedsPath);
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _tracker.Shutdown();
        Close();
    }
}