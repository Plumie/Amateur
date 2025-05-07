using System;
using System.Runtime.InteropServices;
using Dalamud.Game;
using Dalamud.Game.ClientState.Objects;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Hooking;
using Dalamud.Interface.Windowing;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using Action = Lumina.Excel.Sheets.Action;

namespace Amateur;

public class Amateur : IDalamudPlugin
{
    public string Name => "Amateur";

    [PluginService] public static IDalamudPluginInterface PluginInterface { get; set; } = null!;
    [PluginService] private static ICommandManager CommandManager { get; set; } = null!;
    [PluginService] private static IClientState ClientState { get; set; } = null!;
    [PluginService] private static IDataManager DataManager { get; set; } = null!;
    [PluginService] private static IPluginLog Log { get; set; } = null!;
    [PluginService] private static IFramework Framework { get; set; } = null!;
    [PluginService] private static ITargetManager TargetManager { get; set; } = null!;
    [PluginService] private static ISigScanner SigScanner { get; set; } = null!;
    [PluginService] private static IGameInteropProvider GameInteropProvider { get; set; } = null!;

    public Configuration Configuration { get; }

    private readonly WindowSystem _windowSystem = new("Amateur");
    private readonly ConfigurationUI _configurationUI;

    private const string CASTBAR_SIGNATURE = "E8 ?? ?? ?? ?? 0F B7 43 ?? 66 3B C5";
    private delegate void SetCastBarDelegate(IntPtr thisPtr, IntPtr a2, IntPtr a3, IntPtr a4, char a5);
    private readonly Hook<SetCastBarDelegate> _setCastBarHook;

    private const int BUFFER_SIZE = 256;
    private IntPtr _customTextBuffer = IntPtr.Zero;

    public Amateur()
    {
        Configuration = PluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        _configurationUI = new ConfigurationUI(this);
        _windowSystem.AddWindow(_configurationUI);

        PluginInterface.UiBuilder.Draw += DrawUI;
        PluginInterface.UiBuilder.OpenConfigUi += ToggleConfigUI;

        _customTextBuffer = Marshal.AllocHGlobal(BUFFER_SIZE);

        var funcPtr = SigScanner.ScanText(CASTBAR_SIGNATURE);
        _setCastBarHook = GameInteropProvider.HookFromAddress<SetCastBarDelegate>(funcPtr, OnSetCastBar);
        _setCastBarHook.Enable();
    }

    private unsafe void OnSetCastBar(nint thisPtr, nint a2, nint a3, nint a4, char a5)
    {
        try
        {
            if (TargetManager.Target is not IBattleChara target || target.ObjectKind != ObjectKind.BattleNpc)
                goto Original;

            var actionId = target.CastActionId;
            var originalName = Translate(actionId, ClientState.ClientLanguage);
            var translatedName = Translate(actionId, Configuration.Language);

            if (originalName.TextValue == string.Empty || translatedName.TextValue == string.Empty)
                goto Original;

            string? currentText = Marshal.PtrToStringUTF8(a2);
            if (currentText == null || currentText != originalName.TextValue)
                goto Original;

            var encoded = translatedName.Encode();
            if (encoded.Length >= BUFFER_SIZE)
            {
                Log.Warning($"[SetCastBar] Translated name too long, truncating.");
                Array.Resize(ref encoded, BUFFER_SIZE - 1);
            }

            Marshal.Copy(encoded, 0, _customTextBuffer, encoded.Length);
            Marshal.WriteByte(_customTextBuffer, encoded.Length, 0);

            _setCastBarHook.Original(thisPtr, _customTextBuffer, a3, a4, a5);
            return;

        Original:
            _setCastBarHook.Original(thisPtr, a2, a3, a4, a5);
        }
        catch (Exception ex)
        {
            Log.Error($"[SetCastBar] Exception: {ex}");
            _setCastBarHook.Original(thisPtr, a2, a3, a4, a5);
        }
    }

    private static SeString Translate(uint actionId, ClientLanguage language)
    {
        var sheet = DataManager.GetExcelSheet<Action>(language);
        if (sheet == null)
        {
            Log.Error($"[Translate] Failed to load Action sheet for language: {language}");
            return SeString.Empty;
        }

        var action = sheet.GetRow(actionId);
        if ((object)action == null)
        {
            Log.Warning($"[Translate] Action ID {actionId} not found for language: {language}");
            return SeString.Empty;
        }

        return new SeString().Append(action.Name.ToString());
    }

    public void ToggleConfigUI() => _configurationUI.Toggle();

    private void DrawUI() => _windowSystem.Draw();

    public void Dispose()
    {
        _setCastBarHook.Disable();
        _setCastBarHook.Dispose();

        _windowSystem.RemoveAllWindows();
        _configurationUI.Dispose();

        if (_customTextBuffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_customTextBuffer);
            _customTextBuffer = IntPtr.Zero;
        }
    }
}
