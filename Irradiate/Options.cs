using System;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;

namespace Irradiate
{
    public class Options
    {
        public HashSet<string> AnnotatedArgumentsByName { get; private set;} = new HashSet<string>();
        public List<string> ExcludedMethodsByName { get; private set; } = new List<string>();


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
