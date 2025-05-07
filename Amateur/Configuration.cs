using Dalamud.Configuration;
using System;
using Dalamud.Game;

namespace Amateur;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public ClientLanguage Language { get; set; } = ClientLanguage.English;

    public void Save()
    {
        Amateur.PluginInterface.SavePluginConfig(this);
    }
}
