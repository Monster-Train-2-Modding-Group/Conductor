using Conductor.Interfaces;
using System.Collections;
using static CardManager;

namespace Conductor.CardTraits
{
    /// <summary>
    /// CardTrait that when Scryed it is automatically played.
    /// </summary>
    public class CardTraitOmen : CardTraitState
    {
        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return PropDescriptions.DisplayNone;
        }

        public override string GetCardText()
        {
            return LocalizeTraitKey("CardTraitOmen_CardText");
        }

        public override string GetCardTooltipTitle()
        {
            return LocalizeTraitKey("CardTraitOmen_TooltipTitle");
        }

        public override string GetCardTooltipText()
        {
            return "CardTraitOmen_TooltipText".Localize();
        }
    }
}
