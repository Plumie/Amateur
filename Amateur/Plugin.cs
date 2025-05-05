using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Hooking;

using Amateur.Windows;
using System;
using Dalamud.Game;

namespace Amateur;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework {get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager {get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;

    private const string CommandName = "/amateur";

    public Configuration Configuration { get; init; }

    public readonly WindowSystem WindowSystem = new("Amateur");
    private ConfigWindow ConfigWindow { get; init; }

	private delegate void SetCastBarDelegate(IntPtr thisPtr, IntPtr a2, IntPtr a3, IntPtr a4, char a5);
	private readonly Hook<SetCastBarDelegate> _setCastBarHook = null!;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();
        ConfigWindow = new ConfigWindow(this);
        WindowSystem.AddWindow(ConfigWindow);
        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;
        var setCastBarFuncPtr = SigScanner.ScanText("E8 ?? ?? ?? ?? 4C 8D 8F ?? ?? ?? ?? 4D 8B C6 48 8B D5");
        _setCastBarHook = GameInteropProvider.HookFromAddress<SetCastBarDelegate>(setCastBarFuncPtr, SetCastBarDetour);

		_setCastBarHook.Enable();
        // Framework.Update += FrameworkOnUpdate;
    }

	private void SetCastBarDetour(nint thisPtr, nint a2, nint a3, nint a4, char a5)
	{
        Log.Debug("Casting" + thisPtr);
	}

    public void Dispose()
    {
        WindowSystem.RemoveAllWindows();
        ConfigWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);
        _setCastBarHook?.Disable();
        _setCastBarHook?.Dispose();
    }

    private void DrawUI() => WindowSystem.Draw();

    public void ToggleConfigUI() => ConfigWindow.Toggle();
}
