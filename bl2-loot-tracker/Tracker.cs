using System.IO;
using System.Text.Json;
using Octokit;

namespace bl2_loot_tracker;

public class Tracker
{
    private const string GISTS_FILE = "gists.json";
    private const string SEED_LIST = "Seed List.txt";
    private readonly FileSystemWatcher _watcher;
    private readonly Dictionary<string, GistInformation> _gists;
    private readonly string _token;

    public Tracker(string token, string path)
    {
        _watcher = new FileSystemWatcher(path);
        _watcher.Created += WatcherEvent;
        _watcher.Changed += WatcherEvent;

        _token = token;
        
        if (File.Exists(GISTS_FILE))
        {
            try
            {
                _gists = JsonSerializer.Deserialize<Dictionary<string, GistInformation>>(File.ReadAllText(GISTS_FILE));
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

        CheckExistingSeeds(path);
        
        _watcher.EnableRaisingEvents = true;
    }

    public void Shutdown()
    {
        _watcher.EnableRaisingEvents = false;
        _watcher.Dispose();
    }

    private void CheckExistingSeeds(string seedsPath)
    {
        foreach (string filePath in Directory.GetFiles(seedsPath))
        {
            string seed = Path.GetFileNameWithoutExtension(filePath);
            if (!_gists.ContainsKey(seed))
            {
                UpdateSeed(filePath);
            }
        }
    }
    
    private void WatcherEvent(object sender, FileSystemEventArgs e)
    {
        UpdateSeed(e.FullPath);
    }

    private async void UpdateSeed(string path)
    {
        if (path.EndsWith(SEED_LIST))
        {
            return;
        }
        
        try
        {
            string seed = Path.GetFileNameWithoutExtension(path);
            string trackerContents = File.ReadAllText(path);

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

            File.WriteAllText(GISTS_FILE, JsonSerializer.Serialize(_gists, new JsonSerializerOptions(JsonSerializerOptions.Default) {WriteIndented = true}));
        }
        catch (Exception exception)
        {
            Console.WriteLine(exception);
        }
    }
}