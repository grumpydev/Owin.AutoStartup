namespace Owin.AutoStartup
{
    public static class AutoStartupExtentions
    {
        public static string Pretty(this IAutoStartup startup)
        {
            return string.Format("Name: {0}\nPath {1}\n", startup.Name, startup.Path);
        }
    }
}