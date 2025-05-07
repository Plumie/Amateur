using System;
using System.Numerics;
using Dalamud.Game;
using Dalamud.Interface.Windowing;
using ImGuiNET;

namespace Amateur;

public class ConfigWindow : Window
{
    public ConfigWindow() : base("Amateur Configuration")
    {
        Flags = ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse | ImGuiWindowFlags.NoScrollbar |
                ImGuiWindowFlags.NoScrollWithMouse;

        Size = new Vector2(232, 90);
        SizeCondition = ImGuiCond.Always;
    }

    public override void PreDraw()
    {
        Flags &= ~ImGuiWindowFlags.NoMove;
    }

    public override void Draw()
    {
        var languageValues = Enum.GetValues<ClientLanguage>();
        var languageNames = Enum.GetNames<ClientLanguage>();
        var currentIndex = Array.IndexOf(languageValues, Amateur.Configuration.Language);

        if (ImGui.Combo("Language", ref currentIndex, languageNames, languageNames.Length))
        {
            Amateur.Configuration.Language = languageValues[currentIndex];
            Amateur.Configuration.Save();
        }

        ImGui.Spacing();
        ImGui.Separator();

        string sourceLang = Service.ClientState.ClientLanguage.ToString();
        string targetLang = Amateur.Configuration.Language.ToString();

        ImGui.TextColored(new System.Numerics.Vector4(1f, 1f, 0.6f, 1f), $"{sourceLang} → {targetLang}");
    }

    public void Dispose() {}
}
