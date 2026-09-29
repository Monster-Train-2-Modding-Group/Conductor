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
using Conductor.CardEffects;
using MonoMod.Utils;
using Conductor.Enums;
using TrainworksReloaded.Base.Enums;

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
        private void Initialize()
        {
            Utilities.VanillaFilters.AddRange(Resources.FindObjectsOfTypeAll<CardUpgradeMaskData>().ToDictionary(x => x.name, x => x));
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
                        "json/plugin.json",
                        "json/status_effects/brambles.json",
                        "json/status_effects/construct.json",
                        "json/status_effects/divine_blessing.json",
                        "json/status_effects/growth.json",
                        "json/status_effects/heroic.json",
                        "json/status_effects/hex.json",
                        "json/status_effects/intangible.json",
                        //"json/status_effects/siege.json",
                        //"json/status_effects/portal.json",
                        "json/status_effects/pierce.json",
                        "json/status_effects/recoil.json",
                        "json/status_effects/smirk.json",
                        
                        //"json/status_effects/test.json",
                        "json/status_effects/other_sprites.json",
                        //"json/status_effects/curse.json",
                        "json/target_modes.json",
                        "json/traits.json",
                        "json/event_triggers.json",
                        "json/triggers.json",
                        "json/tracked_values.json",
                        "json/status_effect_trigger_stages.json"
                        //,"json/test.json"
                        //,"json/test2.json"
                    );
                }
            );

            Railend.ConfigurePreAction(
                c =>
                {
                    Initialize();
                    c.RegisterSingleton<UnitEssenceProcessor, UnitEssenceProcessor>();
                    c.RegisterSingleton<HudProcessor, HudProcessor>();
                    c.RegisterSingleton<UnitEssenceRegistry, UnitEssenceRegistry>();
                }
            );

            Railend.ConfigurePostAction(
                c =>
                {
                    // Parse Essences.
                    c.GetInstance<UnitEssenceProcessor>().Run();
                    c.GetInstance<HudProcessor>().Run();

                    // Wire Implementations of CharacterTriggers
                    var manager = c.GetInstance<IRegister<CharacterTriggerData.Trigger>>();
                    CharacterTriggerData.Trigger GetTrigger(string id)
                    {
                        return manager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.CharacterTriggerEnum, id));
                    }
                    CharacterTriggers.Absolve = GetTrigger("Absolve").SetToTriggerOnCardPurged(CharacterTriggers.OnPurgedBlightOrScourge);
                    CharacterTriggers.Vanish = GetTrigger("Vanish").SetToTriggerOnCardPlayed(CharacterTriggers.OnEphemeralCardPlayed).SetToTriggerOnCardDiscarded(CharacterTriggers.OnEphemeralCardDiscarded);
                    CharacterTriggers.Vengeance = GetTrigger("Vengeance").SetToTriggerOnCharacterHit(CharacterTriggers.OnAlliedCharacterHit);
                    CharacterTriggers.FollowUp = GetTrigger("FollowUp").SetToTriggerOnCharacterHit(CharacterTriggers.OnOpposingCharacterHitByDirectAttack);
                    CharacterTriggers.Junk = GetTrigger("Junk").SetToTriggerOnCardDiscarded(CharacterTriggers.OnDiscardedAnyCard);
                    CharacterTriggers.Furnish = GetTrigger("Furnish").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedRoom);
                    CharacterTriggers.Penance = GetTrigger("Penance").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedBlightOrScourge);
                    CharacterTriggers.Accursed = GetTrigger("Accursed").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedBlightOrScourge).SetToTriggerOnCardDiscarded(CharacterTriggers.OnDiscardedBlightOrScourge);
                    CharacterTriggers.Resonance = GetTrigger("Resonance").SetToTriggerOnPyreDamage(CharacterTriggers.OnPyreTakeDamage);
                    CharacterTriggers.Evoke = GetTrigger("Evoke").SetToTriggerOnCardPlayed(CharacterTriggers.OnPlayedUnitAbility);
                    CharacterTriggers.AfterSpawnBetterEnchant = GetTrigger("AfterSpawnBetterEnchant").AliasOfTriggerType(CharacterTriggerData.Trigger.AfterSpawnEnchant);
                    CharacterTriggers.OnBuffed = GetTrigger("OnBuffed").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedABuff).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnDebuffed = GetTrigger("OnDebuffed").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedADebuff).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnGrowthGained = GetTrigger("OnGrowthGained").SetToTriggerOnStatusEffectAdded(CharacterTriggers.OnGainedGrowth).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.OnGrowthLost = GetTrigger("OnGrowthLost").SetToTriggerOnStatusEffectRemoved(CharacterTriggers.OnLostGrowth).AllowTriggerToFirePreCharacterTriggerStatus();
                    CharacterTriggers.Binder = GetTrigger("Binder");

                    // Implementation of Encounter is in SpawnBumpTriggerPatches.cs
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
                    var steelmight = StatusEffectManager.Instance.GetStatusEffectDataById("armorattack");
                    if (steelmight != null && steelmight?.GetIcon()?.name == "StatusBlessing")
                    {
                        iconField.SetValue(steelmight, GetSprite("Steelmight"));
                    }

                    // Trigger Displays
                    Utilities.AddCardEffectDisplay(typeof(Conductor.CardEffects.CardEffectEnchant), GetSprite("trigger_enchant")!, CharacterTriggerData.DisplayCategory.StateModifier, ColorDisplayData.ColorType.StateModifier);

                    // Target Mode implementation wiring.
                    var targetModeRegister = c.GetInstance<IRegister<TargetMode>>();
                    TargetMode GetTargetMode(string id)
                    {
                        return targetModeRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.TargetModeEnum, id));
                    }

                    Enums.TargetModes.PlayedCard = GetTargetMode("played_card").SetTargetModeSelector(new PlayedCard());
                    Enums.TargetModes.OverrideTargetCharacter = GetTargetMode("override_target_character").SetTargetModeSelector(new OverrideTargetCharacter());
                    Enums.TargetModes.InFrontOfSelf = GetTargetMode("in_front_of_self").SetTargetModeSelector(new InFrontOfSelf());
                    Enums.TargetModes.BehindSelf = GetTargetMode("behind_self").SetTargetModeSelector(new BehindSelf());
                    Enums.TargetModes.AroundSelf = GetTargetMode("around_self").SetTargetModeSelector(new AroundSelf());
                    Enums.TargetModes.Strongest = GetTargetMode("strongest").SetTargetModeSelector(new Strongest());
                    Enums.TargetModes.HighestAttack = GetTargetMode("highest_attack").SetTargetModeSelector(new HighestAttack());
                    Enums.TargetModes.LowestAttack = GetTargetMode("lowest_attack").SetTargetModeSelector(new LowestAttack());
                    Enums.TargetModes.HighestAttackAllRooms = GetTargetMode("highest_attack_all_rooms").SetTargetModeSelector(new HighestAttackAllRooms());
                    Enums.TargetModes.LowestAttackAllRooms = GetTargetMode("lowest_attack_all_rooms").SetTargetModeSelector(new LowestAttackAllRooms());
                    Enums.TargetModes.HigestAttackExcludingSelf = GetTargetMode("highest_attack_excluding_self").SetTargetModeSelector(new HighestAttackExcludingSelf());
                    Enums.TargetModes.NUnits = GetTargetMode("n-units").SetTargetModeSelector(new NUnits());

                    // TrackedValue implementation wiring.
                    var trackedValueRegister = c.GetInstance<IRegister<CardStatistics.TrackedValueType>>();
                    CardStatistics.TrackedValueType GetTrackedValueType(string id)
                    {
                        return trackedValueRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.TrackedValueTypeEnum, id));
                    }
                    GetTrackedValueType("BlightsAndScourgesInDeck").SetIsValidOutsideBattle().SetTrackedValueGetter(TrackedValueFunctions.CountBlightsAndScourgesInDeck);
                    GetTrackedValueType("NumUnitsInTargetRoom").SetTrackedValueGetter(TrackedValueFunctions.CountUnitsInTargetRoom);
                    GetTrackedValueType("NumUnitsOnTrain").SetTrackedValueGetter(TrackedValueFunctions.CountUnitsOnTrain);

                    // Status Effect Trigger Stages
                    var triggerStageRegister = c.GetInstance<IRegister<StatusEffectData.TriggerStage>>();
                    StatusEffectData.TriggerStage GetTriggerStage(string id)
                    {
                        return triggerStageRegister.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.StatusEffectTriggerStageEnum, id));
                    }
                    StatusEffectTriggerStages.OnShift = GetTriggerStage("on_shift");

                    // Fix dormant
                    var dormant = StatusEffectManager.Instance.GetStatusEffectDataById("dormant");
                    AccessTools.Field(typeof(StatusEffectData), "triggerStage").SetValue(dormant, StatusEffectData.TriggerStage.OnPreCharacterTrigger);

                    // Additional Setup
                    Utilities.SetupTraitTooltips(Assembly.GetExecutingAssembly());
                    Utilities.SetupCardEffectTooltips(Assembly.GetExecutingAssembly());
                    Utilities.MarkEffectAsStatusGivingEffect(typeof(CardEffectAddStatusEffectUpToMaximum));

                }
            );

            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");

            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
}

