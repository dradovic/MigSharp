using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;

namespace MigSharp.Providers
{
    internal static class ProviderExtensions
    {
        public static IEnumerable<SupportsAttribute> GetSupportsAttributes(this IProvider provider)
        {
            return provider.GetType().GetCustomAttributes(typeof(SupportsAttribute), true)
                           .Cast<SupportsAttribute>()
                           .OrderBy(a => a.DbType);
        }

        public static IEnumerable<UnsupportedMethod> GetUnsupportedMethods(this IProvider provider)
        {
            var unsupportedMethods = new List<UnsupportedMethod>();
            foreach (MethodInfo method in typeof(IProvider).GetMethods())
            {
                try
                {
                    method.Invoke(provider, GetDefaultParameters(method));
                }
                catch (TargetInvocationException x)
                {
                    NotSupportedException notSupportedException = x.InnerException as NotSupportedException;
                    if (notSupportedException != null)
                    {
                        unsupportedMethods.Add(new UnsupportedMethod(method.Name, notSupportedException.Message));
                    }
                    // other exception types are disregarded
                }
            }
            try
            {
                provider.DropTable(string.Empty, true);
            }
            catch (NotSupportedException x)
            {
                unsupportedMethods.Add(new UnsupportedMethod("DropTableIfExists", x.Message));
            }
            return unsupportedMethods;
        }

        private static object[] GetDefaultParameters(MethodInfo method)
        {
            var parameters = new List<object>();
            foreach (ParameterInfo parameter in method.GetParameters())
            {
                if (parameter.ParameterType == typeof(string))
                {
                    parameters.Add(string.Empty);
                }
                else if (parameter.ParameterType == typeof(IEnumerable<CreatedColumn>))
                {
                    parameters.Add(Enumerable.Empty<CreatedColumn>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<Column>))
                {
                    parameters.Add(Enumerable.Empty<Column>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<ColumnReference>))
                {
                    parameters.Add(Enumerable.Empty<ColumnReference>());
                }
                else if (parameter.ParameterType == typeof(IEnumerable<string>))
                {
                    parameters.Add(Enumerable.Empty<string>());
                }
                else if (parameter.ParameterType == typeof(Column))
                {
                    parameters.Add(new Column(string.Empty, new DataType(0), false, null));
                }
                else
                {
                    try
                    {
                        parameters.Add(Activator.CreateInstance(parameter.ParameterType));
                    }
                    catch (MissingMethodException)
                    {
                        Debug.Fail(string.Format("Could not find default constructor for type: {0}", parameter.ParameterType));
                        throw;
                    }
                }
            }
            return parameters.ToArray();
        }
    }
}