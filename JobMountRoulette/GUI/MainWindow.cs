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

namespace JobMountRoulette.GUI;

public class MainWindow : Window
{
    private readonly PluginConfiguration mConfiguration;
    private readonly IDalamudPluginInterface mPluginInterface;
    private readonly IObjectTable mObjectTable;
    private readonly IPlayerState mPlayerState;
    private readonly ITextureProvider mTextureProvider;
    private readonly MountInventory mMountInventory;
    private readonly JobInventory mJobInventory;
    private readonly MountTable mMountTable;
    private float mWidth;
    private bool mShowSelectedOnly = false;
    private bool mShowMultiseatOnly = false;
    private string mMountSearch = string.Empty;

    private RowRef<ClassJob>? mJobClipboard;
    private JobConfiguration? mJobConfigurationClipboard;

    public MainWindow(PluginConfiguration configuration, IDalamudPluginInterface pluginInterface, IPlayerState playerState, IObjectTable objectTable, ITextureProvider textureProvider, MountInventory mountInventory, JobInventory jobInventory)
        : base("Job Mount Roulette##UwU", ImGuiWindowFlags.NoScrollbar | ImGuiWindowFlags.NoScrollWithMouse | ImGuiWindowFlags.AlwaysAutoResize)
    {
        mConfiguration = configuration;
        mPluginInterface = pluginInterface;
        mPlayerState = playerState;
        mObjectTable = objectTable;
        mTextureProvider = textureProvider;
        mMountInventory = mountInventory;
        mJobInventory = jobInventory;
        mMountTable = new MountTable(mTextureProvider, mJobInventory);
    }

    public override void PreDraw()
    {
        base.PreDraw();
        ImGui.SetNextWindowSizeConstraints(new Vector2(mWidth, 0), new Vector2(float.MaxValue, float.MaxValue));
    }

    public override void Draw()
    {
        var player = mObjectTable.LocalPlayer;
        if (player == null)
        {
            ImGui.Text("Player not available.");
            ImGui.Text("Please log in and enter the game world to configure job-specific settings.");
            return;
        }

        var currentJob = player.ClassJob;
        var job = mJobInventory.Find(currentJob.Value.RowId);
        if (job == null)
        {
            ImGui.Text($"This job (ID {currentJob.Value.RowId}) is not recognized by the plugin.");
            ImGui.Text($"It may be unsupported or job data hasn't loaded yet.");
            ImGui.Text($"Try switching to another job or restarting the plugin.");
            return;
        }

        var characterConfiguration = mConfiguration.forCharacter(mPlayerState.ContentId);
        var jobConfiguration = characterConfiguration.forJob(job.ID);

        ImGui.Text($"Current Job: {job.Name}");

        ImGui.SameLine();
        if (ImGui.Button("Copy"))
        {
            mJobClipboard = currentJob;

            var clone = new JobConfiguration
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
                var clone = new JobConfiguration
                {
                    UseCustomRoulette = mJobConfigurationClipboard.UseCustomRoulette,
                    CustomRouletteMounts = [.. mJobConfigurationClipboard.CustomRouletteMounts]
                };

                characterConfiguration.overrideJob(job.ID, clone);
            }
        }

        var useCustomRoulette = jobConfiguration.UseCustomRoulette;
        _ = ImGui.Checkbox("Enable custom mount roulette for this job"u8, ref useCustomRoulette);
        jobConfiguration.UseCustomRoulette = useCustomRoulette;

        if (useCustomRoulette)
        {
            ImGui.Separator();
            var mounts = RenderMountFiltering(jobConfiguration);
            RenderBatchOperations(mounts, jobConfiguration);
            ImGui.Separator();
            mMountTable.Render(mounts, characterConfiguration, jobConfiguration);
        }

        mWidth = ImGui.GetWindowWidth();
    }

    private static void RenderBatchOperations(List<Mount> mounts, JobConfiguration jobConfiguration)
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
                mounts = [.. mounts.Where(m => m.Name.ToString().Contains(mMountSearch, StringComparison.OrdinalIgnoreCase))];
            }

            mounts = mShowSelectedOnly
                ? [.. mounts.Where(m => jobConfiguration.IsMountEnabled(m.ID))]
                : mounts;

            mounts = mShowMultiseatOnly
                ? [.. mounts.Where(m => m.ExtraSeats > 0)]
                : mounts;
        }

        return mounts;
    }

    public override void OnClose()
    {
        mPluginInterface.SavePluginConfig(mConfiguration);
    }
}
