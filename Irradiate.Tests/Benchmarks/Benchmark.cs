using BenchmarkDotNet.Attributes;

namespace Irradiate.Tests
{
    [SimpleJob(launchCount: 1, warmupCount: 2, targetCount: 5)]
    public class Benchmark
    {
        readonly Thing thing;
        readonly IThing proxyNotTraced;
        readonly IThing proxyTraced;

        public Benchmark()
        {
            thing = new Thing();
            proxyNotTraced = Irradiate.ProxyInstance<IThing>(thing, new BenchmarkRecorder(false));
            proxyTraced = Irradiate.ProxyInstance<IThing>(thing, new BenchmarkRecorder(true));
        }

        [Benchmark(Baseline=true, Description="Baseline - Raw function call")]
        public int Baseline() => thing.FuncParams(3, 5);

        [Benchmark(Description="Proxy baseline (0% tracing)")]
        public int ProxyNoTracing() => proxyNotTraced.FuncParams(3, 5);

        [Benchmark(Description="Proxy typical (100% tracing)")]
        public int ProxyTraced() => proxyTraced.FuncParams(3, 5);
    }
}
