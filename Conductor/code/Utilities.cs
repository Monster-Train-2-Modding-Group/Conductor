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

        internal static MethodInfo CreateAdditionalTooltips1 = AccessTools.Method(typeof(CardEffectBase), "CreateAdditionalTooltips",
            [typeof(CardEffectState), typeof(TooltipContainer), typeof(SaveManager)]);
        internal static MethodInfo CreateAdditionalTooltips2 = AccessTools.Method(typeof(CardEffectBase), "CreateAdditionalTooltips",
            [typeof(CardEffectData), typeof(List<TooltipContent>), typeof(SaveManager), typeof(CardState)]);

        internal static MethodInfo GetCardTooltipText = AccessTools.Method(typeof(CardTraitState), "GetCardTooltipText");

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
