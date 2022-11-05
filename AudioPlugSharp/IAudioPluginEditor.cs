using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace AudioPlugSharp
{
    public interface IAudioPluginEditor
    {
        uint EditorWidth { get; }
        uint EditorHeight { get; }
        double GetDpiScale();
        void InitializeEditor();
        void ResizeEditor(uint newWidth, uint newHeight);
        void ShowEditor(IntPtr parentWindow);
        void HideEditor();
    }
}
