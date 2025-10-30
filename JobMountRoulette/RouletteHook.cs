using Dalamud.Hooking;
using Dalamud.Plugin.Services;
using FFXIVClientStructs.FFXIV.Client.Game;
using FFXIVClientStructs.FFXIV.Client.Game.Character;
using FFXIVClientStructs.FFXIV.Client.Game.Control;
using JobMountRoulette.Configuration;
using System;

namespace JobMountRoulette;

internal sealed class RouletteHook : IDisposable
{
    private const uint ROULETTE_ACTION_ID = 9;

    private readonly PluginConfiguration mPluginConfiguration;
    private readonly IClientState mClientState;

    private readonly Hook<UseAction>? mUseActionHook;
    public unsafe delegate byte UseAction(ActionManager* actionManager, ActionType actionType, uint actionID, long targetID = 3758096384U, uint a4 = 0U, uint a5 = 0U, uint a6 = 0U, void* a7 = default);

    private readonly Hook<InitializeCastBar>? mInitializeCastbarHook;
    private unsafe delegate void InitializeCastBar(ActionManager* actionManager, BattleChara* chara, ActionType actionType, uint actionId, uint spellId, int mountRouletteIndex);

    private bool mOverrideIcon = false;

    public unsafe RouletteHook(PluginConfiguration pluginConfiguration, IClientState clientState, IGameInteropProvider gameInteropProvider)
    {
        mPluginConfiguration = pluginConfiguration;
        mClientState = clientState;

        mUseActionHook = gameInteropProvider.HookFromAddress<UseAction>(ActionManager.MemberFunctionPointers.UseAction, OnUseAction);
        mUseActionHook.Enable();

        mInitializeCastbarHook = gameInteropProvider.HookFromSignature<InitializeCastBar>("E8 ?? ?? ?? ?? 41 83 FC 04 0F 84 ?? ?? ?? ?? 41 81 FC 5D 4E 00 00", OnInitializeCastBar);
        mInitializeCastbarHook.Enable();
    }

    private unsafe byte OnUseAction(ActionManager* actionManager, ActionType actionType, uint actionID, long targetID, uint a4, uint a5, uint a6, void* a7)
    {
        var isRouletteActionID = actionID == ROULETTE_ACTION_ID && actionType == ActionType.GeneralAction;
        if (isRouletteActionID)
        {
            var characterConfiguration = mPluginConfiguration.forCharacter(mClientState.LocalContentId);
            var jobConfiguration = characterConfiguration.forJob(mClientState.LocalPlayer!.ClassJob.Value.JobIndex);

            var mountIdentifiers = jobConfiguration.CustomRouletteMounts;
            if (mountIdentifiers.Count > 0)
            {
                var random = new Random();
                var randomMountIdentifier = mountIdentifiers[random.Next(mountIdentifiers.Count)];

                mOverrideIcon = true;
                var result = mUseActionHook!.Original(actionManager, ActionType.Mount, randomMountIdentifier, targetID, a4, a5, a6, a7);
                mOverrideIcon = false;

                return result;
            }
        }

        return mUseActionHook!.Original(actionManager, actionType, actionID, targetID, a4, a5, a6, a7);
    }

    private unsafe void OnInitializeCastBar(ActionManager* actionManager, BattleChara* chara, ActionType actionType, uint actionId, uint spellId, int mountRouletteIndex)
    {
        if (chara == Control.GetLocalPlayer() && mOverrideIcon && actionType == ActionType.Mount)
        {
            mInitializeCastbarHook!.Original(actionManager, chara, actionType, actionId, spellId, 1);
        }
        else
        {
            mInitializeCastbarHook!.Original(actionManager, chara, actionType, actionId, spellId, mountRouletteIndex);
        }
    }

    public void Dispose()
    {
        mInitializeCastbarHook?.Dispose();
        mUseActionHook?.Dispose();
    }
}
