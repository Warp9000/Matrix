using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Matrix;

public class Config
{
    public string Characters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    public int Rows = 32;
    public int Columns = 32;
    public int FPS = 15;
    public int Length = 8;
    public int MoveSpeed = 1;
    public int FadeSpeed = 30;
    public int SpawnRate = 4;
    public string Font = "Arial";
    public int FontSize = 16;

    [JsonConverter(typeof(Rgba32JsonConverter))]
    public Rgba32 Background = Color.Black;

    [JsonConverter(typeof(Rgba32JsonConverter))]
    public Rgba32 HeadColor = Color.White;

    [JsonConverter(typeof(Rgba32JsonConverter))]
    public Rgba32 MiddleColor = Color.Green;
    public int? Seed = null;

    public static Config? Parse(string file)
    {
        return JsonConvert.DeserializeObject<Config>(file);
    }
}