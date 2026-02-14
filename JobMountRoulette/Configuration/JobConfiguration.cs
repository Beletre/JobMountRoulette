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

    public void SetMountEnabled(MountIdentifier mountId, bool enabled)
    {
        if (enabled)
        {
            if (!CustomRouletteMounts.Contains(mountId))
            {
                CustomRouletteMounts.Add(mountId);
            }
        }
        else
        {
            CustomRouletteMounts.Remove(mountId);
        }
    }

    public void ToggleMount(MountIdentifier mountId)
    {
        if (!CustomRouletteMounts.Remove(mountId))
        {
            CustomRouletteMounts.Add(mountId);
        }
    }
}

