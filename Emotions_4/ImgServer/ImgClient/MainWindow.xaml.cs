using Contracts;
using Newtonsoft.Json;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ImgClient
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MyDataClass MyData { get; set; }
        CancellationTokenSource cancelTokenSource;
        CancellationToken token;
        Ookii.Dialogs.Wpf.VistaFolderBrowserDialog dialog;
        List<string> path;
        private readonly string url = "https://localhost:5000/images";
        private const int MaxRetries = 3; 
        public MainWindow()
        {
            InitializeComponent();
            MyData = new MyDataClass();
            DataContext = this;
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            Calc.IsEnabled = false;
        }
        private void Create_Collection(object sender, RoutedEventArgs e)
        {
            cancelTokenSource = new CancellationTokenSource();
            dialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            path = new List<string>();
            if (dialog.ShowDialog(this).GetValueOrDefault())
            {
                MyData.FolderName = dialog.SelectedPath;
                Calc.IsEnabled = true;
            }
            else
                MessageBox.Show("Can't open folder");
        }


        private async void Start_Calculation(object sender, RoutedEventArgs e)
        {
            Button_Clear(sender, e);
            MyData.FolderName = dialog.SelectedPath;
            foreach (var t in System.IO.Directory.GetFiles(dialog.SelectedPath))
                path.Add(t);
            int MaxValue = path.Count();
            Emot_Combobox.IsEnabled = false;
            Folder_Button.IsEnabled = false;
            Clear_Button.IsEnabled = false;
            Calc.IsEnabled = false;
            int count = 0;
            var tasks = new List<Task<int>>();

            foreach (var f in path)
            {
                try
                {
                    var content = new StringContent(f);
                    HttpClient client = new HttpClient();
                    var response = await client.PostAsJsonAsync(url, f, cancelTokenSource.Token);
                    string s = await response.Content.ReadAsStringAsync();
                    count++;
                    DBpic pic = JsonConvert.DeserializeObject<DBpic>(s);
                    MyData.Progress += 100.0 / MaxValue;
                    MyData.MyPicture.Add(new Picture
                    {
                        Name = f,
                        Emotions = GITT.StringToTuple(pic.Emotions),
                        Number = count
                    });
                }
                catch (OperationCanceledException ex)
                {
                    MessageBox.Show("Calculation Canceled");
                    break;
                }
            }

            Emot_Combobox.IsEnabled = true;
            Folder_Button.IsEnabled = true;
            Clear_Button.IsEnabled = true;

        }

        private void Button_Clear(object sender, RoutedEventArgs e)
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

        private void Open_Database(object sender, RoutedEventArgs e)
        {
            Database windowDatabase = new Database();
            windowDatabase.ShowDialog();
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
    }
}
