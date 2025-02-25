using System.Diagnostics;
using System.Text.Json;

namespace bl2_loot_tracker;

static class Program
{
    private static Tracker _tracker;
    private static NotifyIcon _icon;
    
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        
        Settings settings = JsonSerializer.Deserialize<Settings>(File.ReadAllText("appsettings.json"));

        SetupAdditionalPaths(settings);
        
        _tracker = new Tracker(settings);
        
        _icon = new NotifyIcon();

        ContextMenuStrip menu = new ContextMenuStrip();
        menu.Items.Add("Open online tracker...", null, OpenLatest);
        menu.Items.Add("Exit", null, Exit);
        _icon.ContextMenuStrip = menu;
        
        _icon.Visible = true;
        _icon.Icon = new Icon("loot.ico");

        Application.Run();
    }
    
    private static void Exit(object? sender, EventArgs e)
    {
        _tracker.Shutdown();
        _icon.Visible = false;
        Application.Exit();
    }

    private static void OpenLatest(object? sender, EventArgs e)
    {
        string url = _tracker.GetLatestGist();
        if (!string.IsNullOrEmpty(url))
        {
            Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
        }
    }

    private static void SetupAdditionalPaths(Settings settings)
    {
        if (settings.AdditionalPaths == null)
        {
            settings.AdditionalPaths = new List<string>();
        }

        List<string> defaultPaths = new List<string>
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Borderlands 2\sdk_mods\LootRandomizer\Seeds",
            @"C:\Program Files (x86)\Steam\steamapps\common\BorderlandsPreSequel\sdk_mods\LootRandomizer\Seeds",
            @"C:\Program Files (x86)\Steam\steamapps\common\BorderlandsPreSequel\Binaries\Win32\Mods\LootRandomizer\Seeds"
        };
        
        settings.AdditionalPaths = defaultPaths.Union(settings.AdditionalPaths).ToList();
    }
}