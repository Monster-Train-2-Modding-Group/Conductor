using HarmonyLib;
using System.Reflection;

namespace Conductor
{
    public static class Utilities
    {
        internal static FieldInfo TraitsSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "TraitsSupportedInTooltips");
        internal static FieldInfo StatesSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "StatesSupportedInTooltips");

        /// <summary>
        /// CardTraits have to be whitelisted to display a tooltip.
        /// </summary>
        /// <param name="assembly">Optional assembly to pass in. If not specified the caller's assembly is assumed</param>
        public static void SetupTraitTooltips(Assembly? assembly)
        {
            assembly = assembly ?? Assembly.GetCallingAssembly();
            List<string> cardTraitNames = [];
            foreach (var type in assembly.GetTypes())
            {
                // CardTraits that have a tooltip.
                if (type.IsSubclassOf(typeof(CardTraitState)))
                {
                    bool needsATooltip = AccessTools.Method(type, "GetCardTooltipText").DeclaringType == type;
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
        /// CardEffects have to be whitelisted to display a tooltip.
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
                    bool needsATooltip = AccessTools.Method(type, "CreateAdditionalTooltips").DeclaringType == type;
                    if (needsATooltip)
                    {
                        cardTraitNames.Add(type.AssemblyQualifiedName);
                    }
                }
            }
            var states = (List<string>)StatesSupportedInTooltips.GetValue(null);
            states.AddRange(cardTraitNames);
        }
    }
}
