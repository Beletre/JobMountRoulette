using System;
using System.Collections.Generic;

namespace JobMountRoulette.Configuration;

using JobIdentifier = byte;

[Serializable]
public sealed class CharacterConfiguration
{
    public Dictionary<JobIdentifier, JobConfiguration> JobConfigurations { get; set; } = new();

    public JobConfiguration forJob(JobIdentifier identifier)
    {
        return JobConfigurations.TryGetValue(identifier, out var config) ? config : JobConfigurations[identifier] = new JobConfiguration();
    }
}
