using Conductor.Extensions;
using Conductor.Interfaces;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(DeckScreen), "SortCards")]
    [HarmonyPriority(401)]
    public class DeckScreenPatches
    {
        public static void Postfix(ref List<DeckScreen.CardInfo> ___cardInfos, DeckScreen.Mode ___mode, AllGameManagers ___allGameManagers)
        {
            var cardEffect = DeckScreenExtensions.RequestingCardEffect;
            if (cardEffect == null || cardEffect is not ICardEffectWithDeckScreen sortDeckCardEffect)
                return;

            ___cardInfos = sortDeckCardEffect.SortDeck(___cardInfos, ___mode, ___allGameManagers.GetCoreManagers());
        }
    }

    [HarmonyPatch(typeof(DeckScreen), "UpdateSelected")]
    public class DeckScreen_UpdateSelected_Patch
    {
        public static void Postfix(List<DeckScreen.CardInfo> ___cardInfos, DeckScreen.Mode ___mode, List<int> ____selectedIndices, AllGameManagers ___allGameManagers)
        {
            var cardEffect = DeckScreenExtensions.RequestingCardEffect;
            if (cardEffect == null || cardEffect is not ICardEffectWithDeckScreen sortDeckCardEffect)
                return;

            sortDeckCardEffect.UpdateHighlight(___cardInfos, ____selectedIndices, ___mode, ___allGameManagers.GetCoreManagers());
        }
    }

    /*[HarmonyPatch(typeof(DeckScreen), "ReplayRecordSelectedCard")]
    public class DeckScreen_ReplayRecordSelectedCard_Patch
    {
        public static bool Prefix(List<DeckScreen.CardInfo> ___cardInfos, DeckScreen.Mode ___mode, List<int> ____selectedIndices, AllGameManagers ___allGameManagers)
        {
            var cardEffect = DeckScreenExtensions.RequestingCardEffect;
            if (cardEffect == null || cardEffect is not ICardEffectWithDeckScreen sortDeckCardEffect)
                return true;

            return !sortDeckCardEffect.ReplayRecordSelection(___cardInfos, ____selectedIndices, ___mode, ___allGameManagers.GetReplayManager(), ___allGameManagers.GetCoreManagers());
        }
    }*/

    [HarmonyPatch(typeof(DeckScreen), "Close")]
    public class DeckScreenPatches_ResetParams
    {
        public static void Postfix()
        {
            DeckScreenExtensions.RequestingCardEffect = null;
        }
    }
}
