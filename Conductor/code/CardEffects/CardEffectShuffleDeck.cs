using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Conductor.CardEffects
{
    /// <summary>
    /// Card effect that shuffles the deck.
    /// 
    /// Parameters:
    ///     param_bool: If true shuffles discard into the draw pile.
    /// </summary>
    public class CardEffectShuffleDeck : CardEffectBase
    {
        public override bool CanApplyInPreviewMode => false;

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var cardManager = coreGameManagers.GetCardManager();
            if (cardEffectState.GetParamBool())
            {
                cardManager.ShuffleDiscardIntoDrawPile();
            }
            else
            {
                cardManager.ShuffleDeck();
            }
            yield break;
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Also shuffle discard into deck.")
            };
        }
    }
}
