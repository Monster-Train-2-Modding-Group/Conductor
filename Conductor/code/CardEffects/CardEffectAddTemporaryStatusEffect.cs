using ShinyShoe;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that adds a status effect that is removed at the end of the turn.
    /// 
    /// Same parameters as CardEffectAddStatusEffect.
    /// </summary>
    public class CardEffectAddTemporaryStatusEffect : CardEffectAddStatusEffect
    {
        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            StatusEffectStackData? statusEffectStack = GetStatusEffectStack(cardEffectState.GetSourceCardEffectData(), cardEffectState, cardEffectParams.targets, cardEffectParams.selfTarget, coreGameManagers);
            if (statusEffectStack == null)
            {
                yield break;
            }
            int intInRange = cardEffectState.GetIntInRange();
            bool paramBool = cardEffectState.GetParamBool2();
            CharacterState.AddStatusEffectParams poolObject;
            using (GenericPools.Get<CharacterState.AddStatusEffectParams>(out poolObject))
            {
                poolObject.ApplyEffectParams(cardEffectState, cardEffectParams);
                poolObject.overrideImmunity = poolObject.overrideImmunity || cardEffectState.GetTargetMode() == TargetMode.Pyre;
                for (int num = cardEffectParams.targets.Count - 1; num >= 0; num--)
                {
                    CharacterState characterState = cardEffectParams.targets[num];
                    RngId rngId = (coreGameManagers.GetSaveManager().PreviewMode ? RngId.BattleTest : RngId.Battle);
                    if (intInRange == 0 || RandomManager.Range(0, 100, rngId) < intInRange)
                    {
                        int count = statusEffectStack.count;
                        characterState.AddStatusEffect(statusEffectStack.statusId, count, poolObject, cardEffectParams.selfTarget, allowModification: true, cardEffectParams.isFromHiddenTrigger, !paramBool);
                        var removeParams = new CharacterState.RemoveStatusEffectParams()
                        {
                            showNotification = true,
                            removeAtEndOfTurn = true,
                            fromEffectType = GetType(),
                            sourceCardState = cardEffectParams.playedCard,
                            sourceRelicState = cardEffectParams.sourceRelic
                        };
                        characterState.RemoveStatusEffect(statusEffectStack.statusId, count, removeParams, allowModification: true);
                    }
                }
                yield return coreGameManagers.GetCombatManager().RunTriggerQueue();
            }
        }
    }
}
