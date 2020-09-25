using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Irradiate
{
    using TypeAnnotater = Func<object, Tuple<object, string>>;
    using TypeFormatter = Func<object, object>;

    public struct AnnotatedArgumentByNameEntry
    {
        public string AnnotationName;
    }

    public struct AnnotatedArgumentByTypeEntry
    {
        public string Name;
        public string NewName;
    }


    public class Options
    {
        public Dictionary<string, AnnotatedArgumentByNameEntry> AnnotatedArgumentsByName { get; private set;} = new Dictionary<string, AnnotatedArgumentByNameEntry > ();
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

        /// <summary>
        /// Create an annotation from any method argument matching the given name.
        /// </summary>
        /// <param name="argumentName">The name of the argument. Is case-sensitive.</param>
        /// <param name="newName">Optional name for the annotation. If not specified then the argument name is used.</param>
        /// <returns></returns>
        public Options Annotate(string argumentName, string annotationName = null)
        {
            AnnotatedArgumentsByName[argumentName] = new AnnotatedArgumentByNameEntry
            {
                AnnotationName = annotationName,
            };

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
