namespace Owin.AutoStartup
{
    using System.Collections.Generic;

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
    }
}