using Microsoft.EntityFrameworkCore;
using Microsoft.ML.OnnxRuntime.Tensors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Contracts
{
    public class Picture : INotifyPropertyChanged
    {
        private int number;
        public int Number
        {
            get => number;
            set
            {
                number = value;
                OnPropertyChanged("Number");
            }
        }
        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }
        public List<Tuple<string, float>> Emotions { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class DBpic
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Hash { get; set; }
        public byte[] Img { get; set; }
        public string Emotions { get; set; }

        public static string GetHash(byte[] data)
        {
            using (var sha256 = SHA256.Create())
            {
                return string.Concat(sha256.ComputeHash(data).Select(x => x.ToString("X2")));
            }
        }
    }

    public class DB : DbContext
    {
        public DbSet<DBpic> Pic { get; set; }
        private string path = "..//..//..//..//emotions.db";

        public DB(string p = "..//..//..//..//emotions.db")
        {
            path = p;
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite($"Data Source={path}");
        }
    }


    public class GITT
    {
        public static DenseTensor<float> GrayscaleImageToTensor(Image<Rgb24> img)
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

        public static List<Tuple<string, float>> StringToTuple(string s)
        {
            List<Tuple<string, float>> r = new();
            List<string> str = s.Split('\n').ToList();
            str.RemoveAt(str.Count - 1);
            foreach (var v in str)
            {
                r.Add(Tuple.Create(v.Split(":")[0], float.Parse(v.Split(" ")[1])));
            }
            return r;
        }
        public static bool FileCompare(string file1, string file2)
        {
            int file1byte;
            int file2byte;
            FileStream fs1;
            FileStream fs2;


            if (file1 == file2)
            {
                return true;
            }

            fs1 = new FileStream(file1, FileMode.Open);
            fs2 = new FileStream(file2, FileMode.Open);

            if (fs1.Length != fs2.Length)
            {
                fs1.Close();
                fs2.Close();

                return false;
            }

            do
            {
                file1byte = fs1.ReadByte();
                file2byte = fs2.ReadByte();
            }
            while ((file1byte == file2byte) && (file1byte != -1));

            fs1.Close();
            fs2.Close();

            return ((file1byte - file2byte) == 0);
        }
    }
}
