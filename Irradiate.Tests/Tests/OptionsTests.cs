using Xunit;

namespace Irradiate.Tests
{
    public class OptionsTests
    {
        [Fact]
        public void NoExludedMethods()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r);

            i.Void();
            i.VoidParams(3, 5);

            Assert.Equal(2, r.Subsegments.Count);
            
        }

        [Fact]
        public void ExcludeMethodByName()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r,
                        new Options().ExcludeMethod(nameof(IThing.Void)));

            i.Void();
            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments);
            Assert.Equal("IThing.VoidParams", r.Subsegments[0].Name);
        }

        [Fact]
        public void NoAnnotations()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r);

            i.VoidParams(3, 5);

            Assert.Empty(r.Subsegments[0].Annotations);
        }

        [Fact]
        public void AnnotateArgumentsByName()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r,
                        new Options().AnnotateArgument("x"));

            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal("3", r.Subsegments[0].Annotations["x"]);            
        }
    }
}
