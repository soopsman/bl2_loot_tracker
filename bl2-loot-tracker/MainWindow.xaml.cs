using System.Diagnostics;
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
    private readonly NotifyIcon _icon;

    public MainWindow()
    {
        InitializeComponent();
        Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));
        _tracker = new Tracker(settings);

        _icon = new NotifyIcon();
        _icon.Icon = new Icon("loot.ico");
        _icon.Visible = true;

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Open latest Gist...", null, OpenLatest);
        menu.Items.Add("Exit", null, Exit);
        _icon.ContextMenuStrip = menu;
        _icon.Visible = true;
        
        Hide();
    }

    private void Exit(object? sender, EventArgs e)
    {
        _tracker.Shutdown();
        _icon.Visible = false;
        Close();
    }

    private void OpenLatest(object? sender, EventArgs e)
    {
        string url = _tracker.GetLatestGist();
        if (!string.IsNullOrEmpty(url))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }
}