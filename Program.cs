using FFMpegCore;
using FFMpegCore.Extensions.System.Drawing.Common;
using FFMpegCore.Pipes;
using Newtonsoft.Json;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Matrix;

public class Program
{
    public static void Main(string[] args)
    {
        if (args.Any(arg => arg == "-h" || arg == "--help") || args.Length == 0)
        {
            Console.WriteLine("Usage: matrix <config.json> -- Generates a matrix animation from a config file");
            Console.WriteLine("       matrix --writeconfig -- Writes a default config file to config.json");
            Console.WriteLine("       matrix --help        -- Shows this help message");
            return;
        }
        if (args.Any(arg => arg.ToLower() == "--writeconfig"))
        {
            File.WriteAllText("config.json", JsonConvert.SerializeObject(new Config(), Formatting.Indented));
            return;
        }
        if (args.Length > 0)
        {
            var config = Config.Parse(File.ReadAllText(args[0]));
            if (config is null)
            {
                Console.WriteLine("Invalid config file");
                return;
            }
            var m = new Matrix(config);

            Console.Write("Rendering...  ");

            m.OnFrame += (sender, e) =>
            {
                Console.CursorLeft = 0;
                Console.Write($"\rRendering...  {e.Frame}/{e.TotalFrames}        ");
            };

            var frames = m.Render();

            Console.WriteLine("\rRendering...  Done!        ");
            Console.WriteLine("Saving gif...");
            SaveGif(frames, config.FPS, "matrix.gif");

            if (!File.Exists("./ffmpeg/ffmpeg.exe"))
            {
                Console.WriteLine("FFmpeg not found, skipping mp4 generation");
                Console.WriteLine("Put an ffmpeg binary in the ffmpeg folder to enable mp4 generation");
                return;
            }

            Console.WriteLine("Saving mp4...");
            SaveMp4(frames, config.FPS, "matrix.mp4");
        }
    }

    public static void SaveGif(List<Image<Rgba32>> frames, int fps, string path)
    {
        using var gif = new Image<Rgba32>(frames[0].Width, frames[0].Height, Color.Black);
        foreach (var frame in frames)
        {
            var metadata = frame.Frames.RootFrame.Metadata.GetGifMetadata();
            metadata.FrameDelay = 100 / fps;
            gif.Frames.AddFrame(frame.Frames.RootFrame);
        }
        gif.Metadata.GetGifMetadata().RepeatCount = 0;
        gif.Frames.RemoveFrame(0);
        gif.SaveAsGif(path);
    }

    public static void SaveMp4(List<Image<Rgba32>> frames, int fps, string path)
    {
        GlobalFFOptions.Configure(options => options.BinaryFolder = AppDomain.CurrentDomain.BaseDirectory + "ffmpeg");

        var videoFramesSource = new RawVideoPipeSource(CreateFrames(frames))
        {
            FrameRate = fps
        };

        FFMpegArguments
            .FromPipeInput(videoFramesSource)
            .OutputToFile(path, true, options => options
                // .WithVideoCodec("libx264")
                .WithFramerate(fps))
            .ProcessSynchronously();
    }

    public static IEnumerable<IVideoFrame> CreateFrames(List<Image<Rgba32>> frames)
    {
        foreach (var frame in frames)
        {
            using var memoryStream = new MemoryStream();
            frame.SaveAsBmp(memoryStream);
            System.Drawing.Bitmap bitmap = (System.Drawing.Bitmap)System.Drawing.Bitmap.FromStream(memoryStream);
            yield return new BitmapVideoFrameWrapper(bitmap);
        }
    }
}
