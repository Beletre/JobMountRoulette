using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace JobMountRoulette;

public sealed class JobInventory
{
    private readonly ITextureProvider mTextureProvider;
    private readonly List<Job> mJobs;

    public JobInventory(IDataManager dataManager, ITextureProvider textureProvider)
    {
        mTextureProvider = textureProvider;
        mJobs = (from job in dataManager.GameData.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>()
                 select new Job(job, mTextureProvider)).ToList();
    }

    public List<Job> GetJobs()
    {
        return mJobs;
    }

    public Job? GetJob(uint id)
    {
        return mJobs.FirstOrDefault(job => job.ID == id);
    }
}
