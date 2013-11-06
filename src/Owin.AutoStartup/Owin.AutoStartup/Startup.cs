namespace Owin.AutoStartup
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    public class Startup
    {
        private static string assemblyName = typeof(Startup).Assembly.GetName().Name;

        public void Configuration(IAppBuilder builder)
        {
            LoadAssembliesWithAutoStartupReferences();

            var autoStartupTypes = this.DiscoverAutoStartups();
            var autoStartups = this.ConstructAutoStartups(autoStartupTypes);

            this.ValidateAutoStartups(autoStartups);

            this.BuildConfiguration(autoStartups, builder);
        }

        private void BuildConfiguration(IAutoStartup[] autoStartups, IAppBuilder builder)
        {
            if (this.AddDiagsPage())
            {
                builder.UseDiags(autoStartups);
            }

            foreach (var autoStartup in autoStartups.SortBySegmentCount())
            {
                autoStartup.Configuration(builder);
            }
        }

        private bool AddDiagsPage()
        {
            var appSetting = System.Configuration.ConfigurationManager.AppSettings["owin:AutoStartupDiagnostics"];

            if (appSetting == null)
            {
                return true;
            }

            bool enable;
            if (!bool.TryParse(appSetting, out enable))
            {
                return true;
            }

            return enable;
        }

        private void ValidateAutoStartups(IAutoStartup[] autoStartups)
        {
            foreach (var autoStartup in autoStartups)
            {
                if (autoStartups.Any(s => !ReferenceEquals(autoStartup, s) && autoStartup.Path == s.Path))
                {
                    throw new ConflictingPathException(autoStartups);
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
                AppDomain.CurrentDomain.GetAssemblies()
                                       .Where(a => a.GetReferencedAssemblies()
                                       .Any(an => an.Name == assemblyName));

            return assemblies.SelectMany(a => a.GetTypes().Where(t => t.GetInterfaces().Contains(typeof(IAutoStartup))))
                             .ToArray();
        }

        private static void LoadAssembliesWithAutoStartupReferences()
        {
            var existingAssemblyPaths =
                AppDomain.CurrentDomain.GetAssemblies().Select(a => a.Location).ToArray();

            foreach (var directory in GetAssemblyDirectories())
            {
                var unloadedAssemblies = Directory
                    .GetFiles(directory, "*.dll")
                    .Where(f => !existingAssemblyPaths.Contains(f, StringComparer.InvariantCultureIgnoreCase)).ToArray();

                foreach (var unloadedAssembly in unloadedAssemblies)
                {
                    Assembly inspectedAssembly = null;
                    try
                    {
                        inspectedAssembly = Assembly.ReflectionOnlyLoadFrom(unloadedAssembly);
                    }
                    catch (BadImageFormatException biEx)
                    {
                        //the assembly maybe it's not managed code
                    }

                    if (inspectedAssembly != null && inspectedAssembly.GetReferencedAssemblies().Any(r => r.Name.StartsWith("Nancy", StringComparison.OrdinalIgnoreCase)))
                    {
                        try
                        {
                            Assembly.Load(inspectedAssembly.GetName());
                        }
                        catch
                        {
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns the directories containing dll files. It uses the default convention as stated by microsoft.
        /// </summary>
        /// <see cref="http://msdn.microsoft.com/en-us/library/system.appdomainsetup.privatebinpathprobe.aspx"/>
        private static IEnumerable<string> GetAssemblyDirectories()
        {
            var privateBinPathDirectories = AppDomain.CurrentDomain.SetupInformation.PrivateBinPath == null
                                                ? new string[] { }
                                                : AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(';');

            foreach (var privateBinPathDirectory in privateBinPathDirectories)
            {
                if (!string.IsNullOrWhiteSpace(privateBinPathDirectory))
                {
                    yield return privateBinPathDirectory;
                }
            }

            if (AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe == null)
            {
                yield return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
        }
    }
}