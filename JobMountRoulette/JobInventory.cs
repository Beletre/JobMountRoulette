using Dalamud.Plugin.Services;
using System.Collections.Generic;
using System.Linq;

namespace JobMountRoulette;

public sealed class JobInventory(IDataManager dataManager, ITextureProvider textureProvider)
{
    private readonly List<Job> mJobs = [.. (from job in dataManager.GameData.GetExcelSheet<Lumina.Excel.Sheets.ClassJob>()
                                        select new Job(job, textureProvider))];

    public Dictionary<Role, List<uint>> JobIDsForRole { get; } = new()
    {
        { Role.Tank, new List<uint>{ 19, 21, 32, 37 } },
        { Role.Healer, new List<uint>{ 24, 28, 33, 40 } },
        { Role.Melee, new List<uint>{ 20, 22, 30, 34, 39, 41} },
        { Role.Ranged, new List<uint>{ 23, 31, 38, 25, 27, 35, 42} },
        { Role.Crafter, new List<uint>{ 8, 9, 10, 11, 12, 13, 14, 15 } },
        { Role.Gatherer, new List<uint>{ 16, 17, 18 } },
        { Role.Limited, new List<uint>{ 36 } }
    };

    public Job? Find(uint id)
    {
        return mJobs.FirstOrDefault(job => job.ID == id);
    }

    public enum Role
    {
        Tank = 1,
        Healer = 2,
        Melee = 3,
        Ranged = 4,
        Crafter = 5,
        Gatherer = 6,
        Limited = 7
    }

    public List<Job> FindByRole(Role role)
    {
        if (!JobIDsForRole.TryGetValue(role, out var jobIds))
            return [];

        // Sort jobs according to the order in jobIds
        var jobDict = mJobs.ToDictionary(job => job.ID, job => job);
        var sortedJobs = new List<Job>();
        foreach (var id in jobIds)
        {
            if (jobDict.TryGetValue(id, out var job))
                sortedJobs.Add(job);
        }
        return sortedJobs;
    }
}
