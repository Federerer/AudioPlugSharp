﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using AudioPlugSharp;

[assembly: System.Windows.ThemeInfo(
        System.Windows.ResourceDictionaryLocation.None,
        System.Windows.ResourceDictionaryLocation.SourceAssembly
    )]

namespace AudioPlugSharpWPF
{    
    public class AudioPluginWPFSaveState : AudioPluginSaveState
    {
        public uint EditorWidth { get; set; } = 800;
        public uint EditorHeight { get; set; } = 600;
    }

    public class AudioPluginWPF : AudioPluginBase
    {
        [DllImport("User32.dll")]
        private static extern IntPtr MonitorFromPoint([In] System.Drawing.Point pt, [In] uint dwFlags);

        [DllImport("Shcore.dll")]
        private static extern IntPtr GetDpiForMonitor([In] IntPtr hmonitor, [In] DpiType dpiType, [Out] out uint dpiX, [Out] out uint dpiY);

        public enum DpiType
        {
            Effective = 0,
            Angular = 1,
            Raw = 2,
        }

        public AudioPluginWPFSaveState WPFSaveState { get { return (SaveStateData as AudioPluginWPFSaveState) ?? new AudioPluginWPFSaveState(); } }

        public EditorWindow EditorWindow { get; set; }
        public UserControl EditorView { get; set; }

        public AudioPluginWPF()
        {
            SaveStateData = new AudioPluginWPFSaveState();
        }

        public override double GetDpiScale()
        {
            uint dpiX;
            uint dpiY;

            var pnt = new System.Drawing.Point(0, 0);
            var mon = MonitorFromPoint(pnt, 2);
            GetDpiForMonitor(mon, DpiType.Effective, out dpiX, out dpiY);

            return (double)dpiX / 96.0;
        }

        public virtual UserControl GetEditorView()
        {
            return new EditorView();
        }

        public override void ResizeEditor(uint newWidth, uint newHeight)
        {
            base.ResizeEditor(newWidth, newHeight);

            if (EditorWindow != null)
            {
                EditorWindow.SetSize(EditorWidth, EditorHeight);
            }
        }

        public override void ShowEditor(IntPtr parentWindow)
        {
            Logger.Log("Open editor. Thread ID is: " + System.Threading.Thread.CurrentThread.ManagedThreadId);

            if (EditorView == null)
                EditorView = GetEditorView();

            EditorWindow = new EditorWindow(this, EditorView);

            EditorWindow.SetSize(EditorWidth, EditorHeight);

            EditorWindow.Show(parentWindow);
        }

        public override byte[] SaveState()
        {
            WPFSaveState.EditorWidth = EditorWidth;
            WPFSaveState.EditorHeight = EditorHeight;

            return base.SaveState();
        }

        public override void RestoreState(byte[] stateData)
        {
            base.RestoreState(stateData);

            EditorWidth = WPFSaveState.EditorWidth;
            EditorHeight = WPFSaveState.EditorHeight;
        }
    }
}
