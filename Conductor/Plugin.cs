using BepInEx;
using BepInEx.Logging;
using Conductor.Triggers;
using HarmonyLib;
using System.Reflection;
using Conductor.Extensions;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;
using Conductor.TargetModes;
using Conductor.TrackedValues;
using Conductor.Data.Processors;
using Conductor.Data.Registers;

namespace Conductor
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);

        public static void Log(string message)
        {
            var timestamp = DateTime.UtcNow.ToString("HH:mm:ss.ffffff");
            Logger.LogError($"[{timestamp}] {message}");
        }

        public void Awake()
        {
            // Plugin startup logic
            Logger = base.Logger;
            var builder = Railhead.GetBuilder();
            builder.Configure(
                MyPluginInfo.PLUGIN_GUID,
                c =>
                {
                    c.AddMergedJsonFile(
                        "json/status_effects/divine_blessing.json",
                        "json/status_effects/growth.json",
                        "json/status_effects/heroic.json",
                        "json/status_effects/hex.json",
                        "json/status_effects/intangible.json",
                        "json/status_effects/smirk.json",
                        "json/status_effects/construct.json",
                        //"json/status_effects/test.json",
                        "json/status_effects/other_sprites.json",
                        //"json/status_effects/curse.json",
                        "json/target_modes.json",
                        "json/traits.json",
                        "json/event_triggers.json",
                        "json/triggers.json",
                        "json/tracked_values.json",
                        "json/room_modifiers.json"
                        //,"json/test.json"
                        //,"json/test2.json"
                    );
                }
            );

            Railend.ConfigurePreAction(
                c =>
                {
                    c.RegisterSingleton<UnitEssenceProcessor, UnitEssenceProcessor>();
                    c.RegisterSingleton<UnitEssenceRegistry, UnitEssenceRegistry>();
                }
            );

            Railend.ConfigurePostAction(
                c =>
                {
                    // Parse Essences.
                    var finalizer = c.GetInstance<UnitEssenceProcessor>();
                    finalizer.Run();

                    // Wire Implementations of CharacterTriggers
                    var manager = c.GetInstance<IRegister<CharacterTriggerData.Trigger>>();
                    CharacterTriggerData.Trigger GetTrigger(string id)
                    {
                        return manager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.CharacterTriggerEnum, id));
                    }
                    CharacterTriggers.Vengeance = GetTrigger("Vengeance").SetToTriggerOnCharacterHit(CharacterTriggers.OnAlliedCharacterHit);
                    CharacterTriggers.FollowUp = GetTrigger("FollowUp").SetToTriggerOnCharacterHit(CharacterTriggers.OnOpposingCharacterHitByDirectAttack);
                    CharacterTriggers.Junk = GetTrigger("Junk").SetToTriggerOnCardDiscarded(CharacterTriggers.OnDiscardedAnyCard);
                    CharacterTriggers.Penance = GetTrigger("Penance").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedBlightOrScourge);
                    CharacterTriggers.Accursed = GetTrigger("Accursed").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedBlightOrScourge).SetToTriggerOnCardDiscarded(CharacterTriggers.OnDiscardedBlightOrScourge);
                    CharacterTriggers.Resonance = GetTrigger("Resonance").SetToTriggerOnPyreDamage(CharacterTriggers.OnPyreTakeDamage);
                    CharacterTriggers.Evoke = GetTrigger("Evoke").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedUnitAbility);
                    CharacterTriggers.OnBuffed = GetTrigger("OnBuffed").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedABuff).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnDebuffed = GetTrigger("OnDebuffed").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedADebuff).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnGrowthGained = GetTrigger("OnGrowthGained").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedGrowth).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnGrowthLost = GetTrigger("OnGrowthLost").SetToTriggerOnStatusEffectRemoved(CharacterTriggers.OnLostGrowth).AllowTriggerToFirePreCharacterTriggerStatus();
                    // Implementations of Mobilize/Encounter is in SpawnBumpTriggerPatches.cs
                    CharacterTriggers.Mobilize = GetTrigger("Mobilize");
                    CharacterTriggers.Encounter = GetTrigger("Encounter");


                    // Setup Card Triggers.
                    var triggerManager = c.GetInstance<IRegister<CardTriggerType>>();
                    CardTriggerType GetCardTrigger(string id)
                    {
                        return triggerManager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.CardTriggerEnum, id));
                    }
                    // Implementation is in DiscardBasedTriggersPatch.cs
                    CardTriggers.Junk = GetCardTrigger("Junk");


                    // Set sprites for abandoned tech
                    var spriteManager = c.GetInstance<IRegister<Sprite>>();
                    var iconField = AccessTools.Field(typeof(StatusEffectData), "icon");
                    Sprite? GetSprite(string id)
                    {
                        return spriteManager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.Sprite, id));
                    }
                    var piercing = StatusEffectManager.Instance.GetStatusEffectDataById("piercing");
                    if (piercing != null && piercing.GetIcon() == null)
                    {
                        iconField.SetValue(piercing, GetSprite("Piercing"));
                    }
                    var sniper = StatusEffectManager.Instance.GetStatusEffectDataById("sniper");
                    if (sniper != null && sniper.GetIcon() == null)
                    {
                        iconField.SetValue(sniper, GetSprite("Sniper"));
                    }


                    // Target Mode implementation wiring.
                    var targetModeRegister = c.GetInstance<IRegister<TargetMode>>();
                    TargetMode GetTargetMode(string id)
                    {
                        return targetModeRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.TargetModeEnum, id));
                    }
                    GetTargetMode("played_card").SetTargetModeSelector(new PlayedCard());
                    GetTargetMode("override_target_character").SetTargetModeSelector(new OverrideTargetCharacter());
                    GetTargetMode("in_front_of_self").SetTargetModeSelector(new InFrontOfSelf());
                    GetTargetMode("behind_self").SetTargetModeSelector(new BehindSelf());
                    GetTargetMode("around_self").SetTargetModeSelector(new AroundSelf());
                    GetTargetMode("strongest").SetTargetModeSelector(new Strongest());
                    GetTargetMode("highest_attack").SetTargetModeSelector(new HighestAttack());
                    GetTargetMode("lowest_attack").SetTargetModeSelector(new LowestAttack());
                    GetTargetMode("highest_attack_all_rooms").SetTargetModeSelector(new HighestAttackAllRooms());
                    GetTargetMode("lowest_attack_all_rooms").SetTargetModeSelector(new LowestAttackAllRooms());
                    GetTargetMode("highest_attack_excluding_self").SetTargetModeSelector(new HighestAttackExcludingSelf());


                    // TrackedValue implementation wiring.
                    var trackedValueRegister = c.GetInstance<IRegister<CardStatistics.TrackedValueType>>();
                    CardStatistics.TrackedValueType GetTrackedValueType(string id)
                    {
                        return trackedValueRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.TrackedValueTypeEnum, id));
                    }
                    GetTrackedValueType("BlightsAndScourgesInDeck").SetIsValidOutsideBattle().SetTrackedValueGetter(TrackedValueFunctions.CountBlightsAndScourgesInDeck);


                    // Status Effect Trigger Stages
                    var triggerStageRegister = c.GetInstance<IRegister<StatusEffectData.TriggerStage>>();
                    StatusEffectData.TriggerStage GetTriggerStage(string id)
                    {
                        return triggerStageRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.StatusEffectTriggerStageEnum, id));
                    }
                    StatusEffectTriggerStages.OnShift = GetTriggerStage("on_shift");
                }
            );

            Utilities.SetupTraitTooltips(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
}
