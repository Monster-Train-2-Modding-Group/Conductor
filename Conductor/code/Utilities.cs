using HarmonyLib;
using System.Reflection;

namespace Conductor
{
    public static class Utilities
    {
        internal static FieldInfo TraitsSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "TraitsSupportedInTooltips");
        internal static FieldInfo StatesSupportedInTooltips = AccessTools.Field(typeof(TooltipContainer), "StatesSupportedInTooltips");

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
