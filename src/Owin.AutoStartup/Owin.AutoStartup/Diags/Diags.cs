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

        private const string OwinResponseBody = "owin.ResponseBody";

        private readonly Task completedTask;

        private readonly IAutoStartup[] autoStartups;

        private readonly Func<IDictionary<string, object>, Task> next;

        private readonly SuperSimpleViewEngine ssve;

        private readonly ViewEngineHost ssveHost;

        private static string OwinResponseHeaders = "owin.ResponseHeaders";

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
            if (IsDiagsRequest(environment))
            {
                return this.RenderDiags(environment);
            }

            var responseHeaderKeys = GetInitialResponseHeaders(environment);
            var responseStream = WrapResponseStream(environment);

            return this.RunNext(environment).ContinueWith(
                t =>
                    {
                        if (CheckAndUnwrapResponseStream(environment))
                        {
                            return t;
                        }

                        var outputResponseHeaders = (IDictionary<string, string[]>)environment[OwinResponseHeaders];
                        foreach (var outputResponseHeader in outputResponseHeaders)
                        {
                            if (!responseHeaderKeys.Any(k => k.Equals(outputResponseHeader.Key)))
                            {
                                return t;
                            }
                        }

                        return this.RenderDiags(environment);
                    },
                    TaskContinuationOptions.NotOnCanceled | TaskContinuationOptions.NotOnFaulted);
        }

        private static bool CheckAndUnwrapResponseStream(IDictionary<string, object> environment)
        {
            var responseStream = environment[OwinResponseBody] as WrappedStream;

            if (responseStream == null)
            {
                return false;
            }

            environment[OwinResponseBody] = responseStream.UnWrap();

            return responseStream.HasBeenWrittenTo;
        }

        private static WrappedStream WrapResponseStream(IDictionary<string, object> environment)
        {
            var responseStream = (Stream)environment[OwinResponseBody];

            var wrapped = new WrappedStream(responseStream);

            environment[OwinResponseBody] = wrapped;

            return wrapped;
        }

        private static List<string> GetInitialResponseHeaders(IDictionary<string, object> environment)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment[OwinResponseHeaders];
            var responseHeaderKeys = responseHeaders.Keys.ToList();
            responseHeaderKeys.Clear();
            responseHeaderKeys.AddRange(responseHeaders.Select(responseHeader => responseHeader.Key));
            return responseHeaderKeys;
        }

        private static bool IsDiagsRequest(IDictionary<string, object> environment)
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

            var responseStream = (Stream)environment[OwinResponseBody];
            var responseHeaders = (IDictionary<string, string[]>)environment[OwinResponseHeaders];

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