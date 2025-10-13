using HarmonyLib;
using JetBrains.Annotations;
using ShinyShoe.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using static CharacterState;

namespace Conductor.StatusEffects
{
    class StatusEffectCurseGiverState : StatusEffectState
    {
        public const string StatusId = "conductor_curse";

        public override void OnStacksAdded(CharacterState character, int numStacksAdded, CharacterState.AddStatusEffectParams addStatusEffectParams, ICoreGameManagers coreGameManagers)
        {
            if (character.GetTeamType() == Team.Type.Monsters)
            {
                character.AddStatusEffect("conductor_curse_monster", numStacksAdded, addStatusEffectParams, null, false, false, false);
            }
            else if (character.IsMiniboss() || character.IsOuterTrainBoss() || character.IsTrueFinalBoss())
            {
                character.AddStatusEffect("conductor_curse_removal", numStacksAdded, addStatusEffectParams, null, false, false, false);
            }
            else
            {
                character.AddStatusEffect("conductor_curse_persistent", numStacksAdded, addStatusEffectParams, null, false, false, false);
            }
        }

        public override void OnStacksRemoved(CharacterState character, int numStacksRemoved, ICoreGameManagers coreGameManagers)
        {
            if (character.GetTeamType() == Team.Type.Monsters)
            {
                character.RemoveStatusEffect("conductor_curse_monster", numStacksRemoved, false);
            }
            else if (character.IsMiniboss() || character.IsOuterTrainBoss() || character.IsTrueFinalBoss())
            {
                character.RemoveStatusEffect("conductor_curse_removal", numStacksRemoved, false);
            }
            else
            {
                character.RemoveStatusEffect("conductor_curse_persistent", numStacksRemoved, false);
            }
        }
    }

    class StatusEffectCurseBase : StatusEffectState, IDamageStatusEffect
    {
        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.attacked == null)
            {
                return false;
            }
            if (inputTriggerParams.attacker == null)
            {
                return false;
            }
            outputTriggerParams.damage = inputTriggerParams.damage;
            if (inputTriggerParams.damage > 0)
            {
                if (inputTriggerParams.damageType == Damage.Type.DirectAttack || inputTriggerParams.damageType == Damage.Type.Trample)
                {
                    return true;
                }
                if (inputTriggerParams.damageSourceCard != null && inputTriggerParams.damageSourceCard.IsUnitAbility())
                {
                    return true;
                }
            }
            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            CharacterState attacker = inputTriggerParams.attacker;
            CharacterState attacked = inputTriggerParams.attacked;

