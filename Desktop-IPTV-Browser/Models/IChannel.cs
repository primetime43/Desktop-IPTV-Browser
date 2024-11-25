namespace Desktop_IPTV_Browser.Models
{
    public interface IChannel
    {
        string DisplayName { get; }
        string IconUrl { get; }
        string Title { get; }
        string Description { get; }
    }
}
