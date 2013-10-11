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

        private const string OwinRequestPath = "owin.RequestPath";

        private const string OwinResponseHeaders = "owin.ResponseHeaders";

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
            if (IsDiagsRequest(environment))
            {
                return this.RenderDiags(environment, true);
            }

            var initialResponseHeaders = GetInitialResponseHeaders(environment);
            WrapResponseStream(environment);

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
                            if (!initialResponseHeaders.Any(kvp => kvp.Key.Equals(outputResponseHeader.Key, StringComparison.OrdinalIgnoreCase)))
                            {
                                return t;
                            }

                            var initialHeaderValues = initialResponseHeaders.First(kvp => kvp.Key.Equals(outputResponseHeader.Key, StringComparison.OrdinalIgnoreCase))
                                                                            .Value;

                            if (!initialHeaderValues.SequenceEqual(outputResponseHeader.Value))
                            {
                                return t;
                            }
                        }

                        return this.RenderDiags(environment, false);
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

        private static void WrapResponseStream(IDictionary<string, object> environment)
        {
            var responseStream = (Stream)environment[OwinResponseBody];

            var wrapped = new WrappedStream(responseStream);

            environment[OwinResponseBody] = wrapped;
        }

        private static List<KeyValuePair<string, string[]>> GetInitialResponseHeaders(IDictionary<string, object> environment)
        {
            var responseHeaders = (IDictionary<string, string[]>)environment[OwinResponseHeaders];
            
            // To work around issue with katana where it adds headers to the collection after it 
            // has been iterated over for the first time.
            var responseHeaderKeys = responseHeaders.Keys.ToList();
            
            var responseHeaderKvps = new List<KeyValuePair<string, string[]>>(responseHeaders.Count);
            responseHeaderKvps.AddRange(responseHeaders.Select(kvp => kvp));
            return responseHeaderKvps;
        }

        private static bool IsDiagsRequest(IDictionary<string, object> environment)
        {
            var path = (string)environment[OwinRequestPath];

            return DiagsPath.Equals(path, StringComparison.OrdinalIgnoreCase);
        }

        private Task RenderDiags(IDictionary<string, object> environment, bool isDiagsRequest)
        {
            var template = this.LoadResource("Owin.AutoStartup.Diags.DiagsView.html");
            var path = (string)environment[OwinRequestPath];

            var output = this.ssve.Render(
                                template, 
                                new 
                                {
                                    AutoStartups = this.autoStartups,
                                    Path = path,
                                    IsDiagsRequest = isDiagsRequest
                                }, 
                                this.ssveHost);

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