using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Plugin;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin.Services;
using JobMountRoulette.Configuration;
using JobMountRoulette.GUI;

namespace JobMountRoulette;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ITextureProvider TextureProvider { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IGameInteropProvider GameInteropProvider { get; private set; } = null!;
    [PluginService] internal static IPlayerState PlayerState { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;

    private const string CommandName = "/jmr";

    public PluginConfiguration PluginConfiguration { get; init; }

    public readonly WindowSystem WindowSystem = new("JobMountRoulette");
    private MainWindow MainWindow { get; init; }
    private MountInventory MountInventory { get; init; }
    private JobInventory JobInventory { get; init; }
    private RouletteHook RouletteHook{ get; init; }

    public Plugin()
    {
        MountInventory = new MountInventory(DataManager, TextureProvider);
        JobInventory = new JobInventory(DataManager, TextureProvider);

        PluginConfiguration = PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();

        RouletteHook = new RouletteHook(PluginConfiguration, PlayerState, ObjectTable, GameInteropProvider);
        MainWindow = new MainWindow(PluginConfiguration, PluginInterface, PlayerState, ObjectTable, TextureProvider, MountInventory, JobInventory);

        WindowSystem.AddWindow(MainWindow);

        CommandManager.AddHandler(CommandName, new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the configuration window and personalize the mount roulette for your current job."
        });

        // Tell the UI system that we want our windows to be drawn throught he window system
        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;

        // Adds another button doing the same but for the main ui of the plugin
        PluginInterface.UiBuilder.OpenMainUi += ToggleMainUi;
    }

    public void Dispose()
    {
        PluginInterface.SavePluginConfig(PluginConfiguration);

        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenMainUi -= ToggleMainUi;
        
        WindowSystem.RemoveAllWindows();

        RouletteHook.Dispose();

        CommandManager.RemoveHandler(CommandName);
    }

    private void OnCommand(string command, string args)
    {
        // In response to the slash command, toggle the display status of our main ui
        MainWindow.Toggle();
    }
    
    public void ToggleMainUi() => MainWindow.Toggle();
}
