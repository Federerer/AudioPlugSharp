using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace AudioPlugSharp
{
    public class AudioPluginBase : IAudioPlugin
    {
        //
        // IAudioPlugin Properties
        //

        public string Company { get; protected set; }
        public string Website { get; protected set; }
        public string Contact { get; protected set; }
        public string PluginName { get; protected set; }
        public string PluginCategory { get; protected set; }
        public string PluginVersion { get; protected set; }
        public ulong PluginID { get; protected set; }

        public IAudioHost Host { get; set; }


        //
        // IAudioPluginProcessor Properties
        //

        public AudioIOPort[] InputPorts { get; protected set; }
        public AudioIOPort[] OutputPorts { get; protected set; }
        public EAudioBitsPerSample SampleFormatsSupported { get; protected set; }

        public AudioPluginSaveState SaveStateData { get; protected set; }

        //
        // IAudioPluginEditor Properties
        //

        public AudioPluginBase()
        {
            SaveStateData = new AudioPluginSaveState();

            InputPorts = new AudioIOPort[0];
            OutputPorts = new AudioIOPort[0];

            SampleFormatsSupported = EAudioBitsPerSample.Bits32 | EAudioBitsPerSample.Bits64;
        }

        //
        // IAudioPluginProcessor Methods
        //

        public virtual void Initialize()
        {
            Logger.Log("Initializing processor"); 
        }


        public virtual byte[] SaveState()
        {
            //SaveStateData.SaveParameterValues(Parameters);

            XmlSerializer serializer = new XmlSerializer(SaveStateData.GetType());

            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    serializer.Serialize(memoryStream, SaveStateData);

                    return memoryStream.ToArray();
                }
            }
            catch (Exception ex)
            {
                Logger.Log("Save state serialization failed with: " + ex.ToString());
            }

            return null;
        }

        public virtual void RestoreState(byte[] stateData)
        {
            if (stateData != null)
            {
                XmlSerializer serializer = new XmlSerializer(SaveStateData.GetType());

                try
                {
                    using (MemoryStream memoryStream = new MemoryStream(stateData))
                    {
                        SaveStateData = serializer.Deserialize(memoryStream) as AudioPluginSaveState;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log("Save state deserialization failed with: " + ex.ToString());
                }

                //SaveStateData.RestoreParameterValues(Parameters);
            }
        }


        public virtual void InitializeProcessing()
        {
            Logger.Log("Initialize Processing");
        }

        public virtual void Start()
        {
            Logger.Log("Start Processor");
        }

        public virtual void Stop()
        {
            Logger.Log("Stop Processor");
        }

        public virtual void HandleNoteOn(int noteNumber, float velocity)
        {
        }

        public virtual void HandleNoteOff(int noteNumber, float velocity)
        {
        }

        public virtual void HandlePolyPressure(int noteNumber, float pressure)
        {
        }

        public virtual void Process()
        {
        }

    }
}
