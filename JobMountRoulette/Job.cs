using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

namespace JobMountRoulette;

public class Job(Lumina.Excel.Sheets.ClassJob rawClassJob, ITextureProvider textureProvider)
{
    private readonly ITextureProvider mTextureProvider = textureProvider;

    public uint ID { get; init;  } = rawClassJob.RowId;
    public uint IconID { get; init; } = 62100 + rawClassJob.RowId;
    public string Name { get; init; } = rawClassJob.NameEnglish.ToString();

    public ImTextureID GetIcon()
    {
        return mTextureProvider.GetFromGameIcon(new GameIconLookup(IconID)).GetWrapOrEmpty().Handle;
    }
}
