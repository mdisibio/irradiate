# Irradiate
![CI](https://github.com/mdisibio/irradiate/workflows/.NET%20Core/badge.svg)
[![Coverage Status](https://coveralls.io/repos/github/mdisibio/irradiate/badge.svg?branch=master)](https://coveralls.io/github/mdisibio/irradiate?branch=master)

Irradiate is a library to easily add AWS X-Ray tracing to any .NET interface using [DispatchProxy](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy).  Subsegments are captured at the method level and record arguments, return values, exceptions, and timings. Irradiate handles async methods and ends the subsegment when the task is complete.

## Usage
Irradiate can only operate against an `interface` due to the underlying usage of DispatchProxy.  Therefore plain classes cannot be proxied unless an interface is first extracted from it. 

Irradiate only records [subsegments](https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html#xray-concepts-subsegments) (not [segments](https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html#xray-concepts-segments) ) and makes no sampling decisions on its own. Your application must begin segments through another method such as the [AWS X-Ray middleware](https://github.com/aws/aws-xray-sdk-dotnet/tree/master#aspnet-core-framework-net-core--nuget) i.e. `app.UseXray()`.

### Dependency Injection
Typical usage is to trace an interface that has been previously registered in the services collection. The `Irradiate` method replaces the existing service descriptor with a proxy.
```csharp
services.AddSingleton<IMyInterface, MyImplementation>();
services.Irradiate<IMyInterface>();
```

### Irradiate.ProxyInstance<T>
Tracing can be directly added to an object instance as well.
```csharp
var instance = new MyImplementation();

// The desired interface may not be able to be inferred and
// can be specified in a few ways.
IMyInterface proxy = Irradiate.ProxyInstance<IMyInterface>(instance);
    or
IMyInterface proxy = Irradiate.ProxyInstance((IMyInterface)instance);
```

## Configuration

### IAWSXRayRecorder
The recorder instance can optionally be specified. If not specified then the default AWSXrayRecorder.Instance is used.

```csharp
// Register the recorder when using DependencyInjection
services.AddSingleton<IAWSXrayRecorder>(instance);
services.Irradiate<...>();

// Pass argument otherwise
Irradiate.ProxyInstance<...>(..., recorder: r);
```

### Exclude Methods
Methods can be excluded from all tracing via Options.ExludeMethod(). By default all methods are traced.
```csharp
options.ExcludeMethod(nameof(IInterface.Method));
```

### Annotations
By default method arguments are only recorded as metadata.  Arguments can also be recorded as [Annotations](https://docs.aws.amazon.com/xray/latest/devguide/xray-concepts.html#xray-concepts-annotations) via Options.AnnotateArgument(). 
```
options.AnnotateArgument("x");
```

## Performance
Because DispatchProxy is based on reflection and Method.Invoke, there is overhead induced for every method call. This library is intended to trace asynchronous or i/o-bound workloads and may not be suitable for performance-critical areas.  General overhead is 1-2 microseconds per method call. 

Benchmark | Mean | Error
------------ | ------------- | --------
Baseline (direct method call) | 0 ns | 0 ns
Irradiate Proxy (0% tracing) | 785 ns | 11 ns
Irradiate Proxy (100% tracing) | 1126 ns | 17 ns

Benchmarks can be run locally with:
```
docker-compose run benchmark
```
