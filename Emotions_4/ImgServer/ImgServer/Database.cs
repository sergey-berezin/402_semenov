using Contracts;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System.Text.Json;

namespace ImgServer
{
    public class DB : DbContext
    {
        public DbSet<DBpic> Pic { get; set; }

        public DB()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder o)
        {
            o.UseSqlite($"Data Source=emotions.db");
        }
    }

    public interface I_Db
    {
        Task<List<int>> GetAllImages();
        Task<string> PostImage(string path, CancellationToken token);
        Task<DBpic> TryGetImageById(int id);
        Task<int> TryDeleteImage(int id);
    }

    public class ImagesDatabase : I_Db
    {
        public async Task<List<int>> GetAllImages()
        {
            var result = new List<int>();
            try
            {
                using (var db = new DB())
                {
                    var query = db.Pic;
                    foreach (var image in query)
                    {
                        result.Add(image.Id);
                    }
                    return result;
                }
            }
            catch (Exception ex)
            {
                return result;
            }
        }

        public async Task<DBpic> TryGetImageById(int id)
        {
            using (var db = new DB())
            {
                var q = db.Pic.Where(x => x.Id == id).FirstOrDefault();
                if(q is not null)
                    return q;
                else 
                    return null;
            }
        }

        public async Task<int> TryDeleteImage(int id)
        {
            using (var db = new DB())
            {
                var q = db.Pic.Where(x => x.Id == id).FirstOrDefault();
                if (q is not null)
                {
                    db.Pic.Remove(q);
                    db.SaveChanges();
                    return 0;
                }    
                else
                    return -1; 
            }
        }

        public async Task<string> PostImage(string f, CancellationToken token)
        {
            using (var db = new DB())
            {
                string hash = DBpic.GetHash(System.IO.File.ReadAllBytes(f));
                var q = db.Pic.Where(x => x.Hash == hash).FirstOrDefault();
                if ((q is not null) && GITT.FileCompare(f, q.Name))
                {
                    string json = JsonSerializer.Serialize(q);
                    return json; //если изображение есть в базе
                }
                else
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
                        //count++;
                        var res = await a.Emotions(inputs, token);
                        r = res.OrderByDescending(t => t.Item2).ToList();
                    }
                    catch (OperationCanceledException ex) { return null; }
                    string _e = "";
                    foreach (var _ in r)
                        _e += _.Item1 + ": " + _.Item2.ToString() + "\n";
                    DBpic p = new DBpic
                    {
                        Name = f,
                        Emotions = _e,
                        Img = System.IO.File.ReadAllBytes(f),
                        Hash = hash
                    };
                    db.Add(p);
                    db.SaveChanges();
                    string json = JsonSerializer.Serialize(p);
                    return json;
                }
            }
        }
    }
}
