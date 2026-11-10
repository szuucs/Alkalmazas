using Alkalmazas.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;

namespace Alkalmazas
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddOpenApi();

            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseMySQL(
                    builder.Configuration.GetConnectionString("DefaultConnection")!
                ));

            var app = builder.Build();

            var imageFolder = Path.Combine(app.Environment.ContentRootPath, "App_Data", "Images");
            using (var scope = app.Services.CreateScope())
                SeedData.Initialize(scope.ServiceProvider.GetRequiredService<AppDbContext>(), imageFolder);

            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(imageFolder),
                RequestPath = "/images"
            });

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
