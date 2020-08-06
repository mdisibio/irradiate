# Irradiate
Irradiate is a library to easily add AWS X-Ray tracing to any .NET interface using [DispatchProxy](https://docs.microsoft.com/en-us/dotnet/api/system.reflection.dispatchproxy).  Subsegments are captured at the method level and record arguments, return values, exceptions, and timings. Irradiate handles async methods and ends the subsegment when the task is complete.

## Usage
Irradiate can only operate against an `interface` due to the underlying usage of DispatchProxy.  Therefore plain classes cannot be proxied unless an interface is first extracted from it. 

### Dependency Injection
```csharp
// Register both interface and implementing type
services.AddSingleton<IMyInterface, MyImplementation>();

// Decorate the service registration with a tracing proxy
services.Irradiate<IMyInterface>();
```

### Irradiate.ProxyInstance<T>
Tracing can be added to an object instance directly as well.
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
