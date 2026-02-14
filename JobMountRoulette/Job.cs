using Dalamud.Bindings.ImGui;
using Dalamud.Interface.FontIdentifier;
using Dalamud.Interface.Textures;
using Dalamud.Plugin.Services;

namespace JobMountRoulette;

public class Job(Lumina.Excel.Sheets.ClassJob rawClassJob, ITextureProvider textureProvider)
{
    private readonly ITextureProvider mTextureProvider = textureProvider;

    public uint ID { get; init;  } = rawClassJob.RowId;
    public uint JobType { get; init;  } = rawClassJob.JobType;
    public uint IconID { get; init; } = 62100 + rawClassJob.RowId;
    public string Name { get; init; } = $"{rawClassJob.NameEnglish} (id {rawClassJob.RowId})";
    public string Abbreviation { get; init; } = rawClassJob.Abbreviation.ToString();

    public bool isDiscipleOfHand { get; init; } = rawClassJob.DohDolJobIndex > -1;
    public bool isDiscipleOfLand { get; init; } = rawClassJob.DohDolJobIndex > -1;

    public ImTextureID GetIcon()
    {
        return mTextureProvider.GetFromGameIcon(new GameIconLookup(IconID)).GetWrapOrEmpty().Handle;
    }
}
