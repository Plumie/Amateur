using System;
using System.Numerics;
using Dalamud.Interface.Windowing;
using Dalamud.Game;
using ImGuiNET;

namespace Amateur;

public class ConfigurationUI : Window, IDisposable
{
    private Configuration Configuration;

    public ConfigurationUI(Amateur amateur) : base("Amateur config")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;
        Configuration = amateur.Configuration;
    }

    public override void PreDraw()
    {
        Flags &= ~ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var languageValues = Enum.GetValues<ClientLanguage>();
        var languageNames = Enum.GetNames<ClientLanguage>();
        var currentIndex = Array.IndexOf(languageValues, Configuration.Language);

        if (ImGui.Combo("Language", ref currentIndex, languageNames, languageNames.Length))
        {
            Configuration.Language = languageValues[currentIndex];
            Configuration.Save();
        }
    }

    public void Dispose() { }
}
