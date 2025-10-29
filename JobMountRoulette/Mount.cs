namespace JobMountRoulette;

using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using FFXIVClientStructs.Interop;
using Lumina.Text.ReadOnly;

public sealed class Mount(Lumina.Excel.Sheets.Mount rawMount, ITextureProvider textureProvider)
{
    private readonly ITextureProvider mTextureProvider = textureProvider;
    
    public uint ID { get; init; } = rawMount.RowId;
    public uint IconID { get; init; } = rawMount.Icon;
    public ReadOnlySeString Name { get; } = rawMount.Singular;

    public ImTextureID GetIcon()
    {
        return mTextureProvider.GetFromGameIcon(new GameIconLookup(IconID)).GetWrapOrEmpty().Handle;
    }

    public unsafe bool IsAvailable(Pointer<ActionManager> actionManager)
    {
        return actionManager.Value->GetActionStatus(ActionType.Mount, ID) == 0;
    }

    public unsafe bool isUnlocked()
    {
        return PlayerState.Instance()->IsMountUnlocked(ID);
    }
}
