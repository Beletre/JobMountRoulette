using Dalamud.Bindings.ImGui;
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

    // Static job list for demonstration (add more jobs as needed)
    private static readonly List<JobInfo> JobList = new()
    {
        new JobInfo { Id = 19, Name = "PLD" },
        new JobInfo { Id = 21, Name = "WAR" },
        new JobInfo { Id = 24, Name = "WHM" },
        new JobInfo { Id = 25, Name = "SCH" },
        new JobInfo { Id = 27, Name = "DRG" },
        new JobInfo { Id = 30, Name = "BRD" },
        new JobInfo { Id = 31, Name = "MNK" },
        new JobInfo { Id = 32, Name = "SMN" },
        new JobInfo { Id = 34, Name = "BLM" },
        new JobInfo { Id = 35, Name = "ACN" },
        new JobInfo { Id = 37, Name = "NIN" },
        new JobInfo { Id = 38, Name = "MCH" },
        new JobInfo { Id = 39, Name = "DRK" },
        new JobInfo { Id = 40, Name = "AST" },
        new JobInfo { Id = 41, Name = "SAM" },
        new JobInfo { Id = 43, Name = "RDM" },
        new JobInfo { Id = 44, Name = "BLU" },
        // ... add more jobs as needed
    };

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

    private void RenderCurrentPage(List<Mount> mounts, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration)
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

            RenderMount(mount, characterConfiguration, jobConfiguration);
        }

        ImGui.EndTable();
    }

    public void Render(List<Mount> mounts, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration)
    {
        RenderNavigationBar(mounts);
        RenderCurrentPage(mounts, characterConfiguration, jobConfiguration);
    }

    private static int GetPageCount(int mountCount)
    {
        return (mountCount / PAGE_SIZE) + (mountCount % PAGE_SIZE == 0 ? 0 : 1);
    }

    private void RenderMount(Mount mount, CharacterConfiguration characterConfiguration, JobConfiguration jobConfiguration)
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

        bool mountClicked = ImGui.ImageButton(mountIcon, buttonSize, Vector2.Zero, Vector2.One, 0);
        bool mountRightClicked = ImGui.IsItemClicked(ImGuiMouseButton.Right);
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

        // --- Job Enable/Disable Popup ---
        string popupId = $"##jobpopup_{mount.ID}";
        if (mountRightClicked)
        {
            ImGui.OpenPopup(popupId);
        }

        if (ImGui.BeginPopup(popupId))
        {
            ImGui.Text($"Jobs for: {mount.Name}");
            ImGui.Separator();

            int columns = 8;
            int i = 0;
            ImGui.BeginTable($"##jobtable_{mount.ID}", columns, ImGuiTableFlags.None);
            foreach (var job in JobList)
            {
                ImGui.TableNextColumn();

                var perJobConfig = characterConfiguration.forJob(job.Id);
                bool enabled = perJobConfig.IsMountEnabled(mount.ID);
                // Placeholder: colored square for enabled, grey for disabled
                var color = enabled ? new Vector4(0.2f, 0.8f, 0.2f, 1f) : new Vector4(0.5f, 0.5f, 0.5f, 1f);
                ImGui.PushStyleColor(ImGuiCol.Button, color);
                ImGui.PushStyleColor(ImGuiCol.ButtonHovered, color * new Vector4(1.1f, 1.1f, 1.1f, 1f));
                ImGui.PushStyleColor(ImGuiCol.ButtonActive, color);
                var btnSize = new Vector2(32, 32);
                string btnId = $"##jobbtn_{mount.ID}_{job.Id}";
                if (ImGui.Button(btnId, btnSize))
                {
                    perJobConfig.ToggleMount(mount.ID);
                }
                if (ImGui.IsItemHovered())
                {
                    ImGui.SetTooltip(job.Name);
                }
                ImGui.PopStyleColor(3);
                i++;
            }
            ImGui.EndTable();
            ImGui.EndPopup();
        }
    }
}
