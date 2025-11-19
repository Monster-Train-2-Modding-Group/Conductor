using Conductor.Triggers;
using ShinyShoe;
using System.Collections;

namespace Conductor.CardEffects
{
    /// <summary>
    /// A variant of CardEffectEnchant that allows for the following extra features:
    /// 
    /// 1. Enchanting other floors or the entire train.
    /// 2. Enchanting the pyre (Note that status effects can't be enchanted unless you override the immune status).
    /// 3. Enchanting a CardUpgrade (This allows you to enchant triggers, and stat upgrades just like a Room card can).
    /// 4. Self Enchanting
    /// 
    /// Note that this CardEffect must be played in a AfterSpawnBetterEnchant CharacterTrigger also defined in Conductor.
    /// 
    /// Parameters:
    ///   param_upgrade: Optional CardUpgrade
    ///   param_status_effects: Optional. Must be 1 status effect to enchant if multiple are present one is chosen at random.
    ///   param_bool: True to allow enchanting self.
    ///   use_status_effect_stack_multiplier: Multiply param_status_effects by the stacks of a particular status on character.
    ///   supress_pyre_room_focus (sic): Must be set to true if enchanting the Pyre (target_mode = "pyre"
    ///   
    /// Additionally if enchanting other floors the trigger must have suppress_notifications set to true.
    /// </summary>
    public sealed class CardEffectEnchant : CardEffectAddCardUpgradeToUnits, ICardEffectEnchant, ICardEffect
    {
        private enum EnchanterStateNextAction
        {
            NoAction,
            AddStatusEffect,
            RemoveStatusEffect
        }

        private class EnchantedState
        {
            public bool isEnchanted;
            public EnchanterStateNextAction nextStateAction;
            public CardUpgradeState? enchantmentUpgrade;

            public EnchantedState()
            {
            }

            public EnchantedState(EnchantedState other)
            {
                isEnchanted = other.isEnchanted;
                nextStateAction = other.nextStateAction;
                enchantmentUpgrade = other.enchantmentUpgrade;
            }

            public override string ToString()
            {
                return $"(Applied? {isEnchanted} Action? {nextStateAction})";
            }
        }

        private Dictionary<CharacterState, EnchantedState> _primaryEnchantedTargets = [];
        private Dictionary<CharacterState, EnchantedState> _previewEnchantedTargets = [];
        private bool _previewEnchantedTargetsRequireSync;
        private ICoreGameManagers? cachedCoreGameManagers;
        private ISystemManagers? cachedSysManagers;
        private CardEffectState? cachedState;
        private CharacterState? enchanterCharacter;
        private StatusEffectStackData? statusEffect;
        private CharacterState? applyUpgradeTarget;
        private CardUpgradeData? cardUpgrade;

        private Dictionary<CharacterState, EnchantedState> EnchantedTargets
        {
            get
            {
                if (!cachedCoreGameManagers.IsNullOrDestroyed())
                {
                    if (cachedCoreGameManagers.GetSaveManager().PreviewMode)
                    {
                        return _previewEnchantedTargets;
                    }
                    return _primaryEnchantedTargets;
                }
                return _primaryEnchantedTargets;
            }
        }

        public override PropDescriptions CreateEditorInspectorDescriptions()
        {
            return new PropDescriptions
            {
                [CardEffectFieldNames.ParamStatusEffects.GetFieldName()] = new PropDescription("Status Effect", "If more than one provided, chooses one at random"),
                [CardEffectFieldNames.ParamBool.GetFieldName()] = new PropDescription("Enchant self", "True - Enchant also effects the unit enchanting"),
                [CardEffectFieldNames.ParamCardUpgradeData.GetFieldName()] = new PropDescription("Card Upgrade", "Card upgrade to enchant to eligible units"),
                [CardEffectFieldNames.UseStatusEffectStackMultiplier.GetFieldName()] = new PropDescription("Use Status Effect Stack Multiplier", "Multiply the number of stacks added by the number of this type of status effect the target already has.")
            };
        }

        public override void Setup(CardEffectState cardEffectState)
        {
            _primaryEnchantedTargets.Clear();
            _previewEnchantedTargets.Clear();
            cachedState = cardEffectState;
            cardEffectState.SetShouldOverrideTriggerUI(value: true);
        }

        public void SetEnchanterCharacter(CharacterState enchanterCharacter, ICoreGameManagers coreGameManagers)
        {
            this.enchanterCharacter = enchanterCharacter;
            cachedCoreGameManagers = coreGameManagers;
            this.enchanterCharacter.AddCharacterRemovedSignal(OnEnchanterRemoved, addOnce: true);
        }

