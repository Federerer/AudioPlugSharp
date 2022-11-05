using System;
using AudioPlugSharp;

namespace SimpleExample
{
    public class SimpleExamplePlugin : IAudioPlugin
    {
        public string Company => "My Company";

        public string Website => "www.mywebsite.com";

        public string Contact => "contact@my.email";

        public string PluginName => "Simple Gain Plugin";

        public string PluginCategory => "Fx";

        public string PluginVersion => "1.0.0";

        // Unique 64bit ID for the plugin
        public ulong PluginID => 0xF57703946AFC4EF8;

        public IAudioHost Host { get; set; }

    }
}
