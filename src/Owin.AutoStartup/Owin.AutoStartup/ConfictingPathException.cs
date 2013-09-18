namespace Owin.AutoStartup
{
    using System;
    using System.Linq;

    internal class ConfictingPathException : Exception
    {
        private const string MessageTemplate = "AutoStartups with conflicting paths were found. Discovered AutoStartups:\n\n{0}";

        private readonly IAutoStartup[] autoStartups;

        public override string Message
        {
            get
            {
                return string.Format(MessageTemplate, this.autoStartups.Select(s => s.Pretty()).Aggregate((s1, s2) => s1 + "\n" + s2));
            }
        }

        public ConfictingPathException(IAutoStartup[] autoStartups)
        {
            this.autoStartups = autoStartups;
        }
    }
}