        private IEnumerator ApplyEffectInternal()
        {
            if (cachedCoreGameManagers.IsNullOrDestroyed() || enchanterCharacter == null || cachedState == null)
            {
                yield break;
            }
            statusEffect = CardEffectAddStatusEffect.GetStatusEffectStack(cachedState.GetSourceCardEffectData(), cachedState, Array.Empty<CharacterState>(), null, cachedCoreGameManagers);
            cardUpgrade = cachedState.GetParamCardUpgradeData();

            SaveManager saveManager = cachedCoreGameManagers.GetSaveManager();
            StatusEffectManager statusEffectManager = cachedCoreGameManagers.GetStatusEffectManager();
            using (new CharacterState.SetAllowDestroyedAccessHelper(enchanterCharacter, onlyIfDestroyed: true))
            {
                CardEffectParams cardEffectParams = new()
                {
                    selfTarget = enchanterCharacter,
                    selectedRoom = enchanterCharacter.GetCurrentRoomIndex()
                };
                TargetHelper.CollectTargets(cachedState, cardEffectParams, cachedCoreGameManagers, saveManager.PreviewMode);
                if (saveManager.PreviewMode && _previewEnchantedTargetsRequireSync)
                {
                    _previewEnchantedTargetsRequireSync = false;
                    foreach (KeyValuePair<CharacterState, EnchantedState> primaryEnchantedTarget in _primaryEnchantedTargets)
                    {
                        _previewEnchantedTargets[primaryEnchantedTarget.Key] = new EnchantedState(primaryEnchantedTarget.Value);
                    }
                }
                foreach (CharacterState target in cardEffectParams.targets)
                {
                    if (!EnchantedTargets.ContainsKey(target) && target != enchanterCharacter)
                    {
                        EnchantedTargets.Add(target, new EnchantedState());
                    }
                }
                bool flag = false;
                foreach (KeyValuePair<CharacterState, EnchantedState> enchantedTarget in EnchantedTargets)
                {
                    CharacterState key = enchantedTarget.Key;
                    enchantedTarget.Value.nextStateAction = (IsEnchantmentValidForTarget(key) ? EnchanterStateNextAction.AddStatusEffect : EnchanterStateNextAction.RemoveStatusEffect);
                    if (enchantedTarget.Value.nextStateAction == EnchanterStateNextAction.AddStatusEffect && !enchantedTarget.Value.isEnchanted)
                    {
                        flag = true;
                    }
                }
                BalanceData.TimingData activeTiming = saveManager.GetActiveTiming();
                if (flag && !saveManager.PreviewMode && enchanterCharacter.IsAlive && !enchanterCharacter.IsDestroyed)
                {
                    enchanterCharacter.DoMovementAttacking(activeTiming.UnitAttackWindUpDuration, activeTiming.UnitAttackDuration, CharacterState.MovementActionType.Enchant);
                }
                bool flag2 = enchanterCharacter.GetStatusEffectStacks("duality") > 0;
                foreach (KeyValuePair<CharacterState, EnchantedState> enchantedTarget2 in EnchantedTargets)
                {
                    CharacterState key2 = enchantedTarget2.Key;
                    EnchantedState value = enchantedTarget2.Value;
                    if (key2.IsDestroyed || (key2.IsDead && key2.GetStatusEffectStacks("undying") <= 0) || key2.PreviewMode != saveManager.PreviewMode)
                    {
                        continue;
                    }
                    int num = statusEffect?.count ?? 0;
                    if (flag2 && statusEffect != null && StatusEffectDualityState.IsStatusEffectedByDuality(statusEffect.statusId, statusEffectManager))
                    {
                        num *= 2;
                    }
                    if (value.nextStateAction == EnchanterStateNextAction.AddStatusEffect && !value.isEnchanted)
                    {
                        value.isEnchanted = true;
                        if (statusEffect != null)
                        {
                            using (GenericPools.Get<CharacterState.AddStatusEffectParams>(out CharacterState.AddStatusEffectParams poolObject))
                            {
                                poolObject.ApplyEffectParams<CardEffectEnchant>(cachedState, cardEffectParams);
                                poolObject.sourceIsHero = key2.GetTeamType() == Team.Type.Heroes;
                                key2.AddStatusEffect(statusEffect.statusId, num, poolObject, null, allowModification: true, cardEffectParams.isFromHiddenTrigger);
                            }
                        }
                        if (cardUpgrade != null)
                        {
                            applyUpgradeTarget = key2;
                            int upgradesCount = key2.GetAppliedCardUpgrades().Count;
                            yield return base.ApplyEffect(cachedState, cardEffectParams, cachedCoreGameManagers, cachedSysManagers!);
                            if (upgradesCount < key2.GetAppliedCardUpgrades().Count)
                                value.enchantmentUpgrade = key2.GetAppliedCardUpgrades().Last();
                        }
                    }
                    else if (value.nextStateAction == EnchanterStateNextAction.RemoveStatusEffect && value.isEnchanted)
                    {
                        value.isEnchanted = false;
                        if (statusEffect != null)
                        {
                            using (GenericPools.Get<CharacterState.RemoveStatusEffectParams>(out CharacterState.RemoveStatusEffectParams poolObject2))
                            {
                                poolObject2.ApplyEffectParams<CardEffectEnchant>(cardEffectParams);
                                poolObject2.showNotification = !saveManager.PreviewMode;
                                key2.RemoveStatusEffect(statusEffect.statusId, num, poolObject2);
                            }
                        }
                        if (cardUpgrade != null && value.enchantmentUpgrade != null)
                        {
                            yield return key2.RemoveCardUpgrade(value.enchantmentUpgrade, value.enchantmentUpgrade.GetCardUpgradeDataId());
                            value.enchantmentUpgrade = null;
                            // Kick off a signal
                            if (!saveManager.PreviewMode && key2.IsPyreHeart() && cachedState.GetTargetMode() == TargetMode.Pyre && cardUpgrade.GetBonusDamage() != 0)
                            {
                                saveManager.pyreAttackChangedSignal.Dispatch(saveManager.GetDisplayedPyreAttack(), saveManager.GetDisplayedPyreNumAttacks());
                            }
                        }
                    }
                    value.nextStateAction = EnchanterStateNextAction.NoAction;
                }
            }
        }

