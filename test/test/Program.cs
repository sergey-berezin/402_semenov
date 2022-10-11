using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;

public class ConsoleApp1
{
    static DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
    {
        var w = img.Width;
        var h = img.Height;
        var t = new DenseTensor<float>(new[] { 1, 1, h, w });

        img.ProcessPixelRows(pa =>
        {
            for (int y = 0; y < h; y++)
            {
                Span<Rgb24> pixelSpan = pa.GetRowSpan(y);
                for (int x = 0; x < w; x++)
                {
                    t[0, 0, y, x] = pixelSpan[x].R; // B and G are the same
                }
            }
        });

        return t;
    }
    public async static Task Main(string[] args)
    {
        using Image<Rgb24> img = Image.Load<Rgb24>("..//..//..//..//face1.jpg");
        img.Mutate(ctx => {
            ctx.Resize(new Size(64, 64));
        });
        var inputs = GrayscaleImageToTensor(img);
        var a = new Emotions.EmotionsAsync();
        var res = await a.Emotions(inputs, CancellationToken.None);
        foreach(var i in res)
            Console.WriteLine($"{i.Item1}: {(i.Item2 * 100):f4}%");
    }
}
