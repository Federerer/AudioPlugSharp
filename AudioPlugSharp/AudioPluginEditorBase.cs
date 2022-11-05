using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlugSharp
{
    public class AudioPluginEditorBase : IAudioPluginEditor
    {
        public bool HasUserInterface { get; protected set; }
        public uint EditorWidth { get; protected set; } = 400;
        public uint EditorHeight { get; protected set; } = 200;

        public virtual double GetDpiScale() => 1.0;

        List<AudioPluginParameter> parameterList = new List<AudioPluginParameter>();

        public IReadOnlyList<AudioPluginParameter> Parameters => parameterList.AsReadOnly();

        Dictionary<string, AudioPluginParameter> parameterDict = new Dictionary<string, AudioPluginParameter>();
        Dictionary<uint, AudioPluginParameter> parameterCCDict = new Dictionary<uint, AudioPluginParameter>();

        public virtual void InitializeEditor()
        {
            Logger.Log("Initialize Editor");
        }

        public virtual void ResizeEditor(uint newWidth, uint newHeight)
        {
            EditorWidth = newWidth;
            EditorHeight = newHeight;
        }

        public virtual void ShowEditor(IntPtr parentWindow)
        {
        }

        public virtual void HideEditor()
        {
        }

        public void AddParameter(AudioPluginParameter parameter)
        {
            parameterList.Add(parameter);
            parameterDict[parameter.ID] = parameter;

            parameter.Value = parameter.DefaultValue;
        }

        public AudioPluginParameter GetParameter(string paramID)
        {
            return parameterDict[paramID];
        }

        public void AddMidiControllerMapping(AudioPluginParameter parameter, uint ccNumber)
        {
            parameterCCDict[ccNumber] = parameter;
        }

        public AudioPluginParameter GetParameterByMidiController(uint ccNumber)
        {
            if (!parameterCCDict.ContainsKey(ccNumber))
                return null;

            return parameterCCDict[ccNumber];
        }

        public void SetComponentState(byte[] state)
        {
            throw new NotImplementedException();
        }
    }
}
