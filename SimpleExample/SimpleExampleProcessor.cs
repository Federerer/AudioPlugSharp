using AudioPlugSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExample
{
    public class SimpleExampleProcessor : IAudioPluginProcessor
    {
        public SimpleExampleProcessor()
        {
            Logger.Log("Created managed processor");
        }
        public AudioIOPort[] InputPorts { get; } = new[] { new AudioIOPort("Mono Input", EAudioChannelConfiguration.Mono) };

        public AudioIOPort[] OutputPorts { get; } = new[] { new AudioIOPort("Mono Output", EAudioChannelConfiguration.Mono) };

        public EAudioBitsPerSample SampleFormatsSupported => EAudioBitsPerSample.Bits32 | EAudioBitsPerSample.Bits64;

        public void HandleNoteOff(int noteNumber, float velocity)
        {
            throw new NotImplementedException();
        }

        public void HandleNoteOn(int noteNumber, float velocity)
        {
            throw new NotImplementedException();
        }

        public void HandlePolyPressure(int noteNumber, float pressure)
        {
            throw new NotImplementedException();
        }

        public void Initialize()
        {

            //AddParameter(new AudioPluginParameter
            //{
            //    ID = "gain",
            //    Name = "Gain",
            //    Type = EAudioPluginParameterType.Float,
            //    MinValue = -20,
            //    MaxValue = 20,
            //    DefaultValue = 0,
            //    ValueFormat = "{0:0.0}dB"
            //});
        }

        public void InitializeProcessing()
        {
            Logger.Log(nameof(InitializeProcessing));
        }

        public void Process()
        {
            double gain = 1d;//GetParameter("gain").Value;
            double linearGain = Math.Pow(10.0, 0.05 * gain);

            var monoInput = InputPorts[0];
            var monoOutput = OutputPorts[0];

            monoInput.ReadData();

            double[] inSamples = monoInput.GetAudioBuffers()[0];
            double[] outSamples = monoOutput.GetAudioBuffers()[0];

            for (int i = 0; i < inSamples.Length; i++)
            {
                outSamples[i] = inSamples[i] * linearGain;
            }

            monoOutput.WriteData();
        }

        public void RestoreState(byte[] stateData)
        {
            Logger.Log(nameof(RestoreState));
        }

        public byte[] SaveState()
        {
            Logger.Log(nameof(SaveState));
            return new byte[0];
        }

        public void Start()
        {
            Logger.Log(nameof(Start));
        }

        public void Stop()
        {
            Logger.Log(nameof(Stop));
        }
    }
}
