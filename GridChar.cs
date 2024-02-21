using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix;

public struct GridChar
{
    public char Character;
    public Rgba32 Color;
    public bool Alive;
    public int FramesAlive;
    public bool Head;
}