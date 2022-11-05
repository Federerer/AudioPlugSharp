using System;
using System.IO;
using System.Reflection;

namespace AudioPlugSharp
{
    public class PluginLoader
    {
        private PluginLoadContext _loadContext;
        private readonly string _pluginAssemblyName;

        private Assembly _pluginAssembly;

        public PluginLoader(string path, string pluginAssemblyName)
        {
            _pluginAssemblyName = pluginAssemblyName;
            _loadContext = new PluginLoadContext(Path.Combine(path, pluginAssemblyName) + ".dll");
        }

        private Assembly LoadAssembly()
        {
            try
            {
                Logger.Log("Load assembly: " + _pluginAssemblyName);

                AssemblyName actualAssemblyName = new AssemblyName(_pluginAssemblyName);

                Logger.Log("Actual assembly name is " + actualAssemblyName);

                Assembly assembly = _loadContext.LoadFromAssemblyName(actualAssemblyName);

                return assembly;
            }
            catch (Exception ex)
            {
                Logger.Log("Unable to load assembly: " + _pluginAssemblyName + " with: " + ex.ToString());
            }

            return null;
        }

        public T GetObjectByInterface<T>()
        {
            if (_pluginAssembly is null)
            {
                _pluginAssembly = LoadAssembly();
            }

            var interfaceType = typeof(T);
            Logger.Log("Looking for type: " + interfaceType.AssemblyQualifiedName + " in assembly " + _pluginAssembly.FullName);

            Logger.Log("FullyQualifiedName: " + typeof(IAudioPlugin).Module.FullyQualifiedName);


            Type matchedType = null;

            try
            {
                Logger.Log("Assembly types: " + _pluginAssembly.GetTypes().Length);


                foreach (Type type in _pluginAssembly.GetTypes())
                {
                    if (type.IsPublic)
                    {
                        Logger.Log("Checking type: " + type.Name + " -- " + type.Module.FullyQualifiedName);

                        if (interfaceType.IsAssignableFrom(type))
                        {
                            Logger.Log("Matched");

                            matchedType = type;

                            break;
                        }
                    }

                    if (matchedType != null)
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Type scanning failed with: " + ex.ToString());
            }

            if (matchedType == null)
            {
                Logger.Log("Unable to find type");

                return default;
            }

            try
            {
                return (T)Activator.CreateInstance(matchedType);
            }
            catch (Exception ex)
            {
                Logger.Log("Failed to create object: " + ex.ToString());
                throw;
            }
        }
    }
}
