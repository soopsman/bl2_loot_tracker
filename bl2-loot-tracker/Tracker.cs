using System.Text.Json;
using Octokit;

namespace bl2_loot_tracker;

public class Tracker
{
    private const string GISTS_FILE = "gists.json";
    private const string SEED_LIST = "Seed List.txt";
    private readonly List<FileSystemWatcher> _watchers;
    private readonly Dictionary<string, GistInformation> _gists;
    private readonly Settings _settings;

    public Tracker(Settings settings)
    {
        _settings = settings;
        
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

        _watchers = new List<FileSystemWatcher>();

        if (Directory.Exists(settings.SeedsPath))
        {
            FileSystemWatcher watcher = new FileSystemWatcher(settings.SeedsPath);
            watcher.Created += WatcherEvent;
            watcher.Changed += WatcherEvent;
            watcher.EnableRaisingEvents = true;
            _watchers.Add(watcher);
        }

        if (settings.AdditionalPaths != null)
        {
            _watchers.AddRange(settings.AdditionalPaths.Where(Directory.Exists).Select(path =>
            {
                FileSystemWatcher additionalWatcher = new FileSystemWatcher(path);
                additionalWatcher.Created += WatcherEvent;
                additionalWatcher.Changed += WatcherEvent;
                additionalWatcher.EnableRaisingEvents = true;
                return additionalWatcher;
            }));
        }

        CheckExistingSeeds();
    }

    public void Shutdown()
    {
        foreach (FileSystemWatcher watcher in _watchers)
        {
            watcher.EnableRaisingEvents = false;
            watcher.Dispose();
        }
    }

    public string GetLatestGist()
    {
        if (_gists.Count == 0)
        {
            return null;
        }

        string latestSeed = Path.GetFileNameWithoutExtension(GetLatestSeed());

        return _gists.ContainsKey(latestSeed) ? _gists[latestSeed].Url : _gists.Last().Value.Url;
    }

    private void CheckExistingSeeds()
    {
        if (_settings.UseSingleGist)
        {
            string latestSeed = GetLatestSeed();

            if (!string.IsNullOrEmpty(latestSeed))
            {
                UpdateSeed(latestSeed);
            }
        }
        else
        {
            foreach (string path in _watchers.Select(watcher => watcher.Path))
            {
                foreach (string filePath in Directory.GetFiles(path))
                {
                    UpdateSeed(filePath);
                }
            }
        }
    }

    private string GetLatestSeed()
    {
        List<string> files = new List<string>();
        foreach (string path in _watchers.Select(watcher => watcher.Path))
        {
            files.AddRange(Directory.GetFiles(path));
        }
        
        return files.Where(seed => !seed.EndsWith(SEED_LIST))
            .Aggregate((seed1, seed2) => new FileInfo(seed1).LastWriteTime > new FileInfo(seed2).LastWriteTime ? seed1 : seed2);
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
            client.Credentials = new Credentials(_settings.Token);

            Gist result;
            if (_gists.ContainsKey(seed))
            {
                result = await client.Gist.Edit(_gists[seed].Id, new GistUpdate {Description = $"Tracker for {seed}", 
                    Files = {{$"{seed}.txt", new GistFileUpdate {Content = trackerContents}}}});
            }
            else if (_settings.UseSingleGist && _gists.Count > 0)
            {
                string gistId = _gists.Last().Value.Id;
                string previousSeed = _gists.Last().Key;
                
                result = await client.Gist.Edit(gistId, new GistUpdate {Description = $"Tracker for {seed}", Files =
                    {
                        {$"{seed}.txt", new GistFileUpdate {Content = trackerContents}},
                        {$"{previousSeed}.txt", new GistFileUpdate {Content = string.Empty}}
                    }
                });
            }
            else
            {
                result = await client.Gist.Create(new NewGist {Description = $"Tracker for {seed}", Public = true, Files = {{$"{seed}.txt", trackerContents}}});
            }

            if (_settings.UseSingleGist)
            {
                _gists.Clear();
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