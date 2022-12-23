using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Contracts;
using System.Net.Http;
using Newtonsoft.Json;
using Polly.Retry;
using Polly;

namespace ImgClient
{
    /// <summary>
    /// Логика взаимодействия для Database.xaml
    /// </summary>
    public partial class Database : Window
    {
        private ICommand _deleteCommand;
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly string url = "https://localhost:5000/images";
        private const int MaxRetries = 3;
        public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(parameter =>
        {
            _retryPolicy.ExecuteAsync(async () =>
            {
                HttpClient client = new HttpClient();
                var taskGetAllImages = await client.GetAsync(url);

                int del_id = Collection[ImagesCollectionListBox.SelectedIndex].Id;
                var result = client.DeleteAsync($"{url}/{del_id}").Result;
                if(result.IsSuccessStatusCode)
                {
                    var q = Collection.Where(x => x.Id == del_id).FirstOrDefault();
                    Collection.Remove(q);
                }
            });
        }));
        public ObservableCollection<DBpic> Collection { get; private set; }
        public Database()
        {
            _retryPolicy = Policy.Handle<HttpRequestException>().WaitAndRetryAsync(MaxRetries, times =>
                   TimeSpan.FromMilliseconds(Math.Exp(times) * 250));
            Collection = new ObservableCollection<DBpic>();

            GetAllImages();
            InitializeComponent();
            DataContext = this;
        }

        public async void GetAllImages()
        {
            await _retryPolicy.ExecuteAsync(async () =>
            {
                HttpClient client = new HttpClient();
                var taskGetAllImages = await client.GetAsync(url);
                var id_list = JsonConvert.DeserializeObject<List<int>>(taskGetAllImages.Content.ReadAsStringAsync().Result);

                for (int i = 0; i < id_list.Count; i++)
                {
                    var taskGetImage = await client.GetAsync($"{url}/{id_list[i]}");
                    var image = JsonConvert.DeserializeObject<DBpic>(taskGetImage.Content.ReadAsStringAsync().Result);
                    Collection.Add(image);
                }
            });
        }
    }
}
