using System.Collections;
using UnityEngine;

// TODO a fancy parser for parsing a TargetMode to additionalParamInt.
namespace Conductor.CardEffects
{
    /// <summary>
    /// A more generic version of CardEffectHealAndDamageRelative. The issues with the stock version is that it was very hardcoded.
    /// 1) In the TargetMode only the first character within the list of targets got the heal.
    /// 2) The CardEffect hardcodes the damage (5 * healed_amount) to go to the front unit of the opposite team.
    /// 
    /// This version aims to support more features.
    /// 
    /// 1) All targets in TargetMode are healed. (Note that behaviour is not defined if target_team is both).
    /// 2) A second TargetMode is passed in through AdditionalParamInt
    /// 3) The TargetMode is passed with the opposite team as 1)
    /// 4) The amount healed is multiplied by ParamMultiplier
    /// 5) If ParamBool is true the damage is split evenly among damaged units. Otherwise each unit takes healed_amount * multiplier damage.
    /// 
    /// Params:
    ///   ParamInt: Amount to heal.
    ///   ParamMultiplier: Damage multiplier.
    ///   AdditionalParmaInt: Second target mode for the damage step
    ///   ParamBool: True to split the damage, false to apply the same amount of damage
    ///   
    /// Example Json
    /// "effects": [ 
    ///    {
    ///      "id": "HealAndSplitHealedAmountTimes10EvenlyToEnemies",
    ///      "name": {
    ///        "id": "@CardEffectHealAndDamageRelativeCustomTargets",
    ///        "mod_reference": "Conductor"
    ///      },
    ///      "target_mode": "drop_target_character",
    ///      "target_team": "monsters",
    ///      "param_int": 20,
    ///      "param_multiplier": 10,
    ///      "param_int_2": 0, // TargetMode.Room
    ///      "param_bool": true
    ///    }
    ///  ]
    /// </summary>
    public sealed class CardEffectHealAndDamageRelativeCustomTargets : CardEffectHeal
    {
        private List<CharacterState> toProcessCharacters = [];

        private TargetHelper.CollectTargetsData collectTargetsData;

        public override bool CanRandomizeTargetMode => false;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Heal Amount."),
                [CardEffectFieldNames.AdditionalParamInt.GetFieldName()] = new PropDescription("Damage Multiplier."),
                [CardEffectFieldNames.UseIntRange.GetFieldName()] = new PropDescription("Use Range For Heal Amount."),
                [CardEffectFieldNames.ParamMinInt.GetFieldName()] = new PropDescription("Min Heal Amount."),
                [CardEffectFieldNames.ParamMaxInt.GetFieldName()] = new PropDescription("Max Heal Amount."),
                [CardEffectFieldNames.ParamMultiplier.GetFieldName()] = new PropDescription("Range Multiplier."),
                [CardEffectFieldNames.AdditionalParamInt.GetFieldName()] = new PropDescription("(TargetMode) TargetMode to apply damage to."),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("True to split damage, or False apply damage equally.")
            };
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            return cardEffectParams.targets.Count > 0;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers systemManagers)
        {
            int num = 0;
            foreach (var target in cardEffectParams.targets)
            {
                int healAmount = GetHealAmount(cardEffectState);
                int oldHp = target.GetHP();
                yield return target.ApplyHeal(healAmount, triggerOnHeal: true, cardEffectParams.playedCard);
                num += Mathf.Max(Mathf.RoundToInt((target.GetHP() - oldHp) * cardEffectState.GetParamMultiplier()), 0);
            }

            if (num > 0)
            {
                var testTarget = cardEffectParams.targets[0];
                CollectTargets(cardEffectState, cardEffectParams, testTarget.GetTeamType(), testTarget.GetCurrentRoomIndex(), coreGameManagers);
                if (toProcessCharacters.Count <= 0)
                {
                    yield break;
                }
                int damage = cardEffectState.GetParamBool() ? num / toProcessCharacters.Count : num;
                foreach (var target2 in toProcessCharacters)
                {
                    yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(damage, target2, new CombatManager.ApplyDamageToTargetParameters
                    {
                        playedCard = cardEffectParams.playedCard,
                        finalEffectInSequence = cardEffectParams.finalEffectInSequence,
                        appliedVfx = cardEffectState.GetAppliedVFX(),
                        appliedVfxId = cardEffectParams.appliedVfxId
                    });
                }
            }
        }

        private void CollectTargets(CardEffectState cardEffectState, CardEffectParams cardEffectParams, Team.Type team, int roomIndex, ICoreGameManagers coreGameManagers, bool isTesting = false)
        {
            toProcessCharacters.Clear();
            collectTargetsData.Reset((TargetMode)cardEffectState.GetAdditionalParamInt());
            collectTargetsData.targetTeamType = team.GetOppositeTeam();
            collectTargetsData.roomIndex = roomIndex;
            collectTargetsData.inCombat = false;
            collectTargetsData.isTesting = isTesting;
            TargetHelper.CollectTargets(collectTargetsData, coreGameManagers, ref toProcessCharacters);
            collectTargetsData.Reset(TargetMode.FrontInRoom);
        }
    }

}
