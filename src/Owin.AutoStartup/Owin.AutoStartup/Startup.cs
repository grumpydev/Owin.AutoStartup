namespace Owin.AutoStartup
{
    using System;
    using System.Linq;
    using System.Reflection;

    public class Startup
    {
        private static AssemblyName assemblyName = typeof(Startup).Assembly.GetName();

        public void Configuration(IAppBuilder builder)
        {
            var autoStartupTypes = this.DiscoverAutoStartups();
            var autoStartups = this.ConstructAutoStartups(autoStartupTypes);

            this.ValidateAutoStartups(autoStartups);

            this.BuildConfiguration(autoStartups, builder);
        }

        private void BuildConfiguration(IAutoStartup[] autoStartups, IAppBuilder builder)
        {
            foreach (var autoStartup in autoStartups)
            {
                autoStartup.Configuration(builder);
            }
        }

        private void ValidateAutoStartups(IAutoStartup[] autoStartups)
        {
            foreach (var autoStartup in autoStartups)
            {
                if (autoStartups.Any(s => !ReferenceEquals(autoStartup, s) && autoStartup.Path == s.Path))
                {
                    throw new ConfictingPathException(autoStartups);
                }
            }
        }

        private IAutoStartup[] ConstructAutoStartups(Type[] autoStartupTypes)
        {
            return autoStartupTypes.Select(t => (IAutoStartup)Activator.CreateInstance(t))
                                   .ToArray();
        }

        private Type[] DiscoverAutoStartups()
        {
            var assemblies =
                AppDomain.CurrentDomain.GetAssemblies().Where(a => a.GetReferencedAssemblies().Contains(assemblyName));

            return assemblies.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAutoStartup))))
                             .ToArray();
        }
    }
}