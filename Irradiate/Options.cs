using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Irradiate
{
    using TypeAnnotater = Func<object, Tuple<object, string>>;
    using TypeFormatter = Func<object, object>;

    public class Options
    {
        public HashSet<string> AnnotatedArgumentsByName { get; private set;} = new HashSet<string>();
        public Dictionary<Type, List<TypeAnnotater>> AnnotatedArgumentsByType { get; private set; } = new Dictionary<Type, List<TypeAnnotater>>();
        public List<string> ExcludedMethodsByName { get; private set; } = new List<string>();
        public Dictionary<Type, TypeFormatter> TypeFormatters { get; private set; } = new Dictionary<Type, TypeFormatter>();

        public Options AddTypeFormatter<T>(Func<T, object> f)
        {
            return AddTypeFormatter(typeof(T), (object o) => f((T)o));
        }

        public Options AddTypeFormatter(Type t, TypeFormatter f)
        {
            TypeFormatters[t] = f;
            return this;
        }

        public Options Annotate(string argumentName)
        {
            AnnotatedArgumentsByName.Add(argumentName);
            return this;
        }

        public Options Annotate<TIn>(Func<TIn, Tuple<object, string>> selector)
        {
            if (!AnnotatedArgumentsByType.ContainsKey(typeof(TIn)))
            {
                AnnotatedArgumentsByType[typeof(TIn)] = new List<TypeAnnotater>();
            }
            AnnotatedArgumentsByType[typeof(TIn)].Add(o => selector((TIn)o));
            return this;
        }

        public Options ExcludeMethod(string name)
        {
            ExcludedMethodsByName.Add(name);
            return this;
        }
    }
}
