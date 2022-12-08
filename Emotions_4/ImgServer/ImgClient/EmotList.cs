using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace ImgClient
{
    public enum Emot
    {
        neutral,
        happiness,
        surprise,
        sadness,
        anger,
        disgust,
        fear,
        contempt
    }
    public class List : INotifyPropertyChanged
    {
        public Emot e { get; set; }
        public string? f { get; set; }
        public event PropertyChangedEventHandler? PropertyChanged;
        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }

    public class EmotList : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;
        public List selectedEmot;
        public ObservableCollection<List> Emotion { get; set; }

        public EmotList()
        {
            Emotion = new ObservableCollection<List>
            {
                new List { e = Emot.neutral, f = "neutral" },
                new List { e = Emot.happiness, f = "happiness" },
                new List { e = Emot.surprise, f = "surprise" },
                new List { e = Emot.anger, f = "anger" },
                new List { e = Emot.sadness, f = "sadness" },
                new List { e = Emot.disgust, f = "disgust" },
                new List { e = Emot.fear, f = "fear" },
                new List { e = Emot.contempt, f = "contempt" }
            };
            //selectedEmot = Emotion[0];
        }
        public List SelectedEmot
        {
            get { return selectedEmot; }
            set
            {
                selectedEmot = value;
                OnPropertyChanged(nameof(SelectedEmot));
            }
        }

        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }
    }
}