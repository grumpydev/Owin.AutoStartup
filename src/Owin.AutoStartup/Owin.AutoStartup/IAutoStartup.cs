namespace Owin.AutoStartup
{
    using System.Collections.Generic;

    /// <summary>
    /// Defines an auto startup provider
    /// </summary>
    public interface IAutoStartup
    {
        /// <summary>
        /// Gets the name of the provider
        /// e.g. Nancy, SignalR
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the path that the provider will bind to
        /// </summary>
        string Path { get; }

        /// <summary>
        /// Gets the default builder commands that are called in configure.
        /// Used for generating help text.
        /// </summary>
        IDictionary<string, object[]> DefaultBuilderCommands { get; }

        /// <summary>
        /// Configure the auto startup
        /// </summary>
        /// <param name="builder">App builder interface</param>
        void Configuration(IAppBuilder builder);
    }
}