using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Emotions_2
{
    public class MyDataClass : INotifyPropertyChanged
    {
        private int maxValue = 100;
        public int MaxValue
        {
            get => maxValue;
            set
            {
                maxValue = value;
                OnPropertyChanged("MaxValue");
            }
        }
        private double progress = 0;
        public double Progress
        {
            get => progress;
            set
            {
                progress = value;
                OnPropertyChanged("Progress");
            }
        }
        private string folderName;
        public string FolderName
        {
            get => folderName;
            set
            {
                folderName = value;
                OnPropertyChanged("FolderName");
            }
        }
        private EmotList emotion = new();
        public EmotList EmotList
        {
            get => emotion;
            set
            {
                emotion = value;
                OnPropertyChanged("EmotList");
            }
        }
        public ObservableCollection<Picture> MyPicture { get; set; } = new ObservableCollection<Picture>();
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
