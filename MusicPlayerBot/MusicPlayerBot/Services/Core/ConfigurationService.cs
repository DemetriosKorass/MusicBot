using Microsoft.Extensions.Configuration;
using MusicPlayerBot.Services.Interfaces;

namespace MusicPlayerBot.Services.Core;
public class ConfigurationService : IConfigurationService
{
    private readonly IConfiguration _conf;
    public ConfigurationService()
    {
        _conf = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
    }

    public string Token => _conf["Token"] ?? string.Empty;
    public string FfmpegPath => _conf["FFmpegPath"] ?? string.Empty;
}
