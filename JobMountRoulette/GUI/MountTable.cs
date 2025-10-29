namespace JobMountRoulette;

using Dalamud.Bindings.ImGui;
using Dalamud.Plugin.Services;
using SamplePlugin.Configuration;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

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

    private void RenderCurrentPage(List<Mount> mounts, JobConfiguration jobConfiguration)
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

            RenderMount(mount, jobConfiguration);
        }

        ImGui.EndTable();
    }

    public void Render(List<Mount> mounts, JobConfiguration jobConfiguration)
    {
        RenderNavigationBar(mounts);
        RenderCurrentPage(mounts, jobConfiguration);
    }

    private static int GetPageCount(int mountCount)
    {
        return (mountCount / PAGE_SIZE) + (mountCount % PAGE_SIZE == 0 ? 0 : 1);
    }

    private void RenderMount(Mount mount, JobConfiguration jobConfiguration)
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

        if (ImGui.ImageButton(mountIcon, buttonSize, Vector2.Zero, Vector2.One, 0))
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
    }
}
