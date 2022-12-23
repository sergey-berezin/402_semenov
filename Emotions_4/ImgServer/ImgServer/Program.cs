using Contracts;
using ImgServer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<I_Db>(new ImagesDatabase());
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllerRoute(
    name: "get",
    pattern: "{controller=images}/{action=get}");

app.MapControllerRoute(
    name: "getbyid",
    pattern: "{controller=images}/{action=index}/{id?}");

app.MapGet("/", () => "Hello World!");

app.Run();
