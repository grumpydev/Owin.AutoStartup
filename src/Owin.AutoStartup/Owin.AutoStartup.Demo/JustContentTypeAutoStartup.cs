namespace Owin.AutoStartup.Demo
{
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

        public IEnumerable<string> DefaultBuilderCommands
        {
            get
            {
                return this.defaultBuilderCommands;
            }
        }

        public void Configuration(IAppBuilder builder)
        {
            builder.UseHandlerAsync((req, res) =>
            {
                if (req.Path == "/ct")
                {
                    res.ContentType = "text/plain";
                    return this.completedTask;
                }

                return this.completedTask;
            });
        }
         
    }
}