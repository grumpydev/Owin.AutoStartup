namespace Owin.AutoStartup.Demo
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;
    using System.Threading.Tasks;

    public class HelloWorldAutoStartup : IAutoStartup
    {
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

        public IDictionary<string, object[]> DefaultBuilderCommands { get; private set; }

        public void Configuration(IAppBuilder builder)
        {
            builder.UseHandlerAsync((req, res) =>
            {
                if (req.Path == "/")
                {
                    res.ContentType = "text/plain";
                    return res.WriteAsync("Hello, World!");
                }

                var tcs = new TaskCompletionSource<object>();
                tcs.SetResult(new Object());
                return tcs.Task;
            });
        }
    }
}