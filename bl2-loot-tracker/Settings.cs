namespace bl2_loot_tracker;

public class Settings
{
    public string Token { get; set; }
    public string SeedsPath { get; set; }
    public List<string>? AdditionalPaths { get; set; }
    public bool UseSingleGist { get; set; }
}