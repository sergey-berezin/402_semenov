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

namespace Emotions_2
{
    /// <summary>
    /// Логика взаимодействия для Database.xaml
    /// </summary>
    public partial class Database : Window
    {
        private ICommand _deleteCommand;
        public ICommand DeleteCommand => _deleteCommand ?? (_deleteCommand = new RelayCommand(parameter =>
        {
            var image = Collection[ImagesCollectionListBox.SelectedIndex];
            using (var db = new DB())
            {
                var deletedImage = db.Pic.Where(x => x.Id == image.Id).First();
                if (deletedImage == null)
                {
                    return;
                }
                db.Pic.Remove(deletedImage);
                db.SaveChanges();
                Collection.Remove(image);
            }
        }));
        public ObservableCollection<DBpic> Collection { get; private set; }
        public Database()
        {
            Collection = new ObservableCollection<DBpic>();

            using (var db = new DB())
            {
                foreach (var v in db.Pic)
                {
                    Collection.Add(v);
                }
            }

            InitializeComponent();
            DataContext = this;
        }

        private void Button_Drop(object sender, RoutedEventArgs e)
        {
            try
            {
                var image = Collection[ImagesCollectionListBox.SelectedIndex];
                using (var db = new DB())
                {
                    var deletedImage = db.Pic.Where(x => x.Id == image.Id).First();
                    if (deletedImage == null)
                    {
                        return;
                    }
                    db.Pic.Remove(deletedImage);
                    db.SaveChanges();
                    Collection.Remove(image);
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show(e1.Message);
            }
        }
    }
}
