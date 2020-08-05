using System;
using BenchmarkDotNet.Attributes;

namespace Irradiate.Tests
{
    [SimpleJob(launchCount: 1, warmupCount: 5, targetCount: 10)]
    public class Benchmark
    {
        readonly Thing thing;
        readonly IThing proxyNotTraced;
        readonly IThing proxyTraced;

        public Benchmark()
        {
            thing = new Thing();
            proxyNotTraced = Irradiate.ProxyInstance<IThing>(thing);
            proxyTraced = Irradiate.ProxyInstance<IThing>(thing, new TestRecorder());
        }

        [Benchmark]
        public int Baseline() => thing.FuncParams(3, 5);

        [Benchmark]
        public int ProxyNoTracing() => proxyNotTraced.FuncParams(3, 5);

        [Benchmark]
        public int ProxyTraced() => proxyTraced.FuncParams(3, 5);
    }
}
