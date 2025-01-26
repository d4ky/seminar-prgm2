using Microsoft.Extensions.Configuration;
using System.IO;

namespace Login
{
    public static class SecurityConfig
    {
        public static string Pepper { get; }

        static SecurityConfig()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            Pepper = config["SecuritySettings:Pepper"];
        }
    }
}
