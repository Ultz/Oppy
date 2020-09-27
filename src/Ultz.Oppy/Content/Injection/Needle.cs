// This file is part of Oppy.
// 
// You may modify and distribute Oppy under the terms
// of the MIT license. See the LICENSE file for details.

using System;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using BlushingPenguin.JsonPath;
using Microsoft.Extensions.Logging;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Content.Injection
{
    /// <summary>
    /// The entry class for injecting well-known Oppy-specific values into objects.
    /// </summary>
    public static class Needle
    {
        private static readonly MethodInfo? _createLoggerGeneric = typeof(LoggerFactoryExtensions).GetMethods()
            .FirstOrDefault(x =>
                x.Name == nameof(LoggerFactoryExtensions.CreateLogger) &&
                x.GetParameters().Length == 1 &&
                x.GetParameters()[0].ParameterType == typeof(ILoggerFactory) &&
                x.ReturnType.IsGenericType &&
                !x.ReturnType.IsGenericTypeDefinition &&
                x.ReturnType.GetGenericTypeDefinition() == typeof(ILogger<>) &&
                x.IsStatic);

        /// <summary>
        /// Injects well-known Oppy-specific values into the given object.
        /// </summary>
        /// <param name="o">The object to inject.</param>
        /// <param name="currentListener">
        /// For context, the current listener. This value, and/or its children, may be injected into
        /// the object.
        /// </param>
        /// <param name="currentHost">
        /// For context, the current host. This value, and/or its children, may be injected into the
        /// object.
        /// </param>
        public static void Inject(object o, IListener currentListener, Host currentHost)
        {
            Type type = o.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var oppyConfigAttribute = fieldInfo.GetCustomAttribute<OppyConfigAttribute>();
                if (!(oppyConfigAttribute is null))
                {
                    var token = currentHost.Config.SelectToken(oppyConfigAttribute.JPath)?.GetRawText();
                    if (!(token is null))
                    {
                        fieldInfo.SetValue(o, JsonSerializer.Deserialize(token, fieldInfo.FieldType));
                        continue;
                    }
                }

                var oppyInjectAttribute = fieldInfo.GetCustomAttribute<OppyInjectAttribute>();
                if (!(oppyInjectAttribute is null))
                {
                    if (fieldInfo.FieldType == typeof(IListener))
                    {
                        fieldInfo.SetValue(o, currentListener);
                    }
                    else if (fieldInfo.FieldType == typeof(Host))
                    {
                        fieldInfo.SetValue(o, currentHost);
                    }
                    else if (fieldInfo.FieldType == typeof(ILogger))
                    {
                        fieldInfo.SetValue(o, currentListener.LoggerFactory?.CreateLogger(type));
                    }
                    else if (fieldInfo.FieldType == typeof(ILogger<>))
                    {
                        fieldInfo.SetValue(o, currentListener.LoggerFactory?.CreateLoggerGeneric(fieldInfo.FieldType));
                    }
                    else if (fieldInfo.FieldType == typeof(ILoggerFactory))
                    {
                        fieldInfo.SetValue(o, currentListener.LoggerFactory);
                    }
                }
            }

            foreach (var propertyInfo in type.GetProperties())
            {
                var oppyConfigAttribute = propertyInfo.GetCustomAttribute<OppyConfigAttribute>();
                if (!(oppyConfigAttribute is null))
                {
                    var token = currentHost.Config.SelectToken(oppyConfigAttribute.JPath)?.GetRawText();
                    if (!(token is null))
                    {
                        propertyInfo.SetValue(o, JsonSerializer.Deserialize(token, propertyInfo.PropertyType));
                        continue;
                    }
                }

                var oppyInjectAttribute = propertyInfo.GetCustomAttribute<OppyInjectAttribute>();
                if (!(oppyInjectAttribute is null))
                {
                    if (propertyInfo.PropertyType == typeof(IListener))
                    {
                        propertyInfo.SetValue(o, currentListener);
                    }
                    else if (propertyInfo.PropertyType == typeof(Host))
                    {
                        propertyInfo.SetValue(o, currentHost);
                    }
                    else if (propertyInfo.PropertyType == typeof(ILogger))
                    {
                        propertyInfo.SetValue(o, currentListener.LoggerFactory?.CreateLogger(type));
                    }
                    else if (propertyInfo.PropertyType.IsGenericType &&
                             propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ILogger<>))
                    {
                        propertyInfo.SetValue(o,
                            currentListener.LoggerFactory?.CreateLoggerGeneric(propertyInfo.PropertyType));
                    }
                    else if (propertyInfo.PropertyType == typeof(ILoggerFactory))
                    {
                        propertyInfo.SetValue(o, currentListener.LoggerFactory);
                    }
                }
            }
        }

        private static ILogger? CreateLoggerGeneric(this ILoggerFactory loggerFactory, Type fieldOrPropertyType)
        {
            return (ILogger?) _createLoggerGeneric?.MakeGenericMethod(fieldOrPropertyType.GenericTypeArguments[0])
                .Invoke(null, new object[] {loggerFactory});
        }
    }
}