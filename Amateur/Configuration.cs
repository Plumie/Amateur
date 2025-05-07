using Dalamud.Configuration;
using Dalamud.Game;
using System;

namespace Amateur;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public ClientLanguage Language { get; set; } = ClientLanguage.English;

    public void Save()
    {
        Service.PluginInterface.SavePluginConfig(this);
    }
}
