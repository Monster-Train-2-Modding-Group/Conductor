using HarmonyLib;
using Ink.Runtime;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(InkStateInspector), nameof(InkStateInspector.BindInkMethods))]
    public class InkStateInspector_BindMoreExternalFunctionsPatch
    {
        public static void Postfix(Story inkStory)
        {
            foreach ((var name, var func) in Utilities.InkExternalFunctions)
            {
                inkStory.TryUnbindExternalFunction(name);
                inkStory.BindExternalFunctionGeneral(name, func);
            }
        }
    }
}
