using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;

namespace Matrix;

public class Matrix
{
    private Config Config { get; }

    private string Chars;
    private int Rows;
    private int Columns;
    private int FPS;
    private int Length;
    private int MoveSpeed;
    private int FadeSpeed;
    private int SpawnRate;
    private string Font;
    private int FontSize;
    private Rgba32 Background;
    private Rgba32 HeadColor;
    private Rgba32 MiddleColor;
    private int? Seed;

    private GridChar[,] grid;

    private Random random;

    private int[] spawnColumns;
    private char[] spawnChars;

    private Font font;

    public event EventHandler<OnFrameEventArgs> OnFrame;

    public Matrix(Config config)
    {
        Config = config;
        Chars = config.Characters;
        Rows = config.Rows;
        Columns = config.Columns;
        FPS = config.FPS;
        Length = config.Length;
        MoveSpeed = config.MoveSpeed;
        FadeSpeed = config.FadeSpeed;
        SpawnRate = config.SpawnRate;
        Font = config.Font;
        FontSize = config.FontSize;
        Background = config.Background;
        HeadColor = config.HeadColor;
        MiddleColor = config.MiddleColor;
        Seed = config.Seed;

        random = Seed.HasValue ? new Random(Seed.Value) : new Random();

        grid = new GridChar[Rows, Columns];
        for (int i = 0; i < Rows; i++)
        {
            for (int j = 0; j < Columns; j++)
            {
                grid[i, j] = new GridChar
                {
                    Character = ' ',
                    Color = Color.Black
                };
            }
        }

        var charsSpawned = Length * FPS / SpawnRate;
        spawnColumns = new int[charsSpawned];
        spawnChars = new char[charsSpawned];
        for (int i = 0; i < charsSpawned; i++)
        {
            spawnColumns[i] = random.Next(Columns);
            spawnChars[i] = Chars[random.Next(Chars.Length)];
        }

        font = SystemFonts.CreateFont(Font, FontSize);

        OnFrame += (sender, e) => { };
    }

    public List<Image<Rgba32>> Render()
    {
        List<Image<Rgba32>> frames = new();
        int frameCount = FPS * Length;

        int preFrames = FadeSpeed * 2 + Rows;

        for (int i = -preFrames; i < 0; i++)
        {
            Simulate(i);
        }

        for (int i = 0; i < frameCount; i++)
        {
            Simulate(i);
            frames.Add(RenderFrame());
            OnFrame(this, new OnFrameEventArgs(i, frameCount));
        }
        return frames;
    }

    private Image<Rgba32> RenderFrame()
    {
        Image<Rgba32> image = new Image<Rgba32>(Columns * FontSize, Rows * FontSize, Color.Black);
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                var gridChar = grid[x, y];
                if (gridChar.Alive)
                {
                    var textBounds = TextMeasurer.MeasureBounds(gridChar.Character.ToString(), new TextOptions(font));
                    var posx = x * FontSize + (FontSize - textBounds.Width) / 2;
                    image.Mutate(img => img.DrawText(gridChar.Character.ToString(), font, gridChar.Color, new PointF(posx, y * FontSize)));
                }
            }
        }
        return image;
    }

    private void Simulate(int frame)
    {
        GridChar[,] newGrid = new GridChar[Rows, Columns];
        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                newGrid[x, y] = grid[x, y].MemberClone();
            }
        }

        // GridChar[,] newGrid = new GridChar[Rows, Columns];
        // for (int y = 0; y < Rows; y++)
        // {
        //     for (int x = 0; x < Columns; x++)
        //     {
        //         newGrid[x, y] = new GridChar
        //         {
        //             Character = ' ',
        //             Color = Color.Black
        //         };
        //     }
        // }

        if (frame % SpawnRate == 0)
        {
            var col = spawnColumns.LoopedIndex(frame / SpawnRate);
            newGrid[col, 0] = new GridChar
            {
                Character = spawnChars.LoopedIndex(frame / SpawnRate),
                Color = HeadColor,
                Alive = true,
                FramesAlive = 0,
                Head = true
            };
        }

        for (int y = 0; y < Rows; y++)
        {
            for (int x = 0; x < Columns; x++)
            {
                if (grid[x, y].Alive)
                {
                    if (grid[x, y].Head && frame % MoveSpeed == 0)
                    {
                        if (y < Rows - 1)
                        {
                            newGrid[x, y + 1] = grid[x, y];
                            newGrid[x, y + 1].Head = true;
                            newGrid[x, y + 1].Character = Chars[new Random((int)grid[x, y].Character + grid[x, y].FramesAlive + y + x + (Seed ?? 0)).Next(Chars.Length)];
                        }
                        newGrid[x, y] = grid[x, y];
                        newGrid[x, y].Head = false;
                        newGrid[x, y].FramesAlive++;
                        newGrid[x, y].Color = ColorLerp(HeadColor, MiddleColor, grid[x, y].FramesAlive / (FadeSpeed / 2f));
                    }
                    if (!grid[x, y].Head)
                    {
                        newGrid[x, y] = grid[x, y];
                        bool firstFade = grid[x, y].FramesAlive < FadeSpeed / 4f;
                        if (firstFade)
                        {
                            newGrid[x, y].Color = ColorLerp(HeadColor, MiddleColor, grid[x, y].FramesAlive / (FadeSpeed / 4f));
                        }
                        else
                        {
                            newGrid[x, y].Color = ColorLerp(MiddleColor, Background, (grid[x, y].FramesAlive - FadeSpeed / 4f) / (FadeSpeed / 4f * 3f));
                        }

                        newGrid[x, y].FramesAlive++;
                    }

                    if (grid[x, y].FramesAlive >= FadeSpeed)
                    {
                        newGrid[x, y] = new GridChar
                        {
                            Character = ' ',
                            Color = Color.Black,
                            Alive = false
                        };
                    }
                }
            }
        }

        grid = newGrid;
    }

    private static Rgba32 ColorLerp(Rgba32 a, Rgba32 b, float t)
    {
        t = Math.Clamp(t, 0, 1);
        return new Rgba32(
            (byte)(a.R + (b.R - a.R) * t),
            (byte)(a.G + (b.G - a.G) * t),
            (byte)(a.B + (b.B - a.B) * t),
            (byte)(a.A + (b.A - a.A) * t)
        );
    }
}
