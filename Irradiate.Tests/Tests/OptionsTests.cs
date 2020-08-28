﻿using System;
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
            Assert.Equal(3, r.Subsegments[0].Annotations["x"]);            
        }

        [Fact]
        public void CustomTypeFormatter()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r,
                        new Options()
                            .AnnotateArgument("x")
                            .AddTypeFormatter<int>(i => "int" + i.ToString()));

            i.VoidParams(3, 5);

            Assert.Single(r.Subsegments[0].Annotations.Keys);
            Assert.Equal("int3", r.Subsegments[0].Annotations["x"]);
            Assert.Equal("int3", r.Subsegments[0].MetaData["args.x"]);
        }

        [Fact]
        public void CustomTypeFormatter_Exception()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r,
                        new Options()
                            .AnnotateArgument("x")
                            .AddTypeFormatter<int>(i => throw new NotImplementedException()));

            Assert.Throws<NotImplementedException>(() => i.VoidParams(3, 5));

            Assert.Single(r.Subsegments);
            Assert.Empty(r.Subsegments[0].MetaData.Keys);
        }

        [Fact]
        public void AnnotateIgnoresNull()
        {
            var r = new TestRecorder();
            var i = Irradiate.ProxyInstance<IThing>(new Thing(), r,
                        new Options().AnnotateArgument("s"));

            i.VoidParamsNullable(null);

            Assert.Empty(r.Subsegments[0].Annotations.Keys);
            Assert.Null(r.Subsegments[0].MetaData["args.s"]);
        }
    }
}
