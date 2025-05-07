using System;

namespace Amateur;

public class HookManager: IDisposable {
    public CastBarHook CastBarHook { get; set; }

    public HookManager()
    {
        CastBarHook = new CastBarHook();
    }

    public void Dispose()
    {
        CastBarHook.Dispose();
    }
}
