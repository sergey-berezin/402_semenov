using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Linq;



namespace Emotions
{
    public class EmotionsAsync
    {
        private InferenceSession session;
        public EmotionsAsync()
        {

            using var modelStream = typeof(EmotionsAsync).Assembly
                .GetManifestResourceStream("Emotions.emotion-ferplus-7.onnx");
            using var memoryStream = new MemoryStream();
            modelStream.CopyTo(memoryStream);
            this.session = new InferenceSession(memoryStream.ToArray());
        }

        static private List<NamedOnnxValue> Tensor(DenseTensor<float> input)
        {
            return new List<NamedOnnxValue> { NamedOnnxValue.CreateFromTensor("Input3", input) };
        }

        public async Task<string[]> Emotions
               (DenseTensor<float> pic, CancellationToken token)
        {
            return await Task<string[]>.Factory.StartNew(() =>
            {
                var inputs = Tensor(pic);
                IDisposableReadOnlyCollection<DisposableNamedOnnxValue> res;

                lock (session)
                {
                    res = session.Run(inputs);
                }

                var result = Softmax(res.First(v =>
                            v.Name == "Plus692_Output_0").AsEnumerable<float>().ToArray());

                string[] keys = { "neutral", "happiness", "surprise", "sadness", "anger", "disgust", "fear", "contempt" };
                for (int i = 0; i < keys.Count(); i++)
                    keys[i] += $": {(result[i] * 100):f4}%";
                return keys;
            }, token, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        ~EmotionsAsync()
        {
            session.Dispose();
        }

        static float[] Softmax(float[] z)
        {
            var exps = z.Select(x => Math.Exp(x)).ToArray();
            var sum = exps.Sum();
            return exps.Select(x => (float)(x / sum)).ToArray();
        }
    }
}