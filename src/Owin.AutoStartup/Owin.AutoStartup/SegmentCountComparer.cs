namespace Owin.AutoStartup
{
    using System;
    using System.Collections.Generic;

    public class SegmentCountComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            return x.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length
                   - y.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }
    }
}