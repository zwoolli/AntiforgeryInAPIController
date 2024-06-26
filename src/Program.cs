var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();


var app = builder.Build();
app.UseRouting();
app.MapRazorPages();
app.MapControllers();

app.Run();
public partial class Program { }
