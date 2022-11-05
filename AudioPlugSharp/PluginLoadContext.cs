using System;
using System.Reflection;
using System.Runtime.Loader;

namespace AudioPlugSharp
{
    public class PluginLoadContext : AssemblyLoadContext
    {
        AssemblyDependencyResolver resolver;

        public PluginLoadContext(string pluginPath)
        {
            Logger.Log("Load context from: " + pluginPath);

            resolver = new AssemblyDependencyResolver(pluginPath);
        }

        protected override Assembly Load(AssemblyName assemblyName)
        {
            if (assemblyName.Name == "AudioPlugSharp")
            {
                Logger.Log("Skipping AudioPlugSharp assembly load");

                return AssemblyLoadContext.GetLoadContext(Assembly.GetExecutingAssembly()).LoadFromAssemblyName(assemblyName);
            }

            string assemblyPath = resolver.ResolveAssemblyToPath(assemblyName);

            Logger.Log("PluginLoadContext load [" + assemblyName + "] from: " + assemblyPath);

            if (assemblyPath != null)
            {
                return LoadFromAssemblyPath(assemblyPath);
            }

            return null;
        }      

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string libraryPath = resolver.ResolveUnmanagedDllToPath(unmanagedDllName);

            if (libraryPath != null)
            {
                return LoadUnmanagedDllFromPath(libraryPath);
            }

            return IntPtr.Zero;
        }
    }
}
