using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading;
using System;

namespace Emotions_2
{
    public partial class MainWindow : Window
    {

        
        

        public MyDataClass MyData { get; set; }
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        public MainWindow()
        {
            InitializeComponent();
            MyData = new MyDataClass();
            DataContext = this;
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
        }
        public async void Create_Collection(object sender, RoutedEventArgs e)
        {
            cancelTokenSource = new CancellationTokenSource();
            var dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            List<string> path = new List<string>();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                Button_Clear(sender, e);
                MyData.FolderName = dialog.SelectedPath;
                foreach (var t in System.IO.Directory.GetFiles(dialog.SelectedPath))
                    path.Add(t);
                int MaxValue = path.Count();
                foreach(var f in path)
                {
                    using Image<Rgb24> img = SixLabors.ImageSharp.Image.Load<Rgb24>(f);
                    img.Mutate(ctx =>
                    {
                        ctx.Resize(new SixLabors.ImageSharp.Size(64, 64));
                    });
                    var inputs = GITT.GrayscaleImageToTensor(img);
                    var a = new Emotions.EmotionsAsync();
                    List<Tuple<string, float>> r = new();
                    try
                    {
                        token = cancelTokenSource.Token;
                        var res = await a.Emotions(inputs, token);
                        r = res.OrderByDescending(t => t.Item2).ToList();
                    }
                    catch (Exception ex) { MessageBox.Show("Calculation Canceled"); break; }

                    MyData.Progress += 100.0 / MaxValue;
                    MyData.MyPicture.Add(new Picture
                    {
                        Name = f,
                        Emotions = r
                    });
                };
            }
        }
        public void Button_Clear(object sender, RoutedEventArgs e)
        {
            MyData.MyPicture.Clear();
            MyData.FolderName = "";
            MyData.Progress = 0;
            MyData.MaxValue = 100;
        }

        private void Button_Cancel(object sender, RoutedEventArgs e)
        {
            cancelTokenSource.Cancel();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(MyData.Progress.ToString());
        }
    }
}
