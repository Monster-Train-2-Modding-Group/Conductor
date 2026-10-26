using Conductor.CardTraits;
using Conductor.Extensions;
using Conductor.Interfaces;
using System.Collections;
using UnityEngine;

namespace Conductor.CardEffects
{
    /// <summary>
    /// (Not to be used from within JSON)

    /// Generalized CardEffect to view the top N cards (or most recently added to pile) of a CardPile.
    /// The player gets to select any number of them.
    /// 
    /// The CardEffect adds a keyword tooltip depending on TargetMode. The following keywords are given.
    ///   DeckPile -> Scry
    ///   Consume -> Exhume
    ///   Discard -> Scavenge
    ///   Eaten -> Regurgitate
    /// </summary>
    public abstract class CardEffectScryTargetPileBase : CardEffectBase, ICardEffectUiDialog, ICardEffectWithDeckScreen
    {
        public override bool CanApplyInPreviewMode => false;
        private TargetMode targetMode;
        private int baseCardsToScry;

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamCardFilter.GetFieldName()] = new PropDescription("Card filter"),
                [CardEffectFieldNames.ParamInt.GetFieldName()] = new PropDescription("Base number of cards to scry."),
                [CardEffectFieldNames.ParamStr.GetFieldName()] = new PropDescription("Override Localization Key for instructions."),
                [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Optional Card Upgrade")
            };
        }

        public override bool TestEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers)
        {
            int count = 0;
            var filter = cardEffectState.GetParamCardFilter();
            var cardType = cardEffectState.GetTargetCardType();
            var relicManager = coreGameManagers.GetRelicManager();
            foreach (var card in cardEffectParams.targetCards)
            {
                if (filter != null && filter.FilterCard(card, relicManager))
                {
                    continue;
                }
                if (cardType != CardType.Invalid && cardType != card.GetCardType())
                {
                    continue;
                }
                count++;
            }
            return count > 0;
        }

        /// <summary>
        /// Get Base Amount to view cards from pile
        /// </summary>
        /// <param name="amount">Base Amount from ParamInt</param>
        /// <param name="cardState">Played card</param>
        /// <param name="targetMode">TargetMode</param>
        /// <param name="cardStatistics">CardStatistics</param>
        /// <returns>The number of cards to view</returns>
        protected virtual int GetBaseScryAmount(int amount, CardState? cardState, TargetMode targetMode, CardStatistics cardStatistics)
        {
            if (cardState != null && targetMode == TargetMode.DrawPile)
            {
                foreach (CardTraitState traitState in cardState.GetTraitStates())
                {
                    if (traitState is ICardTraitScryState trait)
                    {
                        amount += trait.GetScryAmount(amount, cardStatistics);
                    }
                }
            }
            return amount;
        }

        /// <summary>
        /// Called for each card that is revealed in a Scry and can extend the amount of cards scried.
        /// </summary>
        /// <param name="card">Card found</param>
        /// <param name="targetMode">TargetMode</param>
        protected virtual int GetAdditionalScryAmountForCard(CardState card, TargetMode targetMode)
        {
            if (targetMode != TargetMode.DrawPile)
                return 0;

            int ret = 0;
            foreach (CardTraitState traitState in card.GetTraitStates())
            {
                if (traitState is ICardTraitScryState trait)
                {
                    ret += trait.GetScryAmountOnScried();
                }
            }
            return ret;
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            var screenManager = sysManagers.GetScreenManager();
            var cardManager = coreGameManagers.GetCardManager();
            // TODO There is a Replay issue with enabling selecting multiple cards on DeckScreen. The game only records the first card selected.
            // The class can't be hacked because the Replay selects the one card then hits confirm. And you can't apparently modify it to select multiple cards.
            int numCardsSelectable = 1;//cardEffectState.GetAdditionalParamInt();
            targetMode = cardEffectState.GetTargetMode();


            CardState? cardState = cardEffectState.GetParentCardState() ?? cardEffectParams.playedCard;
            baseCardsToScry = GetBaseScryAmount(cardEffectState.GetParamInt(), cardState, cardEffectState.GetTargetMode(), coreGameManagers.GetCardStatistics());


            DeckScreen.FilterCardStateDelegate FirstNValidCards(CardType cardType, CardUpgradeMaskData? filter, List<CardState> outCards, int n, TargetMode targetMode, RelicManager relicManager)
            {
                return c =>
                {
                    if (filter != null && filter.FilterCard(c, relicManager))
                    {
                        return true;
                    }
                    if (cardType != CardType.Invalid && cardType != c.GetCardType())
                    {
                        return true;
                    }
                    if (outCards.Count < n)
                    {
                        outCards.Add(c);
                        n += GetAdditionalScryAmountForCard(c, targetMode);
                        return false;
                    }
                    return true;
                };
            }

            bool cardChosen = false;
            List<CardState> scryedCards = [];
            List<CardState> chosenCards = [];
            screenManager.SetScreenActive(ScreenName.Deck, active: true, delegate (IScreen screen)
            {
                if (screen is DeckScreen deckScreen)
                {
                    string instructionsKey = ((cardEffectState.GetParamStr() != string.Empty) ? cardEffectState.GetParamStr() : "ScreenDeck_Select_CardEffectScryTargetPile");
                    deckScreen.SetRequestingCardEffect(this);
                    deckScreen.Setup(new DeckScreen.Params
                    {
                        mode = DeckScreen.Mode.CardEffectSelection,
                        targetMode = cardEffectState.GetTargetMode(),
                        showCancel = false,
                        titleKey = (cardEffectState.GetParentCardState()?.GetTitleKey() ?? string.Empty),
                        instructionsKey = instructionsKey,
                        cardUpgradeData = cardEffectState.GetParamCardUpgradeData(),
                        filterFunc = FirstNValidCards(cardEffectState.GetTargetCardType(), cardEffectState.GetParamCardFilter(), scryedCards, baseCardsToScry, cardEffectState.GetTargetMode(), coreGameManagers.GetRelicManager()),
                        excludeFilteredOutCards = true,
                        numCardsSelectable = numCardsSelectable,
                    });
                    deckScreen.AddDeckScreenCardStateChosenDelegate(delegate (CardState chosenCardState)
                    {
                        chosenCards.Add(chosenCardState);
                        screenManager.SetScreenActive(ScreenName.Deck, active: false);
                        if (chosenCards.Count == numCardsSelectable || (chosenCards.Count == scryedCards.Count))
                        {
                            cardChosen = true;
                        }
                    });
                }
            });

            // Wait for Completion
            while (!cardChosen)
            {
                yield return null;
            }

            // Pile is in reverse order
            scryedCards.Reverse();
            scryedCards = scryedCards.Take(baseCardsToScry).ToList();

            yield return PostScryCards(chosenCards, scryedCards, cardEffectState.GetTargetMode(), coreGameManagers);
            yield return OnCardsChosen(chosenCards, scryedCards, cardEffectState, cardEffectParams, coreGameManagers, sysManagers);
        }

        /// <summary>
        /// Called after the user closes the Deck. Default implementation plays Omens.
        /// </summary>
        /// <param name="chosenCards">Cards picked</param>
        /// <param name="scryedCards">Cards found from base effect</param>
        /// <param name="targetMode">TargetMode</param>
        /// <param name="coreGameManagers">CoreGameManagers</param>
        protected virtual IEnumerator PostScryCards(List<CardState> chosenCards, List<CardState> scryedCards, TargetMode targetMode, ICoreGameManagers coreGameManagers)
        {
            if (targetMode != TargetMode.DrawPile)
                yield break;

            List<CardState> omens = [];
            foreach (var card in scryedCards)
            {
                foreach (var trait in card.GetTraitStates())
                {
                    if (trait is ICardTraitScryState scryState)
                    {
                        yield return scryState.OnScry(coreGameManagers);
                    }
                    if (trait is CardTraitOmen)
                    {
                        omens.Add(card);
                    }
                }
            }

            yield return PlayOmens(omens, coreGameManagers);

            foreach (var omen in omens)
            {
                scryedCards.Remove(omen);
            }
        }

        private IEnumerator PlayOmens(List<CardState> omens, ICoreGameManagers coreGameManagers)
        {
            CardManager cardManager = coreGameManagers.GetCardManager();
            CombatManager combatManager = coreGameManagers.GetCombatManager();
            SaveManager saveManager = coreGameManagers.GetSaveManager();
            HandUI handUI = coreGameManagers.GetHandUI();

            Vector3 target = AllGameManagers.Instance?.GetGameScreen()?.GetDiscardPileLocation(useScreenSpace: true) ?? Vector3.zero;

            BalanceData.AnimationTimingData animationTimingData = saveManager.GetBalanceData().GetAnimationTimingData();
            for (int i = 0; i < omens.Count; i++)
            {
                var card = omens[i];
                CardAnimator.CardAnimationParameters type = CardAnimator.CardAnimationParameters.FromCardState(card, handUI, target, Vector3.zero);
                type.sourcePile = CardPile.DeckPile;
                type.wasPlayed = true;
                type.currentCardIndex = i;
                type.maxCardsToShowcase = omens.Count;

                // Prevent card object duplication.
                if (cardManager.IsCardInDrawPile(card))
                    cardManager.GetDrawPile().Remove(card);

                CardAnimator.DoCardMovementAnimation.Dispatch(type);
                cardManager.PlayAnyCard(card, out var _, null, null, ignoreCost: true, discardCard: true, fromDirectPlay: false);
                yield return combatManager.ManuallyProcessEffectsQueue();
            }
            float num = animationTimingData.cardDrawAnimationDuration + animationTimingData.cardDiscardAnimationDuration;
            yield return CoreUtil.WaitForSecondsOrBreak(num);
        }

        /// <summary>
        /// Called after the player has selected cards. For Scry (target Deck) Omens will have already played.
        /// </summary>
        /// <param name="chosenCards">Cards picked by player</param>
        /// <param name="scryedCards">All cards that were viewed</param>
        /// <param name="cardEffectState">CardEffectState</param>
        /// <param name="cardEffectParams">CardEffectParams</param>
        /// <param name="coreGameManagers">CoreGameManagers</param>
        /// <param name="sysManagers">SystemManagers</param>
        protected abstract IEnumerator OnCardsChosen(List<CardState> chosenCards, List<CardState> scryedCards, CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers);

        protected void ApplyCardUpgrade(CardEffectState cardEffectState, CardState toCardState, ICoreGameManagers coreGameManagers)
        {
            CardManager cardManager = coreGameManagers.GetCardManager();
            if (cardEffectState.GetParamCardUpgradeData() != null && CardUpgradeHelper.TryApplyUpgradeToCard(toCardState, null, cardEffectState.GetParamCardUpgradeData(), isTemporaryUpgrade: true, isFromHiddenTrigger: false, coreGameManagers, out CardUpgradeState upgradeCopy))
            {
                CardAnimator.CardUpgradeAnimationInfo type = new(toCardState, upgradeCopy);
                CardAnimator.DoAddRecentCardUpgrade.Dispatch(type);
                toCardState.UpdateCardBodyText();
                cardManager.RefreshCardInHand(toCardState);
            }
        }

        public virtual List<DeckScreen.CardInfo> SortDeck(List<DeckScreen.CardInfo> sortedDeck, DeckScreen.Mode deckScreenMode, ICoreGameManagers coreGameManagers)
        {
            static int FindIndex(DeckScreen.CardInfo cardInfo, List<CardState> drawPile)
            {
                for (int i = 0; i < drawPile.Count; i++)
                {
                    if (drawPile[i] == cardInfo.cardState)
                    {
                        return i;
                    }
                }
                return -1;
            }

            List<CardState> drawPile = coreGameManagers.GetCardManager().GetDrawPile();
            List<DeckScreen.CardInfo> ordered = [.. sortedDeck.OrderBy((DeckScreen.CardInfo cardInfo) => FindIndex(cardInfo, drawPile)).Reverse()];
            return ordered;
        }

        public virtual void UpdateHighlight(List<DeckScreen.CardInfo> cardInfos, List<int> selectedIndices, DeckScreen.Mode deckScreenMode, ICoreGameManagers coreGameManagers)
        {
            // Extra handling for Scry
            if (targetMode == TargetMode.DrawPile)
            {
                var total = cardInfos.Count;
                for (int i = 0; i < baseCardsToScry; i++)
                {
                    if (selectedIndices.Contains(i)) continue;
                    cardInfos[i].cardUI.SetEdgeFxActive(CardUI.EdgeHighlight.Castable);
                }
            }
        }

        public override bool CreateAdditionalTooltips(CardEffectData cardEffectData, List<TooltipContent> content, SaveManager saveManager, CardState? sourceCardState = null)
        {
            var baseKey = $"CardEffectScryTargetPile_{cardEffectData.GetTargetMode().ToString()}";
            var title = $"{baseKey}_TooltipTitle";
            var body = $"{baseKey}_TooltipText";
            content.Add(new TooltipContent(title.Localize(), body.Localize(new LocalizedIntegers(cardEffectData.GetParamInt(), cardEffectData.GetAdditionalParamInt())), TooltipDesigner.TooltipDesignType.Keyword, this.GetType().Name));
            return true;
        }
    }
}
