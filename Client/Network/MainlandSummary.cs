using Client.Stream;

namespace Client.Network;

public class MainlandSummary
{
    /// <summary>
    /// Mainland Id
    /// </summary>
    public int Id = 0;

    /// <summary>
    /// Description
    /// </summary>
    public string Name = "en";

    /// <summary>
    /// Description
    /// </summary>
    public string Description = "";

    /// <summary>
    /// Language code
    /// </summary>
    public string LanguageCode = "en";

    /// <summary>
    /// True if mainland is up
    /// </summary>
    public bool Online = false;

    /// <summary>
    /// Serialization coming from a stream (net message)
    /// </summary>
    public void Serial(BitMemoryStream f)
    {
        f.Serial(ref Id);
        f.Serial(ref Name);
        f.Serial(ref Description);
        f.Serial(ref LanguageCode, false);
        f.Serial(ref Online);
    }
}
