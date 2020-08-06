using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Irradiate.Tests
{
    public class ServicesCollectionTests
    {
        [Fact]
        public void Test_ImplmentationType()
        {
            var r = new TestRecorder();

            var services = new ServiceCollection()
                .AddSingleton<IThing, Thing>()
                .AddSingleton<IAWSXRayRecorder>(r)
                .Irradiate<IThing>()
                .BuildServiceProvider();

            services.GetRequiredService<IThing>().FuncParams(5, 7);

            Assert.Single(r.Subsegments);
            Assert.Equal("5", r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal("7", r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal("35", r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public void Test_ImplmentationType_RegisteredAlready()
        {
            var r = new TestRecorder();

            var services = new ServiceCollection()
                .AddSingleton<Thing>() // <-- Implementation type is registered already
                .AddSingleton<IThing, Thing>()
                .AddSingleton<IAWSXRayRecorder>(r)
                .Irradiate<IThing>()
                .BuildServiceProvider();

            services.GetRequiredService<IThing>().FuncParams(5, 7);

            Assert.Single(r.Subsegments);
            Assert.Equal("5", r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal("7", r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal("35", r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public void Test_ImplmentationFactory()
        {
            var r = new TestRecorder();

            var services = new ServiceCollection()
                .AddSingleton<IThing>(p => new Thing())
                .AddSingleton<IAWSXRayRecorder>(r)
                .Irradiate<IThing>()
                .BuildServiceProvider();

            services.GetRequiredService<IThing>().FuncParams(5, 7);

            Assert.Single(r.Subsegments);
            Assert.Equal("5", r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal("7", r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal("35", r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public void Test_ImplmentationInstance()
        {
            var r = new TestRecorder();

            var services = new ServiceCollection()
                .AddSingleton<IThing>(new Thing())
                .AddSingleton<IAWSXRayRecorder>(r)
                .Irradiate<IThing>()
                .BuildServiceProvider();

            services.GetRequiredService<IThing>().FuncParams(5, 7);

            Assert.Single(r.Subsegments);
            Assert.Equal("5", r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal("7", r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal("35", r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public void Test_Options()
        {
            var services = new ServiceCollection()
                .AddSingleton<IThing>(new Thing())
                .Irradiate<IThing>(c => c.ExcludeMethod(nameof(IThing.Void)))
                .BuildServiceProvider();

            var proxy = (XrayProxy)services.GetRequiredService<IThing>();

            Assert.Equal("Void", proxy.Options.ExcludedMethodsByName[0]);
        }
    }
}
