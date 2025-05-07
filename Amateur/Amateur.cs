using Dalamud.Plugin;

namespace Amateur;


public sealed class Amateur : IDalamudPlugin
{
    public string Name => "Amateur";

    public static Configuration Configuration { get; private set; } = null!;

    public readonly HookManager HookManager;
    public readonly UIManager UIManager;

    public Amateur(
        IDalamudPluginInterface pluginInterface
    )
    {
        pluginInterface.Create<Service>();

        Configuration = pluginInterface.GetPluginConfig() as Configuration ?? new Configuration();

        UIManager = new UIManager();
        HookManager = new HookManager();
    }

    public void Dispose()
    {
        HookManager.Dispose();
    }
}
