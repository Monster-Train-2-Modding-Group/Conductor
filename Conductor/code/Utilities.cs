using Conductor.RoomModifiers;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace Conductor
{
    public static class Utilities
    {
        internal static FieldInfo TraitsSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "TraitsSupportedInTooltips");
        internal static FieldInfo StatesSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "StatesSupportedInTooltips");
        internal static FieldInfo StatusEffectManager_displayDataField = AccessTools.Field(typeof(StatusEffectManager), "displayData");
        internal static FieldInfo StatusEffectsDisplayData_cardEffectDisplayDataField = AccessTools.Field(typeof(StatusEffectsDisplayData), "cardEffectDisplayData");
        internal static FieldInfo CardUpgradeMask_excludedCardEffectsField = AccessTools.Field(typeof(CardUpgradeMaskData), "excludedCardEffects");
        internal static FieldInfo CardUpgradeMask_requiredCardEffectsField = AccessTools.Field(typeof(CardUpgradeMaskData), "requiredCardEffects");

        internal static MethodInfo CreateAdditionalTooltips1 = AccessTools.Method(typeof(CardEffectBase), "CreateAdditionalTooltips",
            [typeof(CardEffectState), typeof(TooltipContainer), typeof(SaveManager)]);
        internal static MethodInfo CreateAdditionalTooltips2 = AccessTools.Method(typeof(CardEffectBase), "CreateAdditionalTooltips",
            [typeof(CardEffectData), typeof(List<TooltipContent>), typeof(SaveManager), typeof(CardState)]);
        internal static MethodInfo GetCardTooltipText = AccessTools.Method(typeof(CardTraitState), "GetCardTooltipText");

        internal static Dictionary<string, CardUpgradeMaskData> VanillaFilters = [];

        private static Lazy<List<string>> DrawSpellOnDeploymentExcludedCardEffects = 
            new(static () => { return GetCardEffectsFromFilter("DrawSpellOnDeployment_CardMask", false); });
        private static Lazy<List<string>> OnlyDamageCardRequiredCardEffects =
            new(static () => { return GetCardEffectsFromFilter("OnlyDamageCard"); });
        private static Lazy<List<string>> OnlyDamageCardExcludeSpellsFromMagicPowerRequiredCardEffects =
            new(static () => { return GetCardEffectsFromFilter("OnlyDamageCardExcludeSpellsFromMagicPower"); });
        private static Lazy<List<string>> MagicPowerKillSacrificeSpellsRequiredCardEffects =
            new(static () => { return GetCardEffectsFromFilter("MagicPowerKillSacrificeSpells"); });
        private static Lazy<List<string>> MagicPowerSpellsRequiredCardEffects =
            new(static () => { return GetCardEffectsFromFilter("MagicPowerSpells"); });
        private static Lazy<List<string>> OnlyStatusEffectSettingRequiredCardEffects =
            new(static () => { return GetCardEffectsFromFilter("OnlyStatusEffectSetting"); });

        private static List<string> GetCardEffectsFromFilter(string name, bool required = true)
        {
            var filter = VanillaFilters.GetValueOrDefault(name);
            if (filter == null)
            {
                Plugin.Logger.LogWarning("Could not find filter " + name);
                return [];
            }

            List<string>? ret;
            if (required)
            {
                ret = CardUpgradeMask_requiredCardEffectsField.GetValue(filter) as List<string>;
            }
            else
            {
                ret = CardUpgradeMask_excludedCardEffectsField.GetValue(filter) as List<string>;
            }

            if (ret == null)
            {
                Plugin.Logger.LogWarning("Could not find filter's list " + name);
            }
            return ret ?? [];
        }

        /// <summary>
        /// Mark this effect as a damaging effect.
        /// 
        /// This helper function marks a Custom CardEffect as a damaging effect.
        /// This adds the card effect to CardUpgradeMasks that filter for damaging effects.
        /// 
        /// This function should be called in your call to Railend.ConfigurePostAction.
        /// </summary>
        /// <param name="cardEffectClass">Type that is a subclass of CardEffectBase</param>
        public static void MarkEffectAsADamageEffect(Type cardEffectClass)
        {
            if (!cardEffectClass.IsSubclassOf(typeof(CardEffectBase)))
            {
                return;
            }
            RoomStateDamageSpellCostModifier.DamagingCardEffects.Add(cardEffectClass);

            string fullyQualifiedType = cardEffectClass.AssemblyQualifiedName;
            DrawSpellOnDeploymentExcludedCardEffects.Value.Add(fullyQualifiedType);
            OnlyDamageCardRequiredCardEffects.Value.Add(fullyQualifiedType);
            OnlyDamageCardExcludeSpellsFromMagicPowerRequiredCardEffects.Value.Add(fullyQualifiedType);
            MagicPowerKillSacrificeSpellsRequiredCardEffects.Value.Add(fullyQualifiedType);
            MagicPowerSpellsRequiredCardEffects.Value.Add(fullyQualifiedType);
        }

        /// <summary>
        /// Mark this effect as a healing effect.
        /// 
        /// This helper function marks a Custom CardEffect as a healing effect.
        /// This adds the card effect to CardUpgradeMasks that filter for heal effects.
        /// 
        /// This function should be called in your call to Railend.ConfigurePostAction.
        /// </summary>
        /// <param name="cardEffectClass">Type that is a subclass of CardEffectBase</param>
        public static void MarkEffectAsHealEffect(Type cardEffectClass)
        {
            RoomStateHealSpellCostModifier.OtherHealingCardEffects.Add(cardEffectClass);
            string fullyQualifiedType = cardEffectClass.AssemblyQualifiedName;
            MagicPowerKillSacrificeSpellsRequiredCardEffects.Value.Add(fullyQualifiedType);
            MagicPowerSpellsRequiredCardEffects.Value.Add(fullyQualifiedType);
        }

        /// <summary>
        /// Mark this effect as a status effect giving effect.
        /// 
        /// This helper function marks a Custom CardEffect as a status giving effect.
        /// This adds the card effect to CardUpgradeMasks that filter for effects that give status effects.
        /// 
        /// This function should be called in your call to Railend.ConfigurePostAction.
        /// </summary>
        /// <param name="cardEffectClass">Type that is a subclass of CardEffectBase</param>
        public static void MarkEffectAsStatusGivingEffect(Type cardEffectClass)
        {
            string fullyQualifiedType = cardEffectClass.AssemblyQualifiedName;
            OnlyStatusEffectSettingRequiredCardEffects.Value.Add(fullyQualifiedType);
        }

        /// <summary>
        /// If a CardTraitState subclass defines GetCardTooltipText it does not display automatically when on a card.
        /// CardTraits have to be whitelisted to display a tooltip. This function enables that.
        /// </summary>
        /// <param name="assembly">Optional assembly to pass in. If not specified the caller's assembly is assumed</param>
        public static void SetupTraitTooltips(Assembly? assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            List<string> cardTraitNames = [];
            foreach (var type in assembly.GetTypes())
            {
                // CardTraits that have a tooltip.
                if (type.IsSubclassOf(typeof(CardTraitState)))
                {
                    bool needsATooltip = HasOverride(type, GetCardTooltipText);
                    if (needsATooltip)
                    {
                        cardTraitNames.Add(type.Name);
                        cardTraitNames.Add(type.AssemblyQualifiedName);
                    }
                }
            }
            var traits = (HashSet<string>)TraitsSupportedInTooltips.GetValue(null);
            traits.UnionWith(cardTraitNames);
        }

        /// <summary>
        /// If a CardEffectBase subclass defines CreateAdditionalTooltips it won't display automatically when on a card.
        /// CardEffects have to be whitelisted to display a tooltip. This function enables that.
        /// 
        /// </summary>
        /// <param name="assembly">Optional assembly to pass in. If not specified the caller's assembly is assumed</param>
        public static void SetupCardEffectTooltips(Assembly? assembly = null)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            List<string> cardTraitNames = [];
            foreach (var type in assembly.GetTypes())
            {
                // CardEffects that have a tooltip.
                if (type.IsSubclassOf(typeof(CardEffectBase)))
                {
                    bool needsATooltip = HasOverride(type, CreateAdditionalTooltips1) || HasOverride(type, CreateAdditionalTooltips2);
                    if (needsATooltip)
                    {
                        cardTraitNames.Add(type.AssemblyQualifiedName);
                    }
                }
            }
            var states = (List<string>)StatesSupportedInTooltips.GetValue(null);
            states.AddRange(cardTraitNames);
        }

        /// <summary>
        /// Overrides the Trigger Icon of a particular special CardEffect.
        /// Examples of this in the Vanilla game are:
        ///   Healers, PostBattleHealing trigger w/ CardEffectHeal
        ///   Enchanters AfterSpawnEnchant w/ CardEffectEnchant.
        /// </summary>
        /// <param name="cardEffectClass">Type of a Custom Card Effect class. Must be a Custom Card Effect class.</param>
        /// <param name="icon">Sprite to use</param>
        /// <param name="displayCategory">DisplayCategory for the trigger.</param>
        /// <param name="colorType">ColorType for the trigger.</param>
        public static void AddCardEffectDisplay(Type cardEffectClass, Sprite icon, CharacterTriggerData.DisplayCategory displayCategory, ColorDisplayData.ColorType colorType)
        {
            StatusEffectManager manager = StatusEffectManager.Instance;
            var displayData = StatusEffectManager_displayDataField.GetValue(manager) as StatusEffectsDisplayData;
            var cardEffectDictionary = StatusEffectsDisplayData_cardEffectDisplayDataField.GetValue(displayData) as StatusEffectsDisplayData.CardEffectDictionary;
            cardEffectDictionary!.Add(cardEffectClass.AssemblyQualifiedName, new StatusEffectsDisplayData.CardEffectDisplayData()
            {
                icon = icon,
                displayCategory = displayCategory,
                colorType = colorType
            });
        }

        internal static bool HasOverride(Type type, MethodInfo baseMethod)
        {
            if (baseMethod == null) 
                return false;
            var method = type.GetMethod(
                baseMethod.Name,
                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                null,
                baseMethod.GetParameters().Select(p => p.ParameterType).ToArray(),
                null
            );
            return method != null && method.DeclaringType != baseMethod.DeclaringType;
        }
    }
}
