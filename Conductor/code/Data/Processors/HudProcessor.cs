using Conductor.Data.Registers;
using Conductor.Interfaces;
using Conductor.UI;
using HarmonyLib;
using Microsoft.Extensions.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Xml.Linq;
using TrainworksReloaded.Base;
using TrainworksReloaded.Base.Extensions;
using TrainworksReloaded.Base.Localization;
using TrainworksReloaded.Core.Extensions;
using TrainworksReloaded.Core.Impl;
using TrainworksReloaded.Core.Interfaces;
using UnityEngine;

namespace Conductor.Data.Processors
{
    internal class HudProcessor
    {
        PluginAtlas atlas;
        IRegister<ClassData> classRegister;
        IRegister<Sprite> spriteRegister;
        IRegister<LocalizationTerm> termRegister;
        
        readonly Dictionary<string, ClassMechanicHud.HudType> stringToHudType = new()
        {
            ["invalid"] = ClassMechanicHud.HudType.Invalid,
            ["small"] = ClassMechanicHud.HudType.Small,
            ["large"] = ClassMechanicHud.HudType.Large,
            ["custom"] = ClassMechanicHud.HudType.Custom,
        };

        readonly Dictionary<string, ClassMechanicHud.HudDisplayMode> stringToDisplayType = new()
        {
            ["invalid"] = ClassMechanicHud.HudDisplayMode.Invalid,
            ["battle_only"] = ClassMechanicHud.HudDisplayMode.BattleOnly,
            ["run"] = ClassMechanicHud.HudDisplayMode.Run,
        };

        public HudProcessor(PluginAtlas atlas, IRegister<ClassData> classRegister, IRegister<Sprite> spriteRegister, IRegister<LocalizationTerm> termRegister)
        {
            this.atlas = atlas;
            this.spriteRegister = spriteRegister;
            this.termRegister = termRegister;
            this.classRegister = classRegister;
        }

        public void Run()
        {
            foreach (var pluginDef in atlas.PluginDefinitions)
            {
                var key = pluginDef.Key;
                var config = pluginDef.Value.Configuration;
                foreach (var hudConfig in config.GetSection("huds").GetChildren())
                {
                    ProcessHUD(key, hudConfig);
                }
            }
        }

        private bool GetTypeSubclassingClass<T>(string className, Assembly? assembly, [NotNullWhen(true)] out Type? typeSubclassingType)
        {
            className = className.Replace("@", "");
            Type? type = null;
            typeSubclassingType = null;
            if (assembly != null)
            {
                type = assembly.FindTypeByClassName(className);
            }
            if (type != null && typeof(T).IsAssignableFrom(type))
            {
                typeSubclassingType = type;
                return true;
            }
            return false;
        }

        private void ProcessHUD(string key, IConfiguration config)
        {
            
            var id = config.GetSection("id").ParseString();
            if (id == null)
                return;

            Plugin.Logger.LogInfo($"Creating HUD {key}/{id}");

            var typeStr = config.GetSection("type").ParseString()?.ToLower();
            var type = typeStr == null ? ClassMechanicHud.HudType.Custom : stringToHudType.GetValueOrDefault(typeStr, ClassMechanicHud.HudType.Invalid);

            var displayStr = config.GetSection("display").ParseString()?.ToLower();
            var displayMode = displayStr == null ? ClassMechanicHud.HudDisplayMode.BattleOnly : stringToDisplayType.GetValueOrDefault(displayStr, ClassMechanicHud.HudDisplayMode.Invalid);

            Type? UIClass = typeof(ClassMechanicHud);
            var classReference = config.GetSection("name").ParseReference();
            if (classReference != null)
            {
                var baseClassName = classReference.id;
                var modReference = classReference.mod_reference ?? key;
                var assembly = atlas.PluginDefinitions.GetValueOrDefault(modReference)?.Assembly;
                if (!GetTypeSubclassingClass<ClassMechanicHud>(baseClassName, assembly, out UIClass))
                {
                    Plugin.Logger.LogError($"Failed to load HUD class {baseClassName} in {id} mod {modReference}, Make sure the class exists in {modReference} and that the class inherits from ClassMechanicHudUI.");
                    return;
                }
            }

            var clanReference = config.GetSection("class").ParseReference();
            ClassData? clan = null;
            if (clanReference != null)
            {
                classRegister.TryLookupName(clanReference.ToId(key, TemplateConstants.Class), out var foundClan, out var _, clanReference.context);
                clan = foundClan;
            }

            var backgroundReference = config.GetSection("background").ParseReference();
            Sprite? background = null;
            if (backgroundReference != null)
            {
                spriteRegister.TryLookupName(backgroundReference.ToId(key, TemplateConstants.Sprite), out var sprite, out var _, backgroundReference.context);
                background = sprite;
            }

            var spriteReferences = config.GetSection("icons").GetChildren().Select(x => x.ParseReference()).ToList();
            Sprite[] icons = new Sprite[spriteReferences.Count];
            for (int i = 0; i < icons.Length; i++)
            {
                var spriteReference = spriteReferences[i];
                spriteRegister.TryLookupName(spriteReference?.ToId(key, TemplateConstants.Sprite) ?? "", out var sprite, out var _, spriteReference?.context);
                icons[i] = sprite!;
            }

            var titles = config.GetSection("tooltip_titles").ParseLocalizationTerm();
            if (titles != null)
            {
                titles.Key = $"HudTooltip_{key}_{id}_Title";
                termRegister.Register(titles.Key, titles);
            }

            var texts = config.GetSection("tooltip_texts").ParseLocalizationTerm();
            if (texts != null)
            {
                texts.Key = $"HudTooltip_{key}_{id}_Body";
                termRegister.Register(texts.Key, texts);
            }

            var labelTexts = config.GetSection("label_texts").ParseLocalizationTerm();
            if (labelTexts != null)
            {
                labelTexts.Key = $"HudTooltip_{key}_{id}_Text";
                termRegister.Register(labelTexts.Key, labelTexts);
            }

            var additionalLabels = config.GetSection("additional_texts").GetChildren().Select(x => x.ParseLocalizationTerm()).Where(x => x != null).ToList();
            string[]? additionalLabelKeys = additionalLabels.Count == 0 ? null : new string[additionalLabels.Count];
            for (int i = 0; i < additionalLabels.Count; i++) 
            {
                additionalLabels[i]!.Key = $"HudTooltip_{key}_{id}{i+2}_Text";
                termRegister.Register(additionalLabels[i]!.Key, additionalLabels[i]!);
                additionalLabelKeys![i] = additionalLabels[i]!.Key;
            }

            if (UIClass == null)
            {
                Plugin.Logger.LogWarning($"UIClass is null, this shouldn't happen, defaulting it back to ClassMechanicHud, if name is specified then this is actionable.");
                UIClass = typeof(ClassMechanicHud);
            }
            if (background == null)
            {
                Plugin.Logger.LogError("Background is null, background is a required param.");
                return;
            }

            var object_ui = ClassMechanicHud.ConstructGameObject(id, UIClass, type, displayMode, clan, background, icons, titles?.Key, texts?.Key, labelTexts?.Key, additionalLabelKeys);
            if (object_ui != null)
            {
                HudManager.AddHUD(key, id, object_ui.Value.Item2);
            }
        }
    }
}
