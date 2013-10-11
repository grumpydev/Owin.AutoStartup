namespace Owin.AutoStartup.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public class HelloWorldAutoStartup : IAutoStartup
    {
        private const string HelloWorldBody = "<html><head></head><body><h1>Hello, World!</h1>"
                                            + "<p>Try some of these links:</p>"
                                            + "<ul>"
                                            + "<li><a href='__asdiags'>Owin.AutoStartup Diagnostics Page.</a></li>"
                                            + "<li><a href='ct'>Just changing a header test.</a></li>"
                                            + "<li><a href='fewfew'>No response created test.</a></li>"
                                            + "</ul>"
                                            + "</body></html>";

        private readonly Task<object> completedTask;

        private readonly IEnumerable<string> defaultBuilderCommands = new[]
                                                {
                                                    "// No default builder for the hello world sample :-)",
                                                    "// Real autostartups would have sample code here",
                                                    "// To show how to 'convert' to a normal OWIN startup class"
                                                };

        public string Name
        {
            get
            {
                return "HelloWorld";
            }
        }

        public string Path
        {
            get
            {
                return "/";
            }
        }

        public HelloWorldAutoStartup()
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
                if (req.Path == "/")
                {
                    res.ContentType = "text/html";
                    return res.WriteAsync(HelloWorldBody);
                }

                return this.completedTask;
            });
        }
    }
}