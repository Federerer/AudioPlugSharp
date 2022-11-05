using AudioPlugSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimpleExample
{
    public class SimplePluginController : IAudioPluginController
    {

        public IReadOnlyList<AudioPluginParameter> Parameters => throw new NotImplementedException();

        public void AddParameter(AudioPluginParameter parameter)
        {
            
        }

        public IAudioPluginEditor CreateView()
        {
            return null;
        }

        public AudioPluginParameter GetParameter(string paramID)
        {
            return null;
        }

        public AudioPluginParameter GetParameterByMidiController(uint ccNumber)
        {
            return null;
        }

        public void Initialize()
        {

        }

        public void SetComponentState(byte[] state)
        {
           
        }
    }
}
