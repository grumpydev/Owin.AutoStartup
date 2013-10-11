namespace Owin.AutoStartup.Tests
{
    using System.Collections.Generic;
    using System.Linq;

    using Xunit;

    public class AutoStartupExtensionsFixture
    {
        [Fact]
        public void Should_sort_paths_with_largest_number_of_segments_first()
        {
            var autoStartups = new IAutoStartup[] { new FakeAutoStartup("/"), new FakeAutoStartup("/foo/bar"), new FakeAutoStartup("/foo") };

            var sorted = autoStartups.SortBySegmentCount().ToArray();

            Assert.Equal("/foo/bar", sorted[0].Path);
            Assert.Equal("/foo", sorted[1].Path);
            Assert.Equal("/", sorted[2].Path);
        } 
    }

    public class FakeAutoStartup : IAutoStartup
    {
        public FakeAutoStartup(string path)
        {
            this.Path = path;
        }

        public string Name { get; private set; }

        public string Path { get; private set; }

        public IEnumerable<string> DefaultBuilderCommands { get; private set; }

        public void Configuration(IAppBuilder builder)
        {
            throw new System.NotSupportedException();
        }
    }
}