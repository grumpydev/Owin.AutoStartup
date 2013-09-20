namespace Owin.AutoStartup.SSVE
{
    public class ViewEngineHost : IViewEngineHost
    {
        public object Context { get; private set; }

        public string HtmlEncode(string input)
        {
            return System.Net.WebUtility.HtmlEncode(input);
        }

        public string GetTemplate(string templateName, object model)
        {
            throw new System.NotSupportedException();
        }

        public string GetUriString(string name, params string[] parameters)
        {
            throw new System.NotSupportedException();
        }

        public string ExpandPath(string path)
        {
            throw new System.NotSupportedException();
        }

        public string AntiForgeryToken()
        {
            throw new System.NotSupportedException();
        }
    }
}