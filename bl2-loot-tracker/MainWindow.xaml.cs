using System.IO;
using System.Reflection;
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
        _tracker = new Tracker(File.ReadAllText($@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\token.txt"));
    }

    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        _tracker.Shutdown();
        Close();
    }
}