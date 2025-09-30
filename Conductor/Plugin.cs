using BepInEx;
using BepInEx.Logging;
using Conductor.Triggers;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Core;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Interfaces;
using static CharacterTriggerData;

namespace Conductor
{
    [BepInPlugin(MyPluginInfo.PLUGIN_GUID, MyPluginInfo.PLUGIN_NAME, MyPluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        internal static new ManualLogSource Logger = new(MyPluginInfo.PLUGIN_GUID);

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
                        "json/status_effects/hex.json",
                        "json/status_effects/intangible.json",
                        "json/status_effects/smirk.json",
                        "json/status_effects/construct.json",
                        //"json/status_effects/curse.json",
                        //"json/target_modes.json",
                        "json/traits.json",
                        "json/event_triggers.json",
                        "json/triggers.json",
                        "json/room_modifiers.json"
                        //,"json/test.json"
                        //,"json/test2.json"
                    );
                }
            );

            Railend.ConfigurePostAction(
                c =>
                {
                    var manager = c.GetInstance<IRegister<CharacterTriggerData.Trigger>>();
                    var triggerManager = c.GetInstance<IRegister<CardTriggerType>>();

                    CharacterTriggerData.Trigger GetTrigger(string id)
                    {
                        return manager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.CharacterTriggerEnum, id));
                    }

                    CardTriggerType GetCardTrigger(string id)
                    {
                        return triggerManager.GetValueOrDefault(MyPluginInfo.PLUGIN_GUID.GetId(TemplateConstants.CardTriggerEnum, id));
                    }

                    CharacterTriggers.Vengeance = GetTrigger("Vengeance");
                    CharacterTriggers.Junk = GetTrigger("Junk");
                    CharacterTriggers.Encounter = GetTrigger("Encounter");
                    CharacterTriggers.Penance = GetTrigger("Penance");
                    CharacterTriggers.Accursed = GetTrigger("Accursed");
                    CharacterTriggers.Evoke = GetTrigger("Evoke");

                    CharacterTriggers.OnBuffed = GetTrigger("OnBuffed");
                    CharacterTriggers.OnDebuffed = GetTrigger("OnDebuffed");

                    CardTriggers.Junk = GetCardTrigger("Junk");
                }
            );
            Utilities.SetupTraitTooltips(Assembly.GetExecutingAssembly());
            Logger.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} is loaded!");


            var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
            harmony.PatchAll();
        }
    }
}
