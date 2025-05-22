using System.Collections;
using UnityEngine;

namespace Conductor
{
    class CardTraitLoaned : CardTraitState
    {
        PlayerManager? playerManager;
        SaveManager? saveManager;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return PropDescriptions.DisplayNone;
        }

        public override void OnCardDrawn(CardState thisCard, ICoreGameManagers coreGameManagers)
        {
            playerManager = coreGameManagers.GetPlayerManager();
            saveManager = coreGameManagers.GetSaveManager();
        }

        public override bool GetIsPlayableFromHand(CardManager cardManager, RoomManager roomManager, out CommonSelectionBehavior.SelectionError selectionError)
        {
            selectionError = CommonSelectionBehavior.SelectionError.None;
            if (saveManager!.GetGold() < Mathf.Abs(GetParamInt()))
            {
                selectionError = CommonSelectionBehavior.SelectionError.InsufficientGold;
                return false;
            }
            return true;
        }

        public override bool GetIsPlayableFromPlay(CardManager cardManager, RoomManager roomManager, out CommonSelectionBehavior.SelectionError selectionError)
        {
            selectionError = CommonSelectionBehavior.SelectionError.None;
            if (saveManager!.GetGold() < Mathf.Abs(GetParamInt()))
            {
                selectionError = CommonSelectionBehavior.SelectionError.InsufficientGold;
                return false;
            }
            return true;
        }

        public override IEnumerator OnPreCardPlayed(CardState cardState, ICoreGameManagers coreGameManagers)
        {
            yield return coreGameManagers.GetPlayerManager().AdjustGold(-GetParamInt());
        }

        public override string GetCardText()
        {
            var text = LocalizeTraitKey("CardTraitLoaned_CardText");
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else return string.Format(text, GetParamInt());
        }

        public override string GetCardTooltipTitle()
        {
            return LocalizeTraitKey("CardTraitLoaned_TooltipTitle");
        }

        public override string GetCardTooltipText()
        {
            var text = "CardTraitLoaned_TooltipText".Localize();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else return string.Format(text, GetParamInt());
        }
    }
}
