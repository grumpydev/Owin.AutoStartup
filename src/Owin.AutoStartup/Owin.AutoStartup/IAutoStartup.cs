﻿namespace Owin.AutoStartup
{
    using System;
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
        /// Gets the NuGet packages that a user would need to install in order
        /// to convert their application from AutoStartup to a normal OWIN application.
        /// e.g. Nancy.Owin
        /// </summary>
        IEnumerable<String> NonAutoStartupNugets { get; }

        /// <summary>
        /// Gets the default builder commands that are called in configure.
        /// Used for generating help text.
        /// </summary>
        IEnumerable<String> DefaultBuilderCommands { get; }

        /// <summary>
        /// Configure the auto startup
        /// </summary>
        /// <param name="builder">App builder interface</param>
        void Configuration(IAppBuilder builder);
    }
}