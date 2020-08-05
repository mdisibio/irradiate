using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.XRay.Recorder.Core;

namespace Irradiate
{
    public class XrayProxy : DispatchProxy
    {
        object _instance;
        IAWSXRayRecorder _recorder;
        Options _options;

        static readonly MethodInfo _handleResultTaskT = typeof(XrayProxy).GetRuntimeMethods()
                                .Where(m => m.Name == nameof(handleTaskT) && m.IsGenericMethod)
                                .First();

        readonly ConcurrentDictionary<Type, Func<object, object>> _handlerCache = new ConcurrentDictionary<Type, Func<object, object>>();

        public void Init(object instance, IAWSXRayRecorder recorder, Options options)
        {
            _instance = instance;
            _recorder = recorder;
            _options = options;
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
                    !_options.ExcludedMethodsByName.Contains(m.Name);
        }

        /// <summary>
        /// Get handler for the given return type.
        /// </summary>
        /// <param name="m"></param>
        /// <returns></returns>
        Func<object, object> createResultHandler(Type returnType)
        {
            if (returnType.BaseType == typeof(Task) && returnType.IsGenericType)
            {
                // Task<T>
                // Create implementation of handleTask<T> for the
                // inner return type. Then cast it to a Func.
                return (Func<object, object>)
                    Delegate.CreateDelegate(typeof(Func<object, object>), this,
                    _handleResultTaskT.MakeGenericMethod(returnType.GenericTypeArguments[0]));
            }
            else if (returnType == typeof(Task))
            {
                // Task
                return handleTaskVoid;
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

        object handleTaskVoid(object first)
        {
            return ((Task)first).ContinueWith(t =>
            {
                logTask(t);
                _recorder.EndSubsegment();
            });
        }

        object handleTaskT<T>(object first)
        {
            return ((Task<T>)first).ContinueWith(t =>
            {
                logTask(t);
                _recorder.EndSubsegment();
                return t.Result;
            });
        }

        void logTask(Task t)
        {
            logException(t.Exception);

            if (t.GetType().IsGenericType)
            {
                logResult(((dynamic)t).Result);
            }
        }

        void logArgs(MethodInfo m, object[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                var val = args[i];
                var param = m.GetParameters()[i];

                _recorder.AddMetadata("args." + param.Name, val.ToString());

                if (_options.AnnotatedArgumentsByName.Contains(param.Name))
                {
                    _recorder.AddAnnotation(param.Name, val.ToString());
                }
            }
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
            _recorder.AddMetadata("result", o?.ToString() ?? "null");
        }
    }
}
