using System.Collections.Generic;
using System.Linq;
using System.Windows;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Threading;
using System;
using System.Windows.Controls;
using System.Collections.ObjectModel;

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
                int count = 0;
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
                        Interlocked.Increment(ref count);
                        token = cancelTokenSource.Token;
                        var res = await a.Emotions(inputs, token);
                        r = res.OrderByDescending(t => t.Item2).ToList();
                    }
                    catch (Exception ex) { MessageBox.Show("Calculation Canceled"); break; }

                    MyData.Progress += 100.0 / MaxValue;
                    MyData.MyPicture.Add(new Picture
                    {
                        Name = f,
                        Emotions = r,
                        Number = count
                    });
                };
            }
        }
        private void ComboBox_Selected(object sender, RoutedEventArgs e)
        {
            string emot = MyData.EmotList.SelectedEmot.f;
            List<Tuple<string, Tuple<string, float>>> L = new();
            int num = 1;

            foreach (var pic in MyData.MyPicture)   
            {
                L.Add((pic.Name, pic.Emotions.Where(t => t.Item1 == emot).ToList()[0]).ToTuple());
            }
            foreach (var t in L.OrderByDescending(t => t.Item2.Item2))
                MyData.MyPicture.Where(p => p.Name == t.Item1).ToList()[0].Number = num++;
            var s = MyData.MyPicture.OrderBy(t => t.Number).ToList();
            MyData.MyPicture.Clear();
            foreach (var pic in s)
                MyData.MyPicture.Add(pic);
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
    }
}
