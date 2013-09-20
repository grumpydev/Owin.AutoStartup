namespace Owin.AutoStartup.Diags
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Owin.AutoStartup.SSVE;

    public class Diags
    {
        private const string DiagsPath = "/__asdiags";

        private readonly Task completedTask;

        private readonly IAutoStartup[] autoStartups;

        private readonly Func<IDictionary<string, object>, Task> next;

        private readonly SuperSimpleViewEngine ssve;

        private readonly ViewEngineHost ssveHost;

        public Diags(Func<IDictionary<string, object>, Task> next, IEnumerable<IAutoStartup> autoStartups)
        {
            this.autoStartups = autoStartups.ToArray();
            this.next = next;

            this.ssve = new SuperSimpleViewEngine();
            this.ssveHost = new ViewEngineHost();

            var tcs = new TaskCompletionSource<object>();
            tcs.SetResult(new object());
            this.completedTask = tcs.Task;
        }

        public Task Invoke(IDictionary<string, object> environment)
        {
            if (this.IsDiagsRequest(environment))
            {
                return this.RenderDiags(environment);
            }
            System.Diagnostics.Trace.WriteLine(string.Format("Hitting TestLogger, path: {0}", environment["owin.RequestPath"]));

            return this.RunNext(environment);
        }

        private bool IsDiagsRequest(IDictionary<string, object> environment)
        {
            var path = (string)environment["owin.RequestPath"];

            return DiagsPath.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        private Task RenderDiags(IDictionary<string, object> environment)
        {
            var template = this.LoadResource("Owin.AutoStartup.Diags.DiagsView.html");

            var output = this.ssve.Render(template, this.autoStartups, this.ssveHost);

            this.WriteResponse(environment, output, "text/html; charset=utf-8");

            return this.completedTask;
        }

        private void WriteResponse(IDictionary<string, object> environment, string body, string contentType)
        {
            var responseBytes = Encoding.UTF8.GetBytes(body);

            var responseStream = (Stream)environment["owin.ResponseBody"];
            var responseHeaders = (IDictionary<string, string[]>)environment["owin.ResponseHeaders"];

            responseHeaders["Content-Length"] = new[] { responseBytes.Length.ToString(CultureInfo.InvariantCulture) };
            responseHeaders["Content-Type"] = new[] { contentType };

            responseStream.Write(responseBytes, 0, responseBytes.Length);
        }

        private Task RunNext(IDictionary<string, object> environment)
        {
            if (this.next != null)
            {
                return this.next(environment);
            }

            return this.completedTask;
        }

        private string LoadResource(string filename)
        {
            var resourceStream = typeof(Diags).Assembly.GetManifestResourceStream(filename);

            if (resourceStream == null)
            {
                return string.Empty;
            }

            using (var reader = new StreamReader(resourceStream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}