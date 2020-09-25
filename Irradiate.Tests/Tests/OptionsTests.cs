using System;
using Xunit;

namespace Irradiate.Tests
{
    public class OptionsTests
    {
        (TestRecorder, IThing) setup(Options o = null)
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r, o);
            return (r, i);
        }

        [Fact]
        public void NoExludedMethods()
        {
            var (r, i) = setup();

            i.Void();
            i.VoidParams(3, 5);

            Assert.Equal(2, r.Subsegments.Count);
        }

        [Fact]
        public void ExcludeMethodByName()
        {
            var (r, i) = setup(new Options()
                                .ExcludeMethod(nameof(IThing.Void)));

            i.Void();
            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments);
            Assert.Equal("IThing.VoidParams", r.Subsegments[0].Name);
        }

        [Fact]
        public void NoAnnotations()
        {
            var (r, i) = setup();

            i.VoidParams(3, 5);

            Assert.Empty(r.Subsegments[0].Annotations);
        }

        [Fact]
        public void AnnotateArgumentsByName()
        {
            var (r, i) = setup(new Options().Annotate("x"));

            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal(3, r.Subsegments[0].Annotations["x"]);            
        }

        [Fact]
        public void AnnotateArgumentsByNameWithNameOverride()
        {
            var (r, i) = setup(new Options().Annotate("x", "customX"));

            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal(3, r.Subsegments[0].Annotations["customX"]);
        }


        [Fact]
        public void AnnotateArgumentsByType()
        {
            var (r, i) = setup(new Options()
                            .Annotate<string>(s => Tuple.Create((object)s.ToUpper(), "name")));

            i.VoidParamsNullable("string");

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal("STRING", r.Subsegments[0].Annotations["name"]);
        }

        [Fact]
        public void AnnotateArgumentsByTypeMultiple()
        {
            var (r, i) = setup(new Options()
                            .Annotate<string>(s => Tuple.Create((object)(s + "1"), "1"))
                            .Annotate<string>(s => Tuple.Create((object)(s + "2"), "2"))
                            );

            i.VoidParamsNullable("s");

            Assert.Equal(2, r.Subsegments[0].Annotations.Keys.Count);
            Assert.Equal("s1", r.Subsegments[0].Annotations["1"]);
            Assert.Equal("s2", r.Subsegments[0].Annotations["2"]);
        }

        [Fact]
        public void AnnotateArgumentsByTypeInheritance()
        {
            var (r, i) = setup(new Options()
                            .Annotate<A>(a => Tuple.Create((object)a.a, "a"))
                            );

            i.B(new B { a = 5 });

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal(5, r.Subsegments[0].Annotations["a"]);
        }

        [Fact]
        public void CustomTypeFormatter()
        {
           var (r, i) = setup(new Options()
                            .Annotate("x")
                            .AddTypeFormatter<int>(i => "int" + i.ToString()));

            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal("int3", r.Subsegments[0].Annotations["x"]);
            Assert.Equal("int3", r.Subsegments[0].MetaData["args.x"]);
        }

        [Fact]
        public void CustomTypeFormatter_Exception()
        {
            var (r, i) = setup(new Options()
                            .Annotate("x")
                            .AddTypeFormatter<int>(i => throw new NotImplementedException()));

            Assert.Throws<NotImplementedException>(() => i.VoidParams(3, 5));

            Assert.Single(r.Subsegments);
            Assert.Empty(r.Subsegments[0].MetaData.Keys);
        }

        [Fact]
        public void AnnotateIgnoresNull()
        {
            var (r, i) = setup(new Options().Annotate("s"));

            i.VoidParamsNullable(null);

            Assert.Empty(r.Subsegments[0].Annotations.Keys);
            Assert.Null(r.Subsegments[0].MetaData["args.s"]);
        }
    }
}
