using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlugSharp
{
    public interface IAudioPluginController
    {
        void Initialize();

        IReadOnlyList<AudioPluginParameter> Parameters { get; }
        AudioPluginParameter GetParameterByMidiController(uint ccNumber);

        void SetComponentState(byte[] state);

        IAudioPluginEditor CreateView();
    }
}
