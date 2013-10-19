namespace Owin.AutoStartup.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public class JustContentTypeAutoStartup : IAutoStartup
    {
        private readonly Task<object> completedTask;

        private readonly IEnumerable<string> defaultBuilderCommands = new[]
                                                {
                                                    "// Sample of what a real startup might specify",
                                                    "// If it included a helper Builder extension method",
                                                    "builder.UseCt();"
                                                };

        private readonly IEnumerable<string> nonAutoStartupNugets = new[] { "Just.ContentType.Owin" };

        public string Name
        {
            get
            {
                return "JustContentType";
            }
        }

        public string Path
        {
            get
            {
                return "/ct";
            }
        }

        public JustContentTypeAutoStartup()
        {
            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(new object());
            this.completedTask = tcs.Task;
        }

        public IEnumerable<string> NonAutoStartupNugets
        {
            get
            {
                return this.nonAutoStartupNugets;
            }
        }

        public IEnumerable<string> DefaultBuilderCommands
        {
            get
            {
                return this.defaultBuilderCommands;
            }
        }

        public void Configuration(IAppBuilder builder)
        {
            builder.UseFunc(this.Middleware);
        }

        private Func<IDictionary<string, object>, Task> Middleware(Func<IDictionary<string, object>, Task> next)
        {
            return env =>
                {
                    var path = (string)env["owin.RequestPath"];

                    if (path == "/ct")
                    {
                        var outputResponseHeaders = (IDictionary<string, string[]>)env["owin.ResponseHeaders"];
                        outputResponseHeaders["Content-Type"] = new[] { "text/plain" };
                        return this.completedTask;
                    }

                    return next(env);
                };
        }
    }
}