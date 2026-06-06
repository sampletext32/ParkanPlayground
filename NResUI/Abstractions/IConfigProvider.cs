namespace NResUI.Abstractions;

public interface IConfigProvider
{
    ConfigJson GetConfig();
    void Set(ConfigJson config);
}

public class ConfigProvider : IConfigProvider
{
    private ConfigJson _instance;

    public ConfigProvider(ConfigJson instance)
    {
        _instance = instance;
    }

    public void Set(ConfigJson config)
    {
        _instance = config;
    }

    public ConfigJson GetConfig()
    {
        return _instance;
    }
}
