using Conductor.UI;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(Hud), "Initialize")]
    public class HudPatches_Initialize
    {
        
        public static void Postfix(GameObject ___gameplayHudRoot)
        {
            var needsRebuild = false;
            var classMechanics = ___gameplayHudRoot.transform.Find("ClassMechanics");
            foreach (var ui in HudManager.ClassHUDs.Values)
            {
                ui.Initialize();
                ui.transform.SetParent(classMechanics);
                ui.gameObject.SetActive(false);
                needsRebuild = true;
            }
            if (needsRebuild)
                LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)classMechanics!.transform);
        }
    }

    [HarmonyPatch(typeof(Hud), "RefreshState")]
    public class HudPatches_RefreshState
    {
        internal static bool Initialized = false;
        public static void Postfix(SaveManager ___saveManager, PlayerManager ___playerManager, CardManager ___cardManager, ScreenManager ___screenManager)
        {
            foreach (var ui in HudManager.ClassHUDs.Values)
            {
                if (ui.ShouldShowUI(___saveManager, ___playerManager, ___cardManager, ___screenManager))
                {
                    ui.gameObject.SetActive(true);
                    ui.Refresh(___saveManager, ___playerManager, ___cardManager);
                }
                else
                {
                    ui.gameObject.SetActive(false);
                }
            }
        }
    }
}
