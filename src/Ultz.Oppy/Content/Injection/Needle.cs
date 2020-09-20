using System;
using System.Reflection;
using System.Text.Json;
using BlushingPenguin.JsonPath;
using Ultz.Oppy.Core;

namespace Ultz.Oppy.Content.Injection
{
    public static class Needle
    {
        public static void Inject(object o, Listener currentListener, Host currentHost)
        {
            Type type = o.GetType();
            foreach (var fieldInfo in type.GetFields())
            {
                var oppyConfigAttribute = fieldInfo.GetCustomAttribute<OppyConfigAttribute>();
                if (!(oppyConfigAttribute is null))
                {
                    fieldInfo.SetValue(o,
                        JsonSerializer.Deserialize(
                            currentHost.Config.SelectToken(oppyConfigAttribute.JPath)?.GetRawText(),
                            fieldInfo.FieldType));
                    continue;
                }
                
                var oppyInjectAttribute = fieldInfo.GetCustomAttribute<OppyInjectAttribute>();
                if (!(oppyInjectAttribute is null))
                {
                    if (fieldInfo.FieldType == typeof(Listener))
                    {
                        fieldInfo.SetValue(o, currentListener);
                    }
                    else if (fieldInfo.FieldType == typeof(Host))
                    {
                        fieldInfo.SetValue(o, currentHost);
                    }
                }
            }
        }
    }
}