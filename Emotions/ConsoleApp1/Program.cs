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
    public static int Main(string[] args)
    {
        using Image<Rgb24> img = Image.Load<Rgb24>("..//..//..//..//86bc3cc082bc4ccc4b755653d675190f.jpg");
        img.Mutate(ctx => {
            ctx.Resize(new Size(64, 64));
        });
        var inputs = GrayscaleImageToTensor(img);
        var a = new Emotions.EmotionsAsync();
        foreach(string i in a.Emotions(inputs, CancellationToken.None).Result)
            Console.WriteLine(i);
        return 0;
    }
}
