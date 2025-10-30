using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JobMountRoulette.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
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
    private float mWidth;
    private bool mShowSelectedMountsOnly = false;
    private string mMountSearch = string.Empty;

    public MainWindow(PluginConfiguration configuration, IDalamudPluginInterface pluginInterface, IClientState clientState, ITextureProvider textureProvider, MountInventory mountInventory)
        : base("Job Mount Roulette##UwU", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        mConfiguration = configuration;
        mPluginInterface = pluginInterface;
        mClientState = clientState;
        mTextureProvider = textureProvider;
        mMountInventory = mountInventory;
        mMountTable = new MountTable(mTextureProvider);
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.SetNextWindowSizeConstraints(new Vector2(mWidth, 0), new Vector2(float.MaxValue, float.MaxValue));
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
            ImGui.Separator();
            var mounts = RenderMountFiltering(jobConfiguration);
            ImGui.Separator();
            mMountTable.Render(mounts, jobConfiguration);
        }

        mWidth = ImGui.GetWindowWidth();
    }

    private List<Mount> RenderMountFiltering(JobConfiguration jobConfiguration)
    {
        var mounts = mMountInventory.GetUnlockedMounts();

        if (ImGui.CollapsingHeader("Filter"))
        {
            ImGui.Checkbox("Show only selected mounts", ref mShowSelectedMountsOnly);

            ImGui.InputTextWithHint(string.Empty, "Filter by name", ref mMountSearch, 64);
            if (!string.IsNullOrWhiteSpace(mMountSearch))
            {
                mounts = mounts.Where(m => m.Name.ToString().IndexOf(mMountSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            mounts = mShowSelectedMountsOnly
                ? mounts.Where(m => jobConfiguration.IsMountEnabled(m.ID)).ToList()
                : mounts;
        }

        return mounts;
    }

    public override void OnClose()
    {
        mPluginInterface.SavePluginConfig(mConfiguration);
    }
}
