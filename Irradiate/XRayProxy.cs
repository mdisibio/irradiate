using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Core;

namespace Irradiate
{
    using TypeAnnotater = Func<object, Tuple<object, string>>;
    using TypeFormatter = Func<object, object>;
    using ResultHandler = Func<object, object>;

    public class XrayProxy : DispatchProxy
    {
        object _instance;
        IAWSXRayRecorder _recorder;
        public Options Options { get; private set; }

        static readonly MethodInfo _handleResultTaskT = typeof(XrayProxy).GetRuntimeMethods()
                                .Where(m => m.Name == nameof(handleTaskT) && m.IsGenericMethod)
                                .First();

        readonly ConcurrentDictionary<Type, ResultHandler> _handlerCache = new ConcurrentDictionary<Type, ResultHandler>();
        readonly ConcurrentDictionary<Type, TypeAnnotater[]> _annotaterCache = new ConcurrentDictionary<Type, TypeAnnotater[]>();

        public void Init(object instance, IAWSXRayRecorder recorder, Options options)
        {
            _instance = instance;
            _recorder = recorder;
            Options = options;
        }

        protected override object Invoke(MethodInfo m, object[] args)
        {
            // If not tracing then call underlying method
            // as efficiently as possible.
            if (!shouldTrace(m))
            {
                return m.Invoke(_instance, args);
            }

            try
            {
                _recorder.BeginSubsegment($"{m.DeclaringType.Name}.{m.Name}");

                logArgs(m, args);

                // Invoke underlying method.
                var res = m.Invoke(_instance, args);

                // Post-process the result, which may
                // replace it in case of Tasks.
                var handler = _handlerCache.GetOrAdd(m.ReturnType, createResultHandler);
                res = handler(res);

                return res;
            }
            catch (TargetInvocationException ex)
            {
                logException(ex.InnerException);
                _recorder.EndSubsegment();
                throw ex.InnerException;
            }
            catch (Exception e)
            {
                logException(e);
                _recorder.EndSubsegment();
                throw;
            }
        }

        bool shouldTrace(MethodInfo m)
        {
            return _recorder.TraceContext.IsEntityPresent() &&
                    !Options.ExcludedMethodsByName.Contains(m.Name);
        }

        /// <summary>
        /// Get handler for the given return type.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        ResultHandler createResultHandler(Type returnType)
        {
            if (returnType.BaseType == typeof(Task) && returnType.IsGenericType)
            {
                // Task<T>
                // Create implementation of handleTask<T> for the
                // inner return type. Then cast it to a Func.
                return (ResultHandler)
                    Delegate.CreateDelegate(typeof(ResultHandler), this,
                    _handleResultTaskT.MakeGenericMethod(returnType.GenericTypeArguments[0]));
            }
            else if (returnType == typeof(Task))
            {
                // Task
                return handleTask;
            }
            else
            {
                // All other types
                return handleResultOther;
            }
        }

        object handleResultOther(object res)
        {
            logResult(res);
            _recorder.EndSubsegment();
            return res;
        }

        object handleTask(object o)
        {
            var task = (Task)o;
            if (task.IsCompleted)
            {
                handleCompletedTask(task);
                return task;
            }

            return task.ContinueWith(t =>
            {
                handleCompletedTask(t);
                _recorder.EndSubsegment();
            });
        }

        object handleTaskT<T>(object o)
        {
            var task = (Task<T>)o;

            if (task.IsCompleted)
            {
                handleCompletedTask(task);
                return task;
            }

            return task.ContinueWith(t =>
            {
                handleCompletedTask(t);
                return t.Result;
            });
        }

        void handleCompletedTask(Task t)
        {
            logException(t.Exception);
            _recorder.EndSubsegment();
        }

        void handleCompletedTask<T>(Task<T> t)
        {
            logException(t.Exception);
            logResult(t.Result);
            _recorder.EndSubsegment();
        }

        void logArgs(MethodInfo m, object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var val = args[i];
                var safe = convertArg(val);
                var param = m.GetParameters()[i];
                
                _recorder.AddMetadata("args." + param.Name, safe);

                if (safe != null && Options.AnnotatedArgumentsByName.Contains(param.Name))
                {
                    _recorder.AddAnnotation(param.Name, safe);
                }

                foreach (var annotater in getAnnotaters(param.ParameterType))
                {
                    var ann = annotater(val);
                    _recorder.AddAnnotation(ann.Item2, convertArg(ann.Item1));
                }
            }
        }
       
        object convertArg(object arg)
        {
            if (arg == null)
                return null;

            // Custom
            TypeFormatter custom;
            if (Options.TypeFormatters.TryGetValue(arg.GetType(), out custom))
            {
                return custom(arg);
            }

            // Natively supported by xray
            if (arg is int ||
                arg is long ||
                arg is double ||
                arg is bool)
                return arg;

            // Fallback
            return arg.ToString();
        }

        TypeAnnotater[] getAnnotaters(Type t)
        {
            return _annotaterCache.GetOrAdd(t, t =>
            {
                var f = new List<TypeAnnotater>();

                foreach (var kvp in Options.AnnotatedArgumentsByType)
                {
                    if (kvp.Key.IsAssignableFrom(t))
                    {
                        f.AddRange(kvp.Value);
                    }
                }

                return f.ToArray();
            });
        }

        void logException(Exception e)
        {
            if (e != null)
            {
                _recorder.AddException(e);
            }
        }

        void logResult(object o)
        {
            _recorder.AddMetadata("result", convertArg(o) ?? "null");
        }
    }
}
