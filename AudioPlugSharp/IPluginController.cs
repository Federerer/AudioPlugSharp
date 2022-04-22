using System;
using System.Collections.Generic;
using System.Text;

namespace AudioPlugSharp
{
    public interface IPluginController
    {
        void AddParmameter(AudioPluginParameter parameter, ushort paramID);
    }
}
