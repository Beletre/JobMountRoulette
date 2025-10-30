using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using JobMountRoulette.Configuration;
using Lumina.Excel;
using Lumina.Excel.Sheets;
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
    private bool mShowSelectedOnly = false;
    private bool mShowMultiseatOnly = false;
    private string mMountSearch = string.Empty;

    private RowRef<ClassJob>? mJobClipboard;
    private JobConfiguration? mJobConfigurationClipboard;

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
        var jobIdentifier = currentJob.Value.RowId;
        var jobName = currentJob.Value.NameEnglish;

        var characterConfiguration = mConfiguration.forCharacter(mClientState.LocalContentId);
        var jobConfiguration = characterConfiguration.forJob(jobIdentifier);

        ImGui.Text($"Current Job: {jobName}");

        ImGui.SameLine();
        if (ImGui.Button("Copy"))
        {
            mJobClipboard = currentJob;

            JobConfiguration clone = new JobConfiguration
            {
                UseCustomRoulette = jobConfiguration.UseCustomRoulette,
                CustomRouletteMounts = [.. jobConfiguration.CustomRouletteMounts]
            };

            mJobConfigurationClipboard = clone;
        }

        if (mJobClipboard.HasValue && mJobConfigurationClipboard != null)
        {
            ImGui.SameLine();

            if (ImGui.Button($"Paste from {mJobClipboard?.Value.NameEnglish}"))
            {
                JobConfiguration clone = new JobConfiguration
                {
                    UseCustomRoulette = mJobConfigurationClipboard.UseCustomRoulette,
                    CustomRouletteMounts = [.. mJobConfigurationClipboard.CustomRouletteMounts]
                };

                characterConfiguration.overrideJob(jobIdentifier, clone);
            }
        }

        var useCustomRoulette = jobConfiguration.UseCustomRoulette;
        _ = ImGui.Checkbox("Enable custom mount roulette for this job"u8, ref useCustomRoulette);
        jobConfiguration.UseCustomRoulette = useCustomRoulette;

        if (useCustomRoulette)
        {
            ImGui.Separator();
            var mounts = RenderMountFiltering(jobConfiguration);
            RenderBatchOperations(mounts, characterConfiguration, jobConfiguration);
            ImGui.Separator();
            mMountTable.Render(mounts, jobConfiguration);
        }

        mWidth = ImGui.GetWindowWidth();
    }

    private void RenderBatchOperations(List<Mount> mounts, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration)
    {
        if (ImGui.CollapsingHeader("Batch"))
        {
            if (ImGui.Button("Select all"))
            {
                foreach (var mount in mounts)
                {
                    jobConfiguration.SetMountEnabled(mount.ID, true);
                }
            }

            ImGui.SameLine();

            if (ImGui.Button("Deselect all"))
            {
                foreach (var mount in mounts)
                {
                    jobConfiguration.SetMountEnabled(mount.ID, false);
                }
            }
        }
    }

    private List<Mount> RenderMountFiltering(JobConfiguration jobConfiguration)
    {
        var mounts = mMountInventory.GetUnlockedMounts();

        if (ImGui.CollapsingHeader("Filter"))
        {
            ImGui.Checkbox("Selected only", ref mShowSelectedOnly);
            ImGui.Checkbox("Multiseat only", ref mShowMultiseatOnly);

            ImGui.InputTextWithHint(string.Empty, "Filter by name", ref mMountSearch, 64);
            if (!string.IsNullOrWhiteSpace(mMountSearch))
            {
                mounts = mounts.Where(m => m.Name.ToString().IndexOf(mMountSearch, StringComparison.OrdinalIgnoreCase) >= 0).ToList();
            }

            mounts = mShowSelectedOnly
                ? mounts.Where(m => jobConfiguration.IsMountEnabled(m.ID)).ToList()
                : mounts;

            mounts = mShowMultiseatOnly
                ? mounts.Where(m => m.ExtraSeats > 0).ToList()
                : mounts;
        }

        return mounts;
    }

    public override void OnClose()
    {
        mPluginInterface.SavePluginConfig(mConfiguration);
    }
}
