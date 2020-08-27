﻿using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Irradiate
{
    public class Options
    {
        public HashSet<string> AnnotatedArgumentsByName { get; private set;} = new HashSet<string>();
        public List<string> ExcludedMethodsByName { get; private set; } = new List<string>();
        public Dictionary<Type, Func<object, object>> TypeFormatters { get; private set; } = new Dictionary<Type, Func<object, object>>();

        public Options AddTypeFormatter<T>(Func<T, object> f)
        {
            return AddTypeFormatter(typeof(T), (object o) => f((T)o));
        }

        public Options AddTypeFormatter(Type t, Func<object, object> f)
        {
            TypeFormatters[t] = f;
            return this;
        }

        public Options AnnotateArgument(string name)
        {
            AnnotatedArgumentsByName.Add(name);
            return this;
        }

        public Options ExcludeMethod(string name)
        {
            ExcludedMethodsByName.Add(name);
            return this;
        }
    }
}
