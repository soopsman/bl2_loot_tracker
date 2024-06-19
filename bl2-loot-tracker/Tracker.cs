using System.IO;
using System.Reflection;
using System.Text.Json;
using Octokit;

namespace bl2_loot_tracker;

public class Tracker
{
    private const string TRACKER_FILE_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Borderlands 2\Binaries\Win32\Mods\LootRandomizer\Seeds";
    private const string SEED_LIST = "Seed List.txt";
    private readonly FileSystemWatcher _watcher;
    private readonly Dictionary<string, GistInformation> _gists;
    private readonly string _storage;
    private readonly string _token;

    public Tracker(string token)
    {
        _watcher = new FileSystemWatcher(TRACKER_FILE_PATH);
        _watcher.Created += WatcherEvent;
        _watcher.Changed += WatcherEvent;
        _watcher.EnableRaisingEvents = true;

        _token = token;
        
        _storage = $@"{Path.GetDirectoryName(Assembly.GetEntryAssembly().Location)}\gists.json";
        if (File.Exists(_storage))
        {
            try
            {
                _gists = JsonSerializer.Deserialize<Dictionary<string, GistInformation>>(File.ReadAllText(_storage));
            }
            catch
            {
                _gists = new Dictionary<string, GistInformation>();
            }
        }
        else
        {
            _gists = new Dictionary<string, GistInformation>();
        }
    }

    public void Shutdown()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }
    
    private async void WatcherEvent(object sender, FileSystemEventArgs e)
    {
        if (e.FullPath.EndsWith(SEED_LIST))
        {
            return;
        }

        try
        {
            string seed = Path.GetFileNameWithoutExtension(e.FullPath);
            string trackerContents = File.ReadAllText(e.FullPath);

            GitHubClient client = new GitHubClient(new ProductHeaderValue("bl2-loot-randomizer-tracker"));
            client.Credentials = new Credentials(_token);

            Gist result;
            if (_gists.ContainsKey(seed))
            {
                result = await client.Gist.Edit(_gists[seed].Id, new GistUpdate {Description = $"Tracker for {seed}", Files = {{$"{seed}.txt", new GistFileUpdate {Content = trackerContents}}}});
            }
            else
            {
                result = await client.Gist.Create(new NewGist {Description = $"Tracker for {seed}", Public = true, Files = {{$"{seed}.txt", trackerContents}}});
            }

            _gists[seed] = new GistInformation {Id = result.Id, Url = result.HtmlUrl};

            File.WriteAllText(_storage, JsonSerializer.Serialize(_gists));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}