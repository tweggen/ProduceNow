namespace ProduceNow.Models;

public class ChannelPresentation
{
    public string ShortTitle { get; set; }
    public bool IsRecording { get; set; }
    public string StateString { get; set; }
    public string Uri { get; set; } = "avares://ProducePro/Assets/StudioA.png";
}