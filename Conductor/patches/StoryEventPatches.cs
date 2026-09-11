using Conductor.Interfaces;
using HarmonyLib;
using ShinyShoe;
using ShinyShoe.Logging;
using System.Reflection;
using static RewardDetailsUI;
using static StoryChoiceData;

namespace Conductor.Patches
{
    [HarmonyPatch(typeof(StoryEventScreen), nameof(StoryEventScreen.ApplyScreenInput))]
    public static class StoryEventScreen_ApplyScreenInput
    {
        private static MethodInfo StoryEventScreen_ShowUpgradedCard = AccessTools.Method(typeof(StoryEventScreen), "ShowUpgradedCard", 
            [typeof(CardDetailsScreen), typeof(List<CardUpgradeData>), typeof(CardData), typeof(CardStateModifiers)]);

        private static readonly AccessTools.FieldRef<StoryEventScreen, StoryChoiceData.RewardInfo> SelectedCardRewardRef =
            AccessTools.FieldRefAccess<StoryEventScreen, StoryChoiceData.RewardInfo>("selectedCardReward");

        [HarmonyPrefix]
        public static bool Prefix(
                StoryEventScreen __instance,
                CoreInputControlMapping mapping,
                IGameUIComponent triggeredUI,
                InputManager.Controls triggeredMappingID,
                StoryChoiceData.RewardInfo ___selectedCardReward,
                SaveManager ___saveManager,
                ScreenManager ___screenManager,
                ChoiceRewardPreview ___rewardPreview,
                RelicManager ___relicManager,
                ref bool __result)
        {
            if (triggeredMappingID != InputManager.Controls.ShowCardPreview)
            {
                return true;
            }

            if (___selectedCardReward == null || ___selectedCardReward.previewType != StoryChoiceData.PreviewType.Reward)
            {
                return true;
            }

            GrantableRewardData? grantableRewardData = ___saveManager.GetAllGameData()?.FindRewardDataByName(___selectedCardReward.dataKey);

            if (grantableRewardData is not IRewardPreviewProvider customRewardData)
            {
                return true;
            }
            ___screenManager.ShowScreen(ScreenName.CardDetails, delegate (IScreen screen)
            {
                Plugin.Logger.LogError("DHSDIKJASKDFJAKSJDF");
                CardDetailsScreen cardDetailsScreen = (screen as CardDetailsScreen)!;
                IRewardPreviewProvider.RewardPreviewInfo previewInfo = customRewardData.ProvidePreview(___saveManager, ___relicManager);
                StoryEventScreen_ShowUpgradedCard.Invoke(__instance, [cardDetailsScreen, previewInfo.upgrades!, previewInfo.card!, previewInfo.cardStateModifiers!]);
                SelectedCardRewardRef(__instance) = null!;
                ___rewardPreview?.Hide();
            });

            __result = true;
            return false; //skip
        }
    }

    [HarmonyPatch(typeof(StoryChoiceData), "GetSpecificRewardName")]
    class StoryChoiceData_GetSpecificRewardNamePatch
    {
        public static bool Prefix(RewardInfo rewardInfo, SaveManager saveManager, StatusEffectManager statusEffectManager, ref string __result)
        {
            GrantableRewardData? grantableRewardData = saveManager.GetAllGameData().FindRewardDataByName(rewardInfo.dataKey);
            if (grantableRewardData == null)
            {
                return true;
            }
            if (grantableRewardData is IRewardPreviewProvider provider)
            {
                __result = provider.GetSpecificRewardName();
                return false;
            }
            return true;
        }
    }

    [HarmonyPatch(typeof(RewardDetailsUI), nameof(RewardDetailsUI.Set), [typeof(RewardPreviewData), typeof(int)])]
    class RewardDetailsUI_Set
    {
        private static MethodInfo RewardDetailsUI_ShowUpgradedCard = AccessTools.Method(typeof(RewardDetailsUI), "ShowUpgradedCard",
            [typeof(SaveManager), typeof(RelicManager), typeof(List<CardUpgradeData>), typeof(CardData), typeof(CardStateModifiers), typeof(bool), typeof(TooltipSide?)]);
        public static bool Prefix(RewardDetailsUI __instance, RewardPreviewData data, int index, CardTooltipContainer ___autoShowTooltipContainer, bool ___mTooltipsAreBlocked)
        {
            if (data == null)
            {
                return true;
            }
            if (data.previewType != StoryChoiceData.PreviewType.Reward)
            {
                return true;
            }

            GrantableRewardData? grantableRewardData = data.saveManager.GetAllGameData().FindRewardDataByName(data.dataKey);
            if (grantableRewardData == null || grantableRewardData is not IRewardPreviewProvider rewardPreviewProvider)
                return true;


            __instance.gameObject.SetActive(value: true);
            __instance.Reset();
            TooltipSide value = ((index == 0) ? TooltipSide.Left : TooltipSide.Right);
            bool autoShowTooltips = ___autoShowTooltipContainer != null && !___mTooltipsAreBlocked;

            var info = rewardPreviewProvider.ProvidePreview(data.saveManager, data.relicManager);
            RewardDetailsUI_ShowUpgradedCard.Invoke(__instance, [data.saveManager, data.relicManager, info.upgrades, info.card, info.cardStateModifiers, autoShowTooltips, value]);

            return false;
        }
    }

    [HarmonyPatch(typeof(StoryManager), nameof(StoryManager.CollectCardDatasReferencedByReward))]
    class StoryManager_CollectCardDatasReferencedByReward
    {
        public static void Postfix(RewardData reward, List<CardData> cardDatasCollected)
        {
            if (reward is ICardGiverReward cardGiverReward)
            {
                cardDatasCollected.AddRange(cardGiverReward.GetAllCardData());
            }
        }
    }
}
