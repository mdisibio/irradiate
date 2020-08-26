using System;
using System.Threading.Tasks;
using Xunit;

namespace Irradiate.Tests
{
    public class TraceMethodTests
    {
        (TestRecorder, IThing) setup()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r);
            return (r, i);
        }

        [Fact]
        public void Test_Void()
        {
            var (r, i) = setup();

            i.Void();

            Assert.Single(r.Subsegments);
            Assert.Equal("null", r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public async Task Test_VoidAsync()
        {
            var (r, i) = setup();

            await i.VoidAsync();

            Assert.Single(r.Subsegments);
            Assert.Empty(r.Subsegments[0].MetaData.Keys);
        }

        [Fact]
        public async Task Test_VoidAsyncDelayed()
        {
            var (r, i) = setup();

            await i.VoidAsyncDelayed();

            Assert.Single(r.Subsegments);
            Assert.Empty(r.Subsegments[0].MetaData.Keys);
        }

        [Fact]
        public void Test_FuncParams()
        {
            var (r, i) = setup();

            i.FuncParams(3, 5);

            Assert.Single(r.Subsegments);
            Assert.Equal(3, r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal(5, r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal(15, r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public async Task Test_FuncParamsAsync()
        {
            var (r, i) = setup();

            await i.FuncParamsAsync(3, 5);

            Assert.Single(r.Subsegments);
            Assert.Equal(3, r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal(5, r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal(15, r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public async Task Test_FuncParamsAsyncDelayed()
        {
            var (r, i) = setup();

            await i.FuncParamsAsyncDelayed(3, 5);

            Assert.Single(r.Subsegments);
            Assert.Equal(3, r.Subsegments[0].MetaData["args.x"]);
            Assert.Equal(5, r.Subsegments[0].MetaData["args.y"]);
            Assert.Equal(15, r.Subsegments[0].MetaData["result"]);
        }

        [Fact]
        public void Test_ThrowsException()
        {
            var (r, i) = setup();

            Assert.Throws<NotImplementedException>(() => i.ThrowsException());

            Assert.Single(r.Subsegments);
            Assert.Single(r.Subsegments[0].Exceptions);
        }
    }
}
