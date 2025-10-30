using System;
using System.Collections.Generic;

namespace JobMountRoulette.Configuration;

using JobIdentifier = uint;

[Serializable]
public sealed class CharacterConfiguration
{
    public Dictionary<JobIdentifier, JobConfiguration> JobConfigurations { get; set; } = new();

    public JobConfiguration forJob(JobIdentifier identifier)
    {
        return JobConfigurations.TryGetValue(identifier, out var config) ? config : JobConfigurations[identifier] = new JobConfiguration();
    }

    public void overrideJob(JobIdentifier identifier, JobConfiguration jobConfiguration)
    {
        JobConfigurations[identifier] = jobConfiguration;
    }
}
