using Conductor.Interfaces;
using System.Collections;

namespace Conductor.CardTraits
{
    /// <summary>
    /// Keyword that when scried expands the scry by X.
    /// </summary>
    public class CardTraitOracle : CardTraitState, ICardTraitScryState
    {
        private bool HasBeenScried = false;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions()
            {
                [CardTraitFieldNames.ParamInt.GetFieldName()] = new PropDescription("Additional Scry when Scried")
            };
        }

        public int GetScryAmount(int currentScry, CardStatistics cardStatistics)
        {
            return currentScry;
        }

        public override void OnTraitStateReset()
        {
            HasBeenScried = false;
        }

        public int GetScryAmountOnScried()
        {
            HasBeenScried = true;
            return GetParamInt();
        }

        public IEnumerator OnScry(ICoreGameManagers coreGameManagers)
        {
            if (HasBeenScried)
            {
                var card = GetCard();
                card.RemoveTemporaryTrait(this);
                card.UpdateCardBodyText();
            }
            yield break;
        }

        public override string GetCardText()
        {
            var text = LocalizeTraitKey("CardTraitOracle_CardText");
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else return string.Format(text, GetParamInt());
        }

        public override string GetCardTooltipTitle()
        {
            return LocalizeTraitKey("CardTraitOracle_TooltipTitle");
        }

        public override string GetCardTooltipText()
        {
            var text = "CardTraitOracle_TooltipText".Localize();
            if (string.IsNullOrEmpty(text))
                return string.Empty;
            else return string.Format(text, GetParamInt());
        }
    }
}
