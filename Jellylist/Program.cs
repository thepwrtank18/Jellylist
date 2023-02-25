// ReSharper disable StringLiteralTypo

using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace Jellylist
{
    public abstract partial class Program
    {
        [SuppressMessage("Usage", "CA2211:Non-constant fields should not be visible")] 
        public static string PublicUrl = "";
        public const string Version = "2.2";

        public static void Main(string[] args)
        {
            Console.WriteLine($"Jellylist - version {Version}");
            
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
                        foreach (Match m in UrlRegex().Matches(args[currentArg + 1]))
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

        [GeneratedRegex("https?:\\/\\/(www\\.)?[-a-zA-Z0-9@:%._\\+~#=]{1,256}\\.[a-zA-Z0-9()]{1,6}\\b([-a-zA-Z0-9()@:%_\\+.~#?&\\/\\/=]*)", RegexOptions.Multiline)] // the language of gods
        private static partial Regex UrlRegex();
    }
}
