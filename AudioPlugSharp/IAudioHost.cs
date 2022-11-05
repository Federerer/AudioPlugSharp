﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioPlugSharp
{
    public interface IAudioHost
    {
        double SampleRate { get; }
        uint MaxAudioBufferSize { get; }
        EAudioBitsPerSample BitsPerSample { get; }

        double BPM { get; }
    }
}
