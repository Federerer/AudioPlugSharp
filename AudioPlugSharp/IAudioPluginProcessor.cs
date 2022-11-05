using System;
using System.Collections.Generic;
using System.Text;

namespace AudioPlugSharp
{
    public interface IAudioPluginProcessor
    {
        AudioIOPort[] InputPorts { get; }
        AudioIOPort[] OutputPorts { get; }
        EAudioBitsPerSample SampleFormatsSupported { get; }

        void Initialize();
        
        byte[] SaveState();
        void RestoreState(byte[] stateData);

        void InitializeProcessing();

        void Start();
        void Stop();

        void HandleNoteOn(int noteNumber, float velocity);
        void HandleNoteOff(int noteNumber, float velocity);
        void HandlePolyPressure(int noteNumber, float pressure);

        void Process();
    }
}
