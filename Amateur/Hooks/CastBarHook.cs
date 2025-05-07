using System;
using System.Runtime.InteropServices;
using Dalamud.Game.ClientState.Objects.Types;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Hooking;

namespace Amateur;

public class CastBarHook : IDisposable
{
    private delegate void SetCastBarDelegate(IntPtr thisPtr, IntPtr a2, IntPtr a3, IntPtr a4, char a5);
    private readonly Hook<SetCastBarDelegate> _setCastBarHook;
    private const string CASTBAR_SIGNATURE = "E8 ?? ?? ?? ?? 0F B7 43 ?? 66 3B C5";
    private const int BUFFER_SIZE = 256;
    private IntPtr _customTextBuffer = IntPtr.Zero;

    public CastBarHook()
    {
        var funcPtr = Service.SigScanner.ScanText(CASTBAR_SIGNATURE);
        _setCastBarHook = Service.GameInteropProvider.HookFromAddress<SetCastBarDelegate>(funcPtr, OnSetCastBar);
        _setCastBarHook.Enable();
        _customTextBuffer = Marshal.AllocHGlobal(BUFFER_SIZE);
    }

    private unsafe void OnSetCastBar(nint thisPtr, nint a2, nint a3, nint a4, char a5)
    {
        try
        {
            if (Service.TargetManager.Target is not IBattleChara target || target.ObjectKind != ObjectKind.BattleNpc)
                goto Original;

            var actionId = target.CastActionId;
            var originalName = Utils.TranslateAction(actionId, Service.ClientState.ClientLanguage);
            var translatedName = Utils.TranslateAction(actionId, Amateur.Configuration.Language);

            if (originalName.TextValue == string.Empty || translatedName.TextValue == string.Empty)
                goto Original;

            string? currentText = Marshal.PtrToStringUTF8(a2);
            if (currentText == null || currentText != originalName.TextValue)
                goto Original;

            var encoded = translatedName.Encode();
            if (encoded.Length >= BUFFER_SIZE)
            {
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
            Service.Log.Error($"[SetCastBar] Exception: {ex}");
            _setCastBarHook.Original(thisPtr, a2, a3, a4, a5);
        }
    }


    public void Dispose()
    {
        _setCastBarHook.Disable();
        _setCastBarHook.Dispose();

        if (_customTextBuffer != IntPtr.Zero)
        {
            Marshal.FreeHGlobal(_customTextBuffer);
            _customTextBuffer = IntPtr.Zero;
        }
    }
}
