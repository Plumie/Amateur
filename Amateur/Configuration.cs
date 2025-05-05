using Dalamud.Configuration;
using System;

namespace Amateur;

[Serializable]
public class Configuration : IPluginConfiguration
{
    public int Version { get; set; } = 0;

    public bool Enabled { get; set; } = true;

    // the below exist just to make saving less cumbersome
    public void Save()
    {
        Amateur.PluginInterface.SavePluginConfig(this);
    }
}
