using System;
using System.Collections.Generic;
using Dalamud.Game;
using Dalamud.Hooking;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Dalamud.Game.ClientState.Objects;
using Amateur.Windows;
using Action = Lumina.Excel.Sheets.Action;
using Dalamud.Game.ClientState.Objects.Types;

namespace Amateur;

public unsafe class Amateur : IDalamudPlugin
{
    private const string CommandName = "/amateur";
    private const string CastBarSignature = "E8 ?? ?? ?? ?? 4C 8D 8F ?? ?? ?? ?? 4D 8B C6 48 8B D5";

    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static ISigScanner SigScanner { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;

    private readonly WindowSystem _windowSystem = new("Amateur");
    private readonly ConfigWindow _configWindow;

    public Configuration Configuration { get; init; }

    private delegate void SetCastBarDelegate(IntPtr thisPtr, IntPtr a2, IntPtr a3, IntPtr a4, char a5);
    private readonly Hook<SetCastBarDelegate> _setCastBarHook;

    private readonly Dictionary<uint, string> _actionNames = new();

    public Amateur()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _configWindow = new ConfigWindow(this);
        _windowSystem.AddWindow(_configWindow);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        var funcPtr = SigScanner.ScanText(CastBarSignature);
        _setCastBarHook = GameInteropProvider.HookFromAddress<SetCastBarDelegate>(funcPtr, OnSetCastBar);
        _setCastBarHook.Enable();

        PopulateActionCache();
    }

    private void PopulateActionCache()
    {
        var actionSheet = DataManager.GetExcelSheet<Action>();
        if (actionSheet == null)
        {
            throw new InvalidOperationException("Failed to load Action sheet.");
        }

        foreach (var action in actionSheet)
        {
            var actionName = action.Name.ToString();
            if (string.IsNullOrWhiteSpace(actionName))
                continue;

            _actionNames[action.RowId] = actionName;
        }
    }

    private void OnSetCastBar(nint thisPtr, nint a2, nint a3, nint a4, char a5)
    {
        try
        {
            if (TargetManager.Target is IBattleChara target)
            {
                var actionId = target.CastActionId;

                if (_actionNames.TryGetValue(actionId, out var actionName) && !string.IsNullOrWhiteSpace(actionName))
                {
                    Log.Debug($"Casting: {actionName}");
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error($"Exception in SetCastBar detour: {ex}");
        }

        _setCastBarHook.Original(thisPtr, a2, a3, a4, a5);
    }

    public void ToggleConfigUI() => _configWindow.Toggle();

    private void DrawUI() => _windowSystem.Draw();

    public void Dispose()
    {
        _windowSystem.RemoveAllWindows();
        _configWindow.Dispose();
        CommandManager.RemoveHandler(CommandName);

        _setCastBarHook?.Disable();
        _setCastBarHook?.Dispose();
    }
}
