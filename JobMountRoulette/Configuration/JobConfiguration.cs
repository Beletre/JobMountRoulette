using System;
using System.Collections.Generic;

namespace JobMountRoulette.Configuration;

using MountIdentifier = uint;

[Serializable]
public sealed class JobConfiguration
{
    public bool UseCustomRoulette { get; set; } = false;
    public List<MountIdentifier> CustomRouletteMounts { get; set; } = [];

    public bool IsMountEnabled(MountIdentifier mountId)
    {
        return CustomRouletteMounts.Contains(mountId);
    }

    public void ToggleMount(MountIdentifier mountId)
    {
        if (CustomRouletteMounts.Contains(mountId))
        {
            CustomRouletteMounts.Remove(mountId);
        }
        else
        {
            CustomRouletteMounts.Add(mountId);
        }
    }
}

