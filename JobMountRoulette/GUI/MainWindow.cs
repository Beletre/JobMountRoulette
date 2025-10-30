using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JobMountRoulette.Configuration;
using System.Numerics;

namespace JobMountRoulette.Windows;

public class MainWindow : Window
{
    private readonly PluginConfiguration mConfiguration;
    private readonly IDalamudPluginInterface mPluginInterface;
    private readonly IClientState mClientState;
    private readonly ITextureProvider mTextureProvider;
    private readonly MountInventory mMountInventory;
    private readonly MountTable mMountTable;

    public MainWindow(PluginConfiguration configuration, IDalamudPluginInterface pluginInterface, IClientState clientState, ITextureProvider textureProvider, MountInventory mountInventory)
        : base("Job Mount Roulette##UwU", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse)
    {
        SizeConstraints = new WindowSizeConstraints
        {
            MinimumSize = new Vector2(375, 330),
            MaximumSize = new Vector2(float.MaxValue, float.MaxValue)
        };

        mConfiguration = configuration;
        mPluginInterface = pluginInterface;
        mClientState = clientState;
        mTextureProvider = textureProvider;
        mMountInventory = mountInventory;
        mMountTable = new MountTable(mTextureProvider);
    }

    public override void Draw()
    {
        var player = mClientState.LocalPlayer;
        if (player == null)
        {
            ImGui.Text("You ain't got no job... :,(");
            return;
        }

        var currentJob = player.ClassJob;
        var jobIdentifier = currentJob.Value.JobIndex;

        var characterConfiguration = mConfiguration.forCharacter(mClientState.LocalContentId);
        var jobConfiguration = characterConfiguration.forJob(jobIdentifier);

        ImGui.Text($"Current Job: {currentJob.Value.NameEnglish}");

        var useCustomRoulette = jobConfiguration.UseCustomRoulette;
        _ = ImGui.Checkbox("Enable custom mount roulette for this job"u8, ref useCustomRoulette);
        jobConfiguration.UseCustomRoulette = useCustomRoulette;

        if (useCustomRoulette)
        {
            mMountTable.Render(mMountInventory.GetUnlockedMounts(), jobConfiguration);
        }
    }

    public override void OnClose()
    {
        mPluginInterface.SavePluginConfig(mConfiguration);
    }
}
