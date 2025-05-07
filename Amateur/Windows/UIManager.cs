using System;
using Dalamud.Game.Command;
using Dalamud.Interface.Windowing;

namespace Amateur;

public class UIManager : IDisposable
{
    public readonly WindowSystem WindowSystem = new("Amateur");
    private ConfigWindow ConfigWindow { get; init; }

    public UIManager()
    {
        ConfigWindow = new ConfigWindow();
        WindowSystem.AddWindow(ConfigWindow);

        Service.CommandManager.AddHandler("/amateur", new CommandInfo(OnConfigCommand)
        {
            HelpMessage = "Open Amateur config"   
        });

        Service.PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        Service.PluginInterface.UiBuilder.OpenConfigUi += ConfigWindow.Toggle;
    }

    private void OnConfigCommand(string command, string args)
    {
        ConfigWindow.Toggle();
    }

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
    }
}
