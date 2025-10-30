using Dalamud.Configuration;
using System;
using System.Collections.Generic;

namespace JobMountRoulette.Configuration;

using CharacterIdentifier = ulong;

[Serializable]
public sealed class PluginConfiguration : IPluginConfiguration
{
    public int Version { get; set; } = 0;
    public Dictionary<CharacterIdentifier, CharacterConfiguration> CharacterConfigurations { get; set; } = [];

    public CharacterConfiguration forCharacter(CharacterIdentifier identifier)
    {
        return CharacterConfigurations.TryGetValue(identifier, out var config) ? config : CharacterConfigurations[identifier] = new CharacterConfiguration();
    }
}
