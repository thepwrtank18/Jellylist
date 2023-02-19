
// ReSharper disable StringLiteralTypo
namespace Jellylist
{
    public abstract class Program
    {
        public static string PublicUrl = "";

        public static void Main(string[] args)
        {
            int currentArg = -1;
            if (!args.Contains("--jellyfinUrl"))
            {
                Console.WriteLine("Please put --jellyfinUrl in the console, followed by the URL you use to access Jellyfin. (ex: http(s)://jellyfin.example.com/[subdir if applicable])");
                Environment.Exit(1);
            }
            foreach (string arg in args)
            {
                currentArg++;
                if (arg == "--jellyfinUrl")
                {
                    PublicUrl = args[currentArg + 1];
                }
            }

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
