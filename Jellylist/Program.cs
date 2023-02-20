// ReSharper disable StringLiteralTypo

using System.Text.RegularExpressions;

namespace Jellylist
{
    public abstract class Program
    {
        public static string PublicUrl = "";

        public static void Main(string[] args)
        {
            const string urlPattern = @"https?:\/\/(www\.)?[-a-zA-Z0-9@:%._\+~#=]{1,256}\.[a-zA-Z0-9()]{1,6}\b([-a-zA-Z0-9()@:%_\+.~#?&\/\/=]*)"; // the language of gods
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
                    int matches = 0;
                    string completeUrl = "";
                    try
                    {
                        foreach (Match m in Regex.Matches(args[currentArg + 1], urlPattern, RegexOptions.Multiline))
                        {
                            matches++;
                            if (matches == 1)
                            {
                                completeUrl = m.Value;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("An error occurred reading arguments. Make sure you entered the URL correctly. Full error: ");
                        throw;
                    }
                    
                    switch (matches)
                    {
                        case 0:
                            Console.WriteLine("No URL detected. Please try again.");
                            Environment.Exit(1);
                            break;
                        case 1:
                            Console.WriteLine($"Using URL {completeUrl}.");
                            PublicUrl = completeUrl;
                            break;
                        case >= 2:
                            Console.WriteLine($"Multiple URLs detected. Only 1 is supported. Using first URL ({completeUrl}).");
                            PublicUrl = completeUrl;
                            break;
                    }
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
