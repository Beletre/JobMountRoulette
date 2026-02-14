using Dalamud.Bindings.ImGui;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using JobMountRoulette.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace JobMountRoulette.Windows;

internal sealed class JobInfo
{
    public uint Id { get; set; }
    public string Name { get; set; } = string.Empty;
    // Placeholder for icon, in real use would be ImTextureID or similar
}

internal sealed class MountTable(ITextureProvider textureProvider)
{
    private const int COLUMNS = 5;
    private const int ROWS = 6;
    private const int PAGE_SIZE = COLUMNS * ROWS;
    private const float BUTTON_SIZE = 60f;
    private const float OVERLAY_SIZE = 24f;
    private const float OVERLAY_OFFSET = 4f;
    private int mMountPage = 1;

    private readonly ITextureProvider mTextureProvider = textureProvider;

    private void RenderNavigationBar(List<Mount> mounts)
    {
        var pageCount = GetPageCount(mounts.Count);

        if (mMountPage > pageCount)
            mMountPage = 1;

        ImGui.Text($"Pages: ");

        // Render page number buttons
        for (var page = 1; page <= pageCount; page++)
        {
            ImGui.SameLine();

            if (page == mMountPage)
            {
                ImGui.BeginDisabled();
                ImGui.Button(page.ToString());
                ImGui.EndDisabled();
            }
            else if (ImGui.Button(page.ToString()))
            {
                mMountPage = page;
            }
        }
    }

    private void RenderCurrentPage(List<Mount> mounts, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration, JobInventory jobInventory)
    {
        if (!ImGui.BeginTable("MountTable", 5))
            return;

        var i = 0;
        foreach (var mount in mounts.Skip((mMountPage - 1) * PAGE_SIZE).Take(PAGE_SIZE))
        {
            if (i++ > 0)
            {
                ImGui.SameLine();
            }

            if (i >= COLUMNS)
            {
                i = 0;
            }

            RenderMount(mount, characterConfiguration, jobConfiguration, jobInventory);
        }

        ImGui.EndTable();

        // Help text below the table explaining click behavior (dimmed)
        ImGui.Separator();
        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.75f, 0.75f, 0.75f, 1f));
        ImGui.TextWrapped("Left-click: toggle mount for the current job.");
        ImGui.TextWrapped("Right-click: open popup to view/change which jobs have this mount enabled.");
        ImGui.PopStyleColor();
    }

    public void Render(List<Mount> mounts, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration, JobInventory jobInventory)
    {
        RenderNavigationBar(mounts);
        RenderCurrentPage(mounts, characterConfiguration, jobConfiguration, jobInventory);
    }

    private static int GetPageCount(int mountCount)
    {
        return (mountCount / PAGE_SIZE) + (mountCount % PAGE_SIZE == 0 ? 0 : 1);
    }

    private void RenderMount(Mount mount, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration, JobInventory jobInventory)
    {
        var selectedUnselectedIcon = mTextureProvider.GetFromGame($"ui/uld/readycheck_hr1.tex").GetWrapOrEmpty().Handle;

        var mountIcon = mount.GetIcon();

        _ = ImGui.TableNextColumn();

        var originalPos = ImGui.GetCursorPos();

        var buttonSize = new Vector2(BUTTON_SIZE);
        var overlaySize = new Vector2(OVERLAY_SIZE);

        ImGui.PushStyleColor(ImGuiCol.Button, 0);
        ImGui.PushStyleColor(ImGuiCol.ButtonActive, 0);
        ImGui.PushStyleColor(ImGuiCol.ButtonHovered, 0);

        var mountClicked = ImGui.ImageButton(mountIcon, buttonSize, Vector2.Zero, Vector2.One, 0);
        var mountRightClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
        if (mountClicked)
        {
            jobConfiguration.ToggleMount(mount.ID);
        }

        ImGui.PopStyleColor(3);

        if (ImGui.IsItemHovered())
        {
            ImGui.SetTooltip(mount.Name.ToString());
        }

        var finalPos = ImGui.GetCursorPos();

        var overlayPos = originalPos + new Vector2(buttonSize.X - overlaySize.X + OVERLAY_OFFSET, 0);
        ImGui.SetCursorPos(overlayPos);

        var isEnabled = jobConfiguration.IsMountEnabled(mount.ID);
        var offset = new Vector2(isEnabled ? 0.1f : 0.6f, 0.2f);
        var offset2 = new Vector2(isEnabled ? 0.4f : 0.9f, 0.8f);
        ImGui.Image(selectedUnselectedIcon, overlaySize, offset, offset2);

        ImGui.SetCursorPos(finalPos);

        // --- Job assignment Popup (ausgelagert) ---
        RenderJobAssignmentPopup(mount, characterConfiguration, jobInventory, mountRightClicked);
    }

    private static void RenderJobAssignmentPopup(Mount mount, CharacterConfiguration characterConfiguration, JobInventory jobInventory, bool openRequested)
    {
        var popupId = $"##popup_{mount.ID}";

        if (openRequested)
        {
            ImGui.OpenPopup(popupId);
        }

        if (!ImGui.BeginPopup(popupId))
            return;

        ImGui.Text($"Jobs for: {mount.Name}");
        ImGui.Separator();

        foreach (JobInventory.JobType jobType in System.Enum.GetValues(typeof(JobInventory.JobType)))
        {
            var jobs = jobInventory.GetJobsByType(jobType);
            if (jobs.Count == 0)
                continue;

            ImGui.BeginGroup();
            var first = true;
            foreach (var job in jobs)
            {
                if (!first)
                    ImGui.SameLine();
                first = false;
                var perJobConfig = characterConfiguration.forJob(job.ID);
                var enabled = perJobConfig.IsMountEnabled(mount.ID);
                ImGui.PushStyleColor(ImGuiCol.Button, Vector4.Zero);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, Vector4.Zero);
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, Vector4.Zero);
                var tint_col = enabled ? Vector4.One : new Vector4(0.4f, 0.4f, 0.4f, 1f);
                var btnSize = new Vector2(32, 32);
                if (ImGui.ImageButton(job.GetIcon(), btnSize, Vector2.Zero, Vector2.One, 0, Vector4.Zero, tint_col))
                {
                    perJobConfig.ToggleMount(mount.ID);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(job.Name);
                }
                ImGui.PopStyleColor(3);
            }
            ImGui.EndGroup();
            ImGui.Spacing();
        }

        ImGui.Separator();

        ImGui.PushStyleColor(ImGuiCol.Text, new Vector4(0.75f, 0.75f, 0.75f, 1f));
        ImGui.Text("Click an icon to toggle. Highlighted = Enabled.");
        ImGui.PopStyleColor();

        ImGui.EndPopup();
    }
}
