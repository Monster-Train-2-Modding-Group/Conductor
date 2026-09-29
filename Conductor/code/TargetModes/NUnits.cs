using static TargetHelper;

namespace Conductor.TargetModes
{
    public class NUnits : CharacterTargetSelector
    {
        public override CardTargetMode CardTargetMode { get; } = CardTargetMode.Targetless;
        private int overrideCountSpellsEffects = 0;
        private bool overrideSpellBackAttack = false;

        public override void PreCollectTargets(CardEffectState effectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, bool isTesting)
        {
            if (cardEffectParams.playedCard != null && cardEffectParams.selfTarget == null)
            {
                if (cardEffectParams.fromTrigger)
                {
                    if (effectState.GetCardEffect() is not CardEffectNULL)
                    {
                        Plugin.Logger.LogError("NUnits TargetMode not supported from within a CardTrigger on a non-CardEffectNULL CardEffect. " +
                            "Please use CardEffectNULL and set the target_mode on the next CardEffect to last_targeted_characters");
                        overrideCountSpellsEffects = 0;
                        overrideSpellBackAttack = false;
                        return;
                    }
                    overrideCountSpellsEffects = effectState.GetParamInt();
                    overrideSpellBackAttack = effectState.GetParamBool3();
                }
                else
                {
                    if (effectState.GetCardEffect() is not CardEffectNULL)
                    {
                        Plugin.Logger.LogError("NUnits TargetMode not supported from within a Cards' effects on a non-CardEffectNULL CardEffect. " +
                            "Please use CardEffectNULL and set the target_mode on the next CardEffect to last_targeted_characters");
                        overrideCountSpellsEffects = 0;
                        overrideSpellBackAttack = false;
                        return;
                    }
                    overrideCountSpellsEffects = effectState.GetParamInt();
                    overrideSpellBackAttack = effectState.GetParamBool3();
                }
            }
            else if (cardEffectParams.selfTarget != null)
            {
                if (effectState.GetCardEffect() is not CardEffectNULL)
                {
                    Plugin.Logger.LogError("NUnits TargetMode not supported from within a CharacterTrigger on a non-CardEffectNULL CardEffect. " +
                        "Please use CardEffectNULL and set the target_mode on the next CardEffect to last_targeted_characters");
                    overrideCountSpellsEffects = 0;
                    overrideSpellBackAttack = false;
                    return;
                }
                overrideCountSpellsEffects = effectState.GetParamInt();
                overrideSpellBackAttack = effectState.GetParamBool3();
            }
        }

        public override void FilterTargets(CollectTargetsData data, ICoreGameManagers coreGameManagers, IReadOnlyList<CharacterState> allValidTargets, List<CharacterState> chosenTargets)
        {
            CharacterState? self = data.selfTarget;
            int count = 0;
            // This has been called for an attack.
            if (data.inCombat)
            {
                count = self!.GetStatusEffectStacks("conductor_pierce") + 1;
                chosenTargets.AddRange(allValidTargets.Take(count));
            }
            else
            {
                count = overrideCountSpellsEffects;
                if (overrideSpellBackAttack)
                    chosenTargets.AddRange(allValidTargets.TakeLast(count));
                else
                    chosenTargets.AddRange(allValidTargets.Take(count));
            }
        }
    }
}

