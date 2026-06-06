using System.Text.Json;
using NResUI.Abstractions;
using NResUI.ImGuiUI;

namespace NResUI;

public class LoadConfigOnLaunch : ILaunchReceiver
{
    private readonly ILater _later;
    private readonly MessageBoxModalPanel _messageBoxModalPanel;
    private readonly IConfigProvider _configProvider;

    public LoadConfigOnLaunch(ILater later, MessageBoxModalPanel messageBoxModalPanel, IConfigProvider configProvider)
    {
        _later = later;
        _messageBoxModalPanel = messageBoxModalPanel;
        _configProvider = configProvider;
    }

    public void OnLaunch()
    {
        if (!File.Exists("config.json"))
        {
            _later.Execute(() =>
            {
                _messageBoxModalPanel.Show(
                    "config.json is not present\nViewport may be broken without game files\nPlaceholder file has been created");
            });
            var config = new ConfigJson() { GameBasePath = "" };
            File.WriteAllText("config.json", JsonSerializer.Serialize(config));
            _configProvider.Set(config);
        }
        else
        {
            var content = File.ReadAllText("config.json");
            var config = JsonSerializer.Deserialize<ConfigJson>(content);
            if (config is null)
            {
                _later.Execute(() =>
                {
                    _messageBoxModalPanel.Show("config.json was corrupted\nReplacement has been created");
                });
                config = new ConfigJson() { GameBasePath = "" };
                File.WriteAllText("config.json", JsonSerializer.Serialize(config));
                _configProvider.Set(config);
            }
            else
            {
                _configProvider.Set(config);
            }
        }
    }
}
