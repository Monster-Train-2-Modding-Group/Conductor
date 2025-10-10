using Conductor.Extensions;
using System;
using UnityEngine.TextCore.Text;
using static CardManager;
using static CharacterState;
using static CharacterTriggerData;
using static CombatManager;

namespace Conductor.Triggers
{
    public static class CharacterTriggers
    {
        /// <summary>
        /// Triggers when any allied unit takes damage from any source.
        /// 
        /// Parameters
        ///   paramInt: Original damage dealt before shields and titanskin.
        ///   paramInt2: DamageType casted to int.
        ///   overrideTargetCharacter: Last attacker character (can not be pyre).
        /// </summary>
        public static CharacterTriggerData.Trigger Vengeance;
        internal static bool OnAlliedCharacterHit(TriggerOnCharacterHitParams data, out QueueTriggerParams? queueTriggerParams)
        {
            if (data.DamagedCharacter.GetTeamType() == data.Character.GetTeamType())
            {
                queueTriggerParams = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        // TODO pass the actual amount of damage done.
                        paramInt = data.OriginalDamage,
                        paramInt2 = (int)data.DamageParams.damageType,
                        /* GetLastAttackerCharacter is the same as damageParams.attacker except it won't be the pyre. */
                        overrideTargetCharacter = data.DamagedCharacter.GetLastAttackerCharacter()
                    }
                };
                return true;
            }
            queueTriggerParams = null;
            return false;
        }

        /// <summary>
        /// Triggers when any opposing unit is hit by an allied unit.
        /// Does not include damage from spells or status effects.
        /// 
        /// Parameters
        ///   paramInt: Original damage dealt before shields and titanskin.
        ///   paramInt2: DamageType casted to int.
        ///   overrideTargetCharacter: Last attacker character (can not be pyre).
        /// </summary>
        public static CharacterTriggerData.Trigger FollowUp;
        internal static bool OnOpposingCharacterHitByDirectAttack(TriggerOnCharacterHitParams data, out QueueTriggerParams? queueTriggerParams)
        {
            // 1. The character damaged is an opposing unit.
            // 2. The damage source has to be from a unit.
            // 3. The character who is being triggered has to be an allied unit.
            // 4. The character being triggered can not be the unit who successfully attacked.
            // 5. The damage must be from a direct attack, trample, or splash (unused) or from a Unit Ability. [this excludes damage from status effects and spells]
            if (data.DamagedCharacter.GetTeamType() != data.Character.GetTeamType() && data.DamageParams.attacker != null && data.DamageParams.attacker.GetTeamType() == data.Character.GetTeamType() && data.Character != data.DamageParams.attacker &&
                (data.DamageParams.damageType == Damage.Type.DirectAttack || data.DamageParams.damageType == Damage.Type.Trample || data.DamageParams.damageType == Damage.Type.Splash || (data.DamageParams.damageSourceCard != null && data.DamageParams.damageSourceCard.IsUnitAbility())) 
                )
            {
                queueTriggerParams = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramInt = data.OriginalDamage,
                        paramInt2 = (int)data.DamageParams.damageType,
                        /* GetLastAttackerCharacter is the same as damageParams.attacker except it won't be the pyre. */
                        overrideTargetCharacter = data.DamagedCharacter.GetLastAttackerCharacter()
                    }
                };
                return true;
            }
            queueTriggerParams = null;
            return false;
        }
        
        /// <summary>
        /// Triggers when any card is discarded by an effect before end of turn.
        /// Parameters
        ///   paramInt: Max Hand Size - Hand size.
        ///   paramInt2: The card's CardType as an int.
        /// </summary>
        public static CharacterTriggerData.Trigger Junk;
        internal static bool OnDiscardedAnyCard(TriggerOnCardDiscardedParams data, out QueueTriggerParams? triggerQueueData)
        {
            DiscardCardParams discardCardParams = data.DiscardCardParams;
            if (!discardCardParams.wasPlayed || (discardCardParams.triggeredByCard && discardCardParams.discardCard.HasTrait(typeof(CardTraitTreasure))))
            {
                var cardManager = data.CoreGameManagers.GetCardManager();
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramInt = cardManager.GetMaxHandSize() - cardManager.GetNumCardsInHand(),
                        paramInt2 = (int) discardCardParams.discardCard.GetCardType()
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }

        /// <summary>
        /// Triggers when a unit encounters another allied unit.
        /// 1. When unit is spawned triggers once per other allied unit.
        /// 2. When unit relocates (up/down floor) triggers once per other allied unit.
        /// 
        /// Parameters
        ///   paramInt: Number of allied units in the room
        ///   paramInt2: 0 if triggered as a result of unit spawning, 1 otherwise.
        ///   overrideTargetCharacter: the other allied unit.
        /// </summary>
        public static CharacterTriggerData.Trigger Encounter;

        /// <summary>
        /// The Better Rally Trigger™, Triggers whenever an allied unit is spawned on the floor.
        /// Does not trigger on Spawn unless Spawn creates a Funguy.
        /// This is a distinction with Rally. Rally triggers whenever a monster card is played, on spawn, and other weird scenarios.
        /// 
        /// Parameters
        ///   paramInt: Number of allied characters in the room.
        ///   overrideTargetCharacter: The allied unit that spawned.
        /// </summary>
        public static CharacterTriggerData.Trigger Mobilize;

        /// <summary>
        /// Triggers when a Blight or Scourge is played.
        /// 
        /// Parameters
        ///   paramInt: 1 if the card was a scourge, 0 otherwise.
        ///   paramInt2: always 1.
        /// </summary>
        public static CharacterTriggerData.Trigger Penance;

        /// <summary>
        /// Triggers when a Blight or Scourge is played or discarded
        /// 
        /// Parameters
        ///   paramInt: 1 if the card was a scourge, 0 otherwise.
        ///   paramInt2: 1 if the card was played, 0 otherwise.
        /// </summary>
        public static CharacterTriggerData.Trigger Accursed;
        internal static bool OnPlayedBlightOrScourge(TriggerOnCardPlayedParams data, out QueueTriggerParams? triggerQueueData)
        {
            var cardType = data.Card.GetCardType();
            if (cardType == CardType.Blight || cardType == CardType.Junk)
            {
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramInt = cardType == CardType.Blight ? 0 : 1,
                        paramInt2 = 1
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }
        internal static bool OnDiscardedBlightOrScourge(TriggerOnCardDiscardedParams data, out QueueTriggerParams? triggerQueueData)
        {
            triggerQueueData = null;
            DiscardCardParams discardCardParams = data.DiscardCardParams;
            CardType cardType = discardCardParams.discardCard.GetCardType();
            if (discardCardParams.triggeredByCard && (cardType == CardType.Blight || cardType == CardType.Junk))
            {
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramInt = cardType == CardType.Blight ? 0 : 1,
                        paramInt2 = 0
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }

        /// <summary>
        /// Triggers when any ability is used on the floor.
        /// Equipped -> Artificer as Conjure -> Evoke
        /// 
        /// Parameters
        ///   paramInt: 1 if this unit is the one that activated an ability, 0 otherwise.
        ///   overrideTargetCharacter: Character that activated the ability.
        /// </summary>
        public static CharacterTriggerData.Trigger Evoke;
        internal static bool OnPlayedUnitAbility(TriggerOnCardPlayedParams data, out QueueTriggerParams? triggerQueueData)
        {            
            if (data.Card.IsUnitAbility())
            {
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramInt = data.Character == data.CharacterThatActivatedAbility ? 1 : 0,
                        overrideTargetCharacter = data.CharacterThatActivatedAbility
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }

        // The following a Silent event triggers, if using set hideVisualAndIgnoreSilence on all CharacterTriggers.

        /// <summary>
        /// Silent event trigger that fires when a buff is applied to the unit.
        /// 
        /// Parameters:
        ///   paramString: statusId
        ///   paramInt: Number of distinct buffs on unit.
        ///   paramInt2: total number of stacks of statusId
        /// </summary>
        public static CharacterTriggerData.Trigger OnBuffed;
        internal static bool OnGainedABuff(TriggerOnStatusAddedParams data, out QueueTriggerParams? triggerQueueData)
        {
            if (data.StatusEffectStack.State.GetDisplayCategory() == StatusEffectData.DisplayCategory.Positive)
            {
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramString = data.StatusId,
                        paramInt = data.Character.GetNumberUniqueStatusEffectsInCategory(StatusEffectData.DisplayCategory.Positive, true),
                        paramInt2 = data.StatusEffectStack.Count
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }

        /// <summary>
        /// Silent event trigger that fires when a debuff is applied to the unit.
        /// 
        /// Parameters:
        ///   paramString: statusId
        ///   paramInt: Number of distinct debuffs on unit.
        ///   paramInt2: total number of stacks of statusId
        /// </summary>
        public static CharacterTriggerData.Trigger OnDebuffed;
        internal static bool OnGainedADebuff(TriggerOnStatusAddedParams data, out QueueTriggerParams? triggerQueueData)
        {
            if (data.StatusEffectStack.State.GetDisplayCategory() == StatusEffectData.DisplayCategory.Negative)
            {
                triggerQueueData = new QueueTriggerParams
                {
                    fireTriggersData = new FireTriggersData
                    {
                        paramString = data.StatusId,
                        paramInt = data.Character.GetNumberUniqueStatusEffectsInCategory(StatusEffectData.DisplayCategory.Negative, true),
                        paramInt2 = data.StatusEffectStack.Count
                    }
                };
                return true;
            }
            triggerQueueData = null;
            return false;
        }
    }
}
