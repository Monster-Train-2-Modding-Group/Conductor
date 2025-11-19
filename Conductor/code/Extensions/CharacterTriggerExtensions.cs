using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using static CharacterState;

namespace Conductor.Extensions
{
    public struct TriggerOnStatusAddedParams
    {
        /// <summary>
        /// The character who is receiving the status effect (and whose trigger is being considered to fire).
        /// </summary>
        public CharacterState Character { get; init; }
        /// <summary>
        /// The status effect that was added.
        /// </summary>
        public string StatusId { get; init; }
        /// <summary>
        /// The number of stacks of status effect that was added.
        /// </summary>
        public int NumStacks { get; init; }
        /// <summary>
        /// The status effect stack data. This contains the StatusEffectState subclass.
        /// The current count of status effect that the character has among other things.
        /// </summary>
        public CharacterState.StatusEffectStack StatusEffectStack { get; init; }
        /// <summary>
        /// The remove status effect params.
        /// </summary>
        public CharacterState.AddStatusEffectParams AddParams { get; init; }
        /// <summary>
        /// The Room Index where the card was played.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room where the card was played.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnStatusRemovedParams
    {
        /// <summary>
        /// The character who is losing the status effect (and whose trigger is being considered to fire).
        /// </summary>
        public CharacterState Character { get; init; }
        /// <summary>
        /// The status effect that was removed.
        /// </summary>
        public string StatusId { get; init; }
        /// <summary>
        /// The number of stacks of status effect that was removed.
        /// </summary>
        public int NumStacks { get; init; }
        /// <summary>
        /// The status effect stack data. This contains the StatusEffectState subclass.
        /// The current count of status effect that the character has among other things.
        /// </summary>
        public CharacterState.StatusEffectStack StatusEffectStack { get; init; }
        /// <summary>
        /// The remove status effect params.
        /// </summary>
        public CharacterState.RemoveStatusEffectParams RemoveParams { get; init; }
        /// <summary>
        /// The Room Index where the removal happened.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room where the removal happened.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnCardPlayedParams
    {
        /// <summary>
        /// The character whose trigger is being considered to fire.
        /// </summary>
        public CharacterState Character { get; internal set; }
        /// <summary>
        /// The card or ability played.
        /// </summary>
        public CardState Card { get; init; }
        /// <summary>
        /// The Room Index where the card was played.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room where the card was played.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// Character whose ability was activated.
        /// </summary>
        public CharacterState CharacterThatActivatedAbility { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnCardDiscardedParams
    {
        /// <summary>
        /// The character whose trigger is being considered to fire.
        /// </summary>
        public CharacterState Character { get; internal set; }
        /// <summary>
        /// The Room Index where the card was discarded.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room where the card was discarded.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// DiscardCardParams (will contain the card discarded, why it was discarded among other things).
        /// </summary>
        public CardManager.DiscardCardParams DiscardCardParams { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnAnotherSpawnParams
    {
        /// <summary>
        /// The character whose trigger is being considered to fire.
        /// </summary>
        public CharacterState Character { get; init; }
        /// <summary>
        /// The character that was spawned in.
        /// </summary>
        public CharacterState SpawnedCharacter { get; init; }
        /// <summary>
        /// The room index that spawnedCharacter spawned in.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room that spawnedCharacter spawned in.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnCharacterHitParams
    {
        /// <summary>
        /// The character whose trigger is being considered to fire.
        /// </summary>
        public CharacterState Character { get; internal set; }
        /// <summary>
        /// The character that was hit.
        /// </summary>
        public CharacterState DamagedCharacter { get; init; }
        /// <summary>
        /// The original amount of damage dealt before any shields / titanskin.
        /// </summary>
        public int OriginalDamage { get; init; }
        /// <summary>
        /// ApplyDamageParams (will contain the parameters of damage including the attacker among other things).
        /// </summary>
        public ApplyDamageParams DamageParams { get; init; }
        /// <summary>
        /// The room index that this interaction occurred.
        /// </summary>
        public int RoomIndex { get; init; }
        /// <summary>
        /// The room that this interaction occurred.
        /// </summary>
        public RoomState Room { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    public struct TriggerOnPyreDamageParams
    {
        public CharacterState Pyre {  get; init; }
        /// <summary>
        /// The character whose trigger is being considered to fire.
        /// </summary>
        public CharacterState Character { get; internal set; }
        /// <summary>
        /// Amount of Pyre damage taken.
        /// </summary>
        public int Damage { get; init; }
        /// <summary>
        /// ApplyDamageParams (will contain the parameters of damage including the attacker among other things).
        /// </summary>
        public ApplyDamageParams DamageParams { get; init; }
        /// <summary>
        /// CoreGameManagers (available if you wish to modify other game state if the trigger is fired).
        /// </summary>
        public ICoreGameManagers CoreGameManagers { get; init; }
    }

    /// <summary>
    /// Parameters forwarded to CombatManager.QueueTrigger
    /// 
    /// Completely optional for the Trigger Delegate functions. If set to null then a default parameters are used.
    /// Setting any field overwrites the associated parameter of CombatManager.QueueTrigger.
    /// </summary>
    public struct QueueTriggerParams
    {
        /// <summary>
        /// The dying character, Forwarded to CardEffectParams.dyingCharacter
        /// Currently this is used by Slay / Extinguish and the parameter itself is used in CardEffectAdjustDragonsHoard[PerStatusEffect]
        /// and CardEffectSpawnMonsterOrAddStatusEffect (Spawn)
        /// </summary>
        public CharacterState? dyingCharacter = null;
        /// <summary>
        /// Only for trigger type PostCombatHealing, prevent healing effect.
        /// </summary>
        public bool canAttackOrHeal = true;
        /// <summary>
        /// If you set this to false the trigger doesn't actually fire. Silence/Mute will set this down the chain, so you don't need to handle that.
        /// </summary>
        public bool canFireTriggers = true;
        /// <summary>
        /// FireTriggersData. Optional, but if set, it will sets various trigger properties with this.
        /// Notably,
        ///    paramInt is used for CharacterTriggerData.triggerAtThreshold which powers Fel's Unchained III effect.
        ///    overrideTargetCharacter can be used indirectly with TargetMode (LastAttackedCharacter if effect used within a CharacterTrigger) and is also forwarded to CardEffectParams.overrideTargetCharacter
        ///    any other parameters can be used in RelicEffect implementations particularly effects implementing the ICharacterActionRelicEffect interface.
        /// </summary>
        public CharacterState.FireTriggersData? fireTriggersData = null;
        /// <summary>
        /// Manually override the trigger count. Note that artifacts (i.e. Ashes of the Fallen for Summon Trigger) will modify this value further
        /// (that is you need not query for artifacts that modify trigger count, as that is automatically done later).
        /// </summary>
        public int triggerCount = 1;
        /// <summary>
        /// Rarely used. Setting this prevents other triggers of same CharacterTriggerData.Trigger type from firing
        /// if it is not exactly this specific CharacterTriggerState.
        /// 
        /// The current use of this is to run a Deathwish lost (Vanguard cancel) trigger when the trigger is removed.
        /// 
        /// I'd suggest not depending on this working as in a rare case if the trigger gets requeued this param is not forwarded.
        /// Best to not use at all, since it is an internal parameter.
        /// </summary>
        public CharacterTriggerState? exclusiveTrigger = null;
        public QueueTriggerParams() {}
    }

    public delegate bool TriggerOnStatusAddedDelegate(TriggerOnStatusAddedParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnStatusRemovedDelegate(TriggerOnStatusRemovedParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnCardPlayedDelegate(TriggerOnCardPlayedParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnCardDiscardedDelegate(TriggerOnCardDiscardedParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnAnotherSpawnDelegate(TriggerOnAnotherSpawnParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnCharacterHitDelegate(TriggerOnCharacterHitParams data, out QueueTriggerParams? outParam);
    public delegate bool TriggerOnPyreDamageDelegate(TriggerOnPyreDamageParams data, out QueueTriggerParams? outParam);

    public static class CharacterTriggerExtensions
    {
        internal readonly static HashSet<CharacterTriggerData.Trigger> PreCharacterTriggerAllowedTriggers = [];

        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnStatusAddedDelegate> TriggersOnStatusAdded = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnStatusRemovedDelegate> TriggersOnStatusRemoved = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnCardPlayedDelegate> TriggersOnCardPlayed = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnCardDiscardedDelegate> TriggersOnCardDiscarded = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnAnotherSpawnDelegate> TriggersOnAnotherSpawn = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnCharacterHitDelegate> TriggersOnCharacterHit = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, TriggerOnPyreDamageDelegate> TriggersOnPyreDamage = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, List<CharacterTriggerData.Trigger>> TriggerAliases = [];
        internal readonly static Dictionary<CharacterTriggerData.Trigger, List<CharacterTriggerData.Trigger>> ReverseTriggerAliases = [];

        private static bool IsVanillaTrigger(CharacterTriggerData.Trigger trigger)
        {
            if ((int)trigger <= (from int x in Enum.GetValues(typeof(CharacterTriggerData.Trigger)).AsQueryable() select x).Max())
            {
                Plugin.Logger.LogError($"Attempt to redefine vanilla trigger {trigger.ToString()}, you probably didn't mean to do this?");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Sets the trigger to fire in CharacterState.AddStatusEffect after the status effect is added to the character
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of a status effect being added.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnStatusEffectAdded(this CharacterTriggerData.Trigger trigger, TriggerOnStatusAddedDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnStatusAdded.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in CharacterState.RemoveStatusEffect after the status effect is removed from the character
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of a status effect being removed.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnStatusEffectRemoved(this CharacterTriggerData.Trigger trigger, TriggerOnStatusRemovedDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnStatusRemoved.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in CardManager.FireTriggersForCardPlayed based on the result of func.
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of a status effect being added.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnCardPlayed(this CharacterTriggerData.Trigger trigger, TriggerOnCardPlayedDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnCardPlayed.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in CardManager.DiscardCard if the discarded card was actually discarded as a result of a card and isn't an Infusion card.
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of a card being discarded.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnCardDiscarded(this CharacterTriggerData.Trigger trigger, TriggerOnCardDiscardedDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnCardDiscarded.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in CharacterState.OnOtherCharacterSpawned based on the result of func.
        /// 
        /// Note that the aforementioned function is called only when a allied unit is spawned.
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of another character being spawned</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnCharacterSpawned(this CharacterTriggerData.Trigger trigger, TriggerOnAnotherSpawnDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnAnotherSpawn.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in CharacterState.ApplyDamage based on the result of func.
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of another character being hit</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnCharacterHit(this CharacterTriggerData.Trigger trigger, TriggerOnCharacterHitDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnCharacterHit.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Sets the trigger to fire in RelicManager.ApplyTowerDamageTakenRelicEffects based on the result of func.
        /// </summary>
        /// <param name="trigger">A Custom CharacterTriggerData.Trigger, this function call is ignored on Vanilla Triggers.</param>
        /// <param name="func">Function to call to determine if the trigger should be fired as a result of the pyre taking damage.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger SetToTriggerOnPyreDamage(this CharacterTriggerData.Trigger trigger, TriggerOnPyreDamageDelegate func)
        {
            if (IsVanillaTrigger(trigger)) return trigger;
            TriggersOnPyreDamage.Add(trigger, func);
            return trigger;
        }

        /// <summary>
        /// Allows the CharacterTrigger to fire the StatusEffect TriggerStage PreCharacterTrigger. Only OnSilence/OnSilenceLost always triggers
        /// that specific trigger stage otherwise the unit must have a CharacterTrigger of the same type.
        /// 
        /// This function makes it so that if CharacterTrigger fires it always triggers the "PreCharacterTrigger" trigger stage status effects.
        /// </summary>
        /// <param name="trigger">A CharacterTriggerData.Trigger.</param>
        /// <returns>The trigger</returns>
        public static CharacterTriggerData.Trigger AllowTriggerToFirePreCharacterTriggerStatus(this CharacterTriggerData.Trigger trigger)
        {
            PreCharacterTriggerAllowedTriggers.Add(trigger);
            return trigger;
        }

        /// <summary>
        /// Makes this custom trigger an alias of another trigger type.
        /// </summary>
        /// <param name="trigger"></param>
        /// <returns></returns>
        public static CharacterTriggerData.Trigger AliasOfTriggerType(this CharacterTriggerData.Trigger customTrigger, CharacterTriggerData.Trigger otherTrigger)
        {
            if (IsVanillaTrigger(customTrigger)) return customTrigger;
            if (ReverseTriggerAliases.TryGetValue(customTrigger, out var aliases))
            {
                Plugin.Logger.LogWarning($"Attempt to alias an already aliased trigger, mapping this trigger to the triggers this trigger aliases.");
                Plugin.Logger.LogDebug($"{System.Environment.StackTrace}");
                List<CharacterTriggerData.Trigger> copy = [..aliases];
                ReverseTriggerAliases[customTrigger] = copy;
                foreach (CharacterTriggerData.Trigger trigger in copy)
                {
                    TriggerAliases[trigger]!.Add(customTrigger);
                }
                return customTrigger;
            }
            if (!TriggerAliases.ContainsKey(otherTrigger))
                TriggerAliases.Add(otherTrigger, []);
            TriggerAliases[otherTrigger].Add(customTrigger);
            if (!ReverseTriggerAliases.ContainsKey(customTrigger))
                ReverseTriggerAliases.Add(customTrigger, []);
            ReverseTriggerAliases[customTrigger].Add(otherTrigger);
            return customTrigger;
        }

        internal static void QueueCustomTrigger(this CombatManager combatManager, CharacterState character, CharacterTriggerData.Trigger trigger, QueueTriggerParams? data)
        {
            combatManager.QueueTrigger(character, trigger, dyingCharacter: data?.dyingCharacter, canAttackOrHeal: data?.canAttackOrHeal ?? true,
                                       canFireTriggers: data?.canFireTriggers ?? true, fireTriggersData: data?.fireTriggersData, triggerCount: data?.triggerCount ?? 1,
                                       exclusiveTrigger: data?.exclusiveTrigger);
        }
    }
}
