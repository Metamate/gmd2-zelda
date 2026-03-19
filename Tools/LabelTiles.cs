#:package System.Drawing.Common@9.*

using System.Drawing;
using System.Drawing.Imaging;

if (args.Length < 3)
{
    Console.WriteLine("Usage: dotnet run LabelTiles.cs <image> <tile_width> <tile_height> [output] [--scale N]");
    return 1;
}

string inputPath  = args[0];
int    tileWidth  = int.Parse(args[1]);
int    tileHeight = int.Parse(args[2]);

int scaleIdx = Array.IndexOf(args, "--scale");
int scale    = scaleIdx >= 0 ? int.Parse(args[scaleIdx + 1]) : 3;

string outputPath = args.Length > 3 && !args[3].StartsWith("--")
    ? args[3]
    : Path.Combine(
        Path.GetDirectoryName(inputPath)!,
        Path.GetFileNameWithoutExtension(inputPath) + "_labeled" + Path.GetExtension(inputPath));

using var src  = new Bitmap(inputPath);
int cols = src.Width  / tileWidth;
int rows = src.Height / tileHeight;

int outW = src.Width  * scale;
int outH = src.Height * scale;

using var img  = new Bitmap(outW, outH);
using var draw = Graphics.FromImage(img);

draw.InterpolationMode  = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
draw.PixelOffsetMode    = System.Drawing.Drawing2D.PixelOffsetMode.Half;
draw.DrawImage(src, 0, 0, outW, outH);

using var shadow = new SolidBrush(Color.FromArgb(200, 0, 0, 0));
using var text   = new SolidBrush(Color.Yellow);
using var font   = new Font("Arial", Math.Max(5f, tileHeight * scale * 0.25f), FontStyle.Bold);
using var grid   = new Pen(Color.FromArgb(180, 0, 120, 255), 1f);

int tw = tileWidth  * scale;
int th = tileHeight * scale;

// Gridlines
for (int col = 0; col <= cols; col++)
    draw.DrawLine(grid, col * tw, 0, col * tw, rows * th);
for (int row = 0; row <= rows; row++)
    draw.DrawLine(grid, 0, row * th, cols * tw, row * th);

// Labels
for (int row = 0; row < rows; row++)
for (int col = 0; col < cols; col++)
{
    float  x     = col * tw + 1;
    float  y     = row * th + 1;
    string label = (row * cols + col).ToString();

    draw.DrawString(label, font, shadow, x + 1, y + 1);
    draw.DrawString(label, font, text,   x,     y);
}

img.Save(outputPath, ImageFormat.Png);
Console.WriteLine($"Saved {cols * rows} labelled tiles ({scale}x scale) → {outputPath}");
return 0;
