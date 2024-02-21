using System;

namespace Matrix;

public class OnFrameEventArgs : EventArgs
{
    public int Frame { get; }
    public int TotalFrames { get; }

    public OnFrameEventArgs(int frame, int totalFrames)
    {
        Frame = frame;
        TotalFrames = totalFrames;
    }
}