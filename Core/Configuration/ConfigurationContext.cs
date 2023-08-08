﻿using System.Reflection;
using LitJson;
using SummerFramework.Base;

namespace SummerFramework.Core.Configuration;

public class ConfigurationContext
{
    internal string Path { get; set; }

    public ConfigurationContext(string path)
    {
        Path = path;

        var context = File.ReadAllText(Path);
        var ce = JsonMapper.ToObject(context);

        if (ce["methods"] != null)
        {
            for (int i = 0; i < ce["methods"].Count; i++)
            {
                MethodInfo? dlgt;

                var current = ce["methods"][i];
                var type = ((string)current["type"]);
                var identifier = ((string)current["identifier"]);
                var link = (string)current["link"];

                dlgt = ObjectFactory.GetFunction(link);

                if (dlgt != null)
                    ConfiguredMethodPool.Instance.Add(identifier, dlgt);
            }
        }

        for (int i = 0; i < ce["objects"].Count; i++)
        {
            object? obj;

            var current = ce["objects"][i];
            var type = ((string)current["type"]);
            var identifier = ((string)current["identifier"]);
            string value;

            if (((string)current["value"]).StartsWith("@"))
            {
                ObjectFactory.IsMethodInvoke((string)current["value"], out obj);
                ConfiguredObjectPool.Instance.Add(identifier, obj!);
                break;
            }

            if (ObjectFactory.value_types.Contains(type))
            {
                value = ((string)current["value"]);
                obj = ObjectFactory.CreateValueType(type, value);
            }
            else
            {
                value = current["value"].ToJson();
                obj = ObjectFactory.CreateReferenceType(type, value);
            }

            if (obj != null)
                ConfiguredObjectPool.Instance.Add(identifier, obj);
        }
    }

    public object GetObject(string identifier)
    {
        return ConfiguredObjectPool.Instance.Get(identifier);
    }
}