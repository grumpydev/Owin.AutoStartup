namespace Owin.AutoStartup
{
    using System.Collections.Generic;
    using System.Linq;

    public static class AutoStartupExtentions
    {
        public static string Pretty(this IAutoStartup startup)
        {
            return string.Format("Name: {0}\nPath {1}\n", startup.Name, startup.Path);
        }

        public static void UseDiags(this IAppBuilder builder, IAutoStartup[] autoStartups)
        {
            builder.Use(typeof(Diags.Diags), (IEnumerable<IAutoStartup>)autoStartups);
        }

        public static IEnumerable<IAutoStartup> SortBySegmentCount(this IAutoStartup[] startups)
        {
            return startups.OrderByDescending(s => s.Path, new SegmentCountComparer());
        }
    }
}