        private bool IsEnchantmentValidForTarget(CharacterState target)
        {
            if (enchanterCharacter == null || target == null)
            {
                return false;
            }
            if (enchanterCharacter.GetStatusEffectStacks("muted") > 0)
            {
                return false;
            }
            if (enchanterCharacter.GetStatusEffectStacks("silenced") > 0)
            {
                return false;
            }
            if (!enchanterCharacter.IsDestroyed && enchanterCharacter.IsAlive && !target.IsDestroyed && target.IsAlive && (enchanterCharacter != target) || cachedState!.GetParamBool())
            {
                return cachedState!.GetTargetMode() == TargetMode.Tower || (cachedState!.GetTargetMode() == TargetMode.Pyre && target.IsPyreHeart()) || enchanterCharacter.GetCurrentRoomIndex() == target.GetCurrentRoomIndex();
            }
            return false;
        }

        private IEnumerator OnEnchanterRemoved()
        {
            yield return ApplyEffectInternal();
        }

        public override IEnumerator ApplyEffect(CardEffectState cardEffectState, CardEffectParams cardEffectParams, ICoreGameManagers coreGameManagers, ISystemManagers sysManagers)
        {
            cachedSysManagers = sysManagers;
            yield return ApplyEffectInternal();
        }

        public void OnUpdateEnchantments()
        {
            cachedCoreGameManagers?.GetCombatManager().QueueTrigger(enchanterCharacter, CharacterTriggers.AfterSpawnBetterEnchant);
        }

        public void PrepareForPreview()
        {
            _previewEnchantedTargetsRequireSync = true;
        }

        public override void GetTooltipsStatusList(CardEffectState cardEffectState, ref List<string> outStatusIdList)
        {
            GetTooltipsStatusList(cardEffectState.GetSourceCardEffectData(), ref outStatusIdList);
        }

        public static new void GetTooltipsStatusList(CardEffectData cardEffectData, ref List<string> outStatusIdList)
        {
            CardEffectAddCardUpgradeToUnits.GetTooltipsStatusList(cardEffectData, ref outStatusIdList);
            StatusEffectStackData[] paramStatusEffects = cardEffectData.GetParamStatusEffects();
            foreach (StatusEffectStackData statusEffectStackData in paramStatusEffects)
            {
                outStatusIdList.Add(statusEffectStackData.statusId);
            }
        }

        protected override void CollectTargetsForUpgrade(CardEffectState cardEffectState, CardEffectParams cardEffectParams, List<CharacterState> upgradeTargets, ICoreGameManagers coreGameManagers)
        {
            upgradeTargets.Clear();
            foreach (var upgradeMask in cardUpgrade!.GetFilters())
            {
                if (!upgradeMask.FilterCharacter(applyUpgradeTarget, coreGameManagers.GetRelicManager()))
                {
                    return;
                }
            }
            upgradeTargets.Add(applyUpgradeTarget!);
        }
    }

}
