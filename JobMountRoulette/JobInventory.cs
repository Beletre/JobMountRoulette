using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace JobMountRoulette;

public sealed class JobInventory
{
    private readonly ITextureProvider mTextureProvider;
    private readonly List<Job> mJobs;

    public Dictionary<JobType, List<uint>> CustomJobRoleAssignments { get; } = new()
    {
        { JobType.Tank, new List<uint>{ 19, 21, 32, 37 } },
        { JobType.Healer, new List<uint>{ 24, 28, 33, 40 } },
        { JobType.Melee, new List<uint>{ 20, 22, 30, 34, 39, 41} },
        { JobType.Ranged, new List<uint>{ 23, 25, 27, 31, 35, 36, 38, 42} },
        { JobType.Gatherer, new List<uint>{ 16, 17, 18 } },
        { JobType.Crafter, new List<uint>{ 8, 9, 10, 11, 12, 13, 14, 15 } }
    };

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

    public List<Job> GetJobsSortedByType()
    {
        var sortedJobs = new List<Job>();
        foreach (JobType type in System.Enum.GetValues(typeof(JobType)))
        {
            sortedJobs.AddRange(GetJobsByType(type));
        }
        return sortedJobs;
    }

    public enum JobType
    {
        Tank = 1,
        Healer = 2,
        Melee = 3,
        Ranged = 4,
        Gatherer = 5,
        Crafter = 6
    }

    public List<Job> GetJobsByType(JobType jobType)
    {
        if (!CustomJobRoleAssignments.TryGetValue(jobType, out var jobIds))
            return new List<Job>();

        return mJobs.Where(job => jobIds.Contains(job.ID)).ToList();
    }
}