            if (attacked == null)
            {
                Plugin.Logger.LogError("StatusEffectCurseBase.OnTriggered() attacked character should not be null!");
            }
            else if (attacker == null)
            {
                Plugin.Logger.LogError("StatusEffectCurseState.OnTriggered() attacker character should not be null!");
            }
            else if (inputTriggerParams.damage > 0 && (inputTriggerParams.damageType == Damage.Type.DirectAttack || inputTriggerParams.damageType == Damage.Type.Trample || (inputTriggerParams.damageSourceCard != null && inputTriggerParams.damageSourceCard.IsUnitAbility())))
            {
                int damageForCurse = inputTriggerParams.damageSustained;
                // Interaction with Intangible unit takes unmodified damage + damage modifiers.
                if (attacked.HasStatusEffect(GetStatusId()))
                {
                    // Redo calculation of damage manually.
                    damageForCurse = inputTriggerParams.unmodifiedDamage;
                    int meleeWeaknessStacks = attacked.GetStatusEffectStacks(StatusEffectMeleeWeaknessState.StatusId);
                    if (meleeWeaknessStacks > 0)
                    {
                        damageForCurse *= meleeWeaknessStacks;
                    }
                    int pyreGelStacks = attacked.GetStatusEffectStacks(StatusEffectPyregelState.StatusId);
                    damageForCurse += pyreGelStacks;
                }
                int statusEffectStacks = attacker.GetStatusEffectStacks(GetStatusId());
                int effectMagnitude = GetEffectMagnitude(statusEffectStacks);
                int damage = GetParamSecondaryInt() == 1 ? effectMagnitude : effectMagnitude * damageForCurse;
                if (GetParamSecondaryInt() == 1)
                {
                    outputTriggerParams.count = statusEffectStacks - 1;
                }
                yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(damage, attacker, new CombatManager.ApplyDamageToTargetParameters
                {
                    // TODO Cursed Damage Type.
                    damageType = Damage.Type.Default,
                    affectedVfx = GetSourceStatusEffectData()?.GetOnAffectedVFX(),
                    relicState = inputTriggerParams.suppressingRelic
                });
            }
        }
    }

    /*
    class StatusEffectCurseState : StatusEffectState, IDamageStatusEffect
    {
        public const string StatusId = "conductor_curse";

        public static readonly CharacterState.RemoveStatusEffectParams removeStatusEffectParams = new CharacterState.RemoveStatusEffectParams
        {
            removeAtEndOfTurn = true,
            showNotification = true,
        };

        public override bool TestTrigger(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            if (inputTriggerParams.attacked == null)
            {
                return false;
            }
            if (inputTriggerParams.attacker == null)
            {
                return false;
            }
            outputTriggerParams.damage = inputTriggerParams.damage;
            if (inputTriggerParams.damage > 0)
            {
                if (inputTriggerParams.damageType == Damage.Type.DirectAttack || inputTriggerParams.damageType == Damage.Type.Trample)
                {
                    return true;
                }
                if (inputTriggerParams.damageSourceCard != null && inputTriggerParams.damageSourceCard.IsUnitAbility())
                {
                    return true;
                }
            }
            return false;
        }

        protected override IEnumerator OnTriggered(InputTriggerParams inputTriggerParams, OutputTriggerParams outputTriggerParams, ICoreGameManagers coreGameManagers)
        {
            CharacterState attacker = inputTriggerParams.attacker;
            CharacterState attacked = inputTriggerParams.attacked;
            
            if (attacked == null)
            {
                Log.Warning(LogGroups.Gameplay, "StatusEffectCurseState.OnTriggered() attacked character should not be null!");
            }
            else if (attacker == null)
            {
                Log.Warning(LogGroups.Gameplay, "StatusEffectCurseState.OnTriggered() attacker character should not be null!");
            }
            else if (inputTriggerParams.damage > 0 && (inputTriggerParams.damageType == Damage.Type.DirectAttack || inputTriggerParams.damageType == Damage.Type.Trample || (inputTriggerParams.damageSourceCard != null && inputTriggerParams.damageSourceCard.IsUnitAbility())))
            {
                int damageForCurse = inputTriggerParams.damageSustained;
                // Interaction with Intangible unit takes unmodified damage + damage modifiers.
                if (attacked.HasStatusEffect(StatusEffectIntangibleState.StatusId))
                {
                    // Redo calculation of damage manually.
                    damageForCurse = inputTriggerParams.unmodifiedDamage;
                    int meleeWeaknessStacks = attacked.GetStatusEffectStacks(StatusEffectMeleeWeaknessState.StatusId);
                    if (meleeWeaknessStacks > 0)
                    {
                        damageForCurse *= meleeWeaknessStacks;
                    }
                    int pyreGelStacks = attacked.GetStatusEffectStacks(StatusEffectPyregelState.StatusId);
                    damageForCurse += pyreGelStacks;
                }
                int statusEffectStacks = attacker.GetStatusEffectStacks(StatusId);
                int effectMagnitude = GetEffectMagnitude(statusEffectStacks);
                if (attacker.IsOuterTrainBoss() || attacker.IsTrueFinalBoss() || attacker.IsMiniboss())
                {
                    SetRemoveAtEndOfTurn(true);
                }
                yield return coreGameManagers.GetCombatManager().ApplyDamageToTarget(effectMagnitude * damageForCurse, attacker, new CombatManager.ApplyDamageToTargetParameters
                {
                    // TODO Cursed Damage Type.
                    damageType = Damage.Type.Default,
                    affectedVfx = GetSourceStatusEffectData()?.GetOnAffectedVFX(),
                    relicState = inputTriggerParams.suppressingRelic
                });
                
            }
        }
    }
    */
}
