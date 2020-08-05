using System;
using System.Reflection;
using Amazon.XRay.Recorder.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Irradiate
{
    public static class Irradiate
    {
        /// <summary>
        /// Create a tracing proxy around the given instance that implements TInterface.
        /// </summary>
        /// <typeparam name="TInterface"></typeparam>
        /// <param name="instance"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        public static TInterface ProxyInstance<TInterface>(
            TInterface instance,
            IAWSXRayRecorder r = null,
            Options options = null)
        {
            var p = DispatchProxy.Create<TInterface, XrayProxy>();
            (p as XrayProxy).Init(instance, r ?? AWSXRayRecorder.Instance, options ?? new Options());
            return p;
        }
    }

    public static class ServicesCollectionExtensions
    {
        /// <summary>
        /// Find the service registration for type T and decorate it with Xray tracing.
        /// Type T must be an interface.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection Irradiate<T>(this IServiceCollection services, Action<Options> config = null)
        {
            for(int i=0; i < services.Count; i++)
            {
                var reg = services[i];

                if (reg.ServiceType == typeof(T))
                {
                    Func<IServiceProvider, object> factory;

                    var options = new Options();
                    config?.Invoke(options);

                    if (reg.ImplementationFactory != null)
                    {
                        factory = p => global::Irradiate.Irradiate.ProxyInstance(
                                (T)reg.ImplementationFactory(p),
                                p.GetService<IAWSXRayRecorder>(),
                                options);
                        
                    }
                    else if (reg.ImplementationInstance != null)
                    {
                        factory = p => global::Irradiate.Irradiate.ProxyInstance(
                                (T)reg.ImplementationInstance,
                                p.GetService<IAWSXRayRecorder>(),
                                options);
                    }
                    else if (reg.ImplementationType != null)
                    {
                        // Ensure underlying type is registered or else
                        // GetRequiredService will not know how to construct it.
                        services.Add(new ServiceDescriptor(reg.ImplementationType, reg.ImplementationType, reg.Lifetime));

                        factory = p => global::Irradiate.Irradiate.ProxyInstance<T>(
                                (T)p.GetRequiredService(reg.ImplementationType),
                                p.GetService<IAWSXRayRecorder>(),
                                options);
                    }
                    else
                    {
                        throw new InvalidOperationException($"Registration for type {typeof(T).Name} is of an unhandled type. I don't know what to do with it.");
                    }

                    services[i] = new ServiceDescriptor(typeof(T), factory, reg.Lifetime);

                    return services;
                }
            }

            throw new InvalidOperationException($"Registration for type {typeof(T).Name} was not found in the service collection");
        }
    }
}
