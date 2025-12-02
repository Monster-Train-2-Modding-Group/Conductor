using Conductor.Extensions;
using Conductor.TrackedValues;
using HarmonyLib;
using I2.Loc;
using ShinyShoe;
using System.Reflection;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Conductor.UI
{
    public abstract class ClassMechanicHud : HudCountUI, IGameUISubComponent
    {
        public enum HudType
        {
            Invalid,
            Small,
            Large,
            Custom
        }

        public enum HudDisplayMode
        {
            Invalid,
            BattleOnly,
            Run
        }

#pragma warning disable CS8618
        /// <summary>
        /// Main Game Object controlling the UI.
        /// </summary>
        protected GameObject mainGameObject;
        /// <summary>
        /// Hud Display Mode.
        /// </summary>
        protected HudDisplayMode displayMode;
        /// <summary>
        /// Clan associated with this HUD.
        /// </summary>
        protected ClassData? linkedClass;
        /// <summary>
        /// Background image for the banner. Only the Luna Coven sized banner is supported at this time.
        /// </summary>
        protected Sprite backgroundImage;
        /// <summary>
        /// An Array of Icon Images, by default index 0 is displayed as the icon.
        /// If intending to switch between the images a SpriteSwapper is needed.
        /// </summary>
        protected Sprite[] iconImages;
        /// <summary>
        /// Tooltip Provider which displays a tooltip.
        /// </summary>
        protected TooltipProviderComponent tooltipProvider;
        /// <summary>
        /// Optional TrackedValue Handler associated with the UI.
        /// </summary>
        protected AbstractTrackedValueHandler? trackedValueHandler;
        /// <summary>
        /// Tooltip title Localization Key
        /// </summary>
        protected string tooltipTitleKey = string.Empty;
        /// <summary>
        /// Tooltip text Localization Key
        /// </summary>
        protected string tooltipBodyKey = string.Empty;
        /// <summary>
        /// Label text Localization Key (only for Large style HUDs).
        /// </summary>
        protected string labelTextKey = string.Empty;
        /// <summary>
        /// Additional defined labels (only for Large style HUDs).
        /// Can be used if the Label text changes like Luna Covens New Moon / Full Moon.
        /// </summary>
        protected string[] additionalLabelKeys = Array.Empty<string>();
        /// <summary>
        /// Optional label used in LunaCovenUI which displays "New Moon"/"Full Moon"
        /// Not defined for small HUDs.
        /// </summary>
        protected TMP_Text? label;
        /// <summary>
        /// Font used by labels in HUD
        /// </summary>
        protected static TMP_FontAsset labelFont;
        /// <summary>
        /// Material for outlining characters for Font.
        /// </summary>
        protected static Material labelMaterial;
        /// <summary>
        /// Font used by the count label in HUD
        /// </summary>
        protected static TMP_FontAsset countFont;
        /// <summary>
        /// Material for outlining the characters in count label for Font.
        /// </summary>
        protected static Material countMaterial;
        /// <summary>
        /// FieldInfo for _tooltipSidePivot.
        /// </summary>
        protected static FieldInfo tooltipSidePivotField = AccessTools.Field(typeof(TooltipProviderComponent), "_tooltipSidePivot");
        /// <summary>
        /// AllGameManagers instance.
        /// </summary>
        protected AllGameManagers allGameManagers;
#pragma warning restore CS8618

        private GameObject ContentRoot => mainGameObject.OrNull() ?? base.gameObject;


        private void OnDestroy()
        {
            trackedValueHandler?.ValueChangedSignal?.RemoveListener(HandleValueChanged);
        }

        /// <summary>
        /// Only called when the HUDType is Custom.
        /// You construct the HUD in this function.
        /// </summary>
        public virtual void Construct()
        {

        }

        /// <summary>
        /// Called when this HUD is added as a child of the main HUD
        /// The base class sets up the TooltipProvider and any localized text.
        /// </summary>
        public virtual void Initialize()
        {
            SetTooltip(tooltipTitleKey, tooltipBodyKey);
            if (label != null)
            {
                label.text = labelTextKey.Localize();
            }
        }

        /// <summary>
        /// Called to clear the HUD by setting it inactive and clearing tooltips.
        /// </summary>
        public virtual void Clear()
        {
            ContentRoot.SetActive(value: false);
            tooltipProvider.ClearTooltips();
        }

        /// <summary>
        /// Function to determine if the HUD should be shown.
        /// For a hud that only displays in battle the following code should be present. 
        /// <code>
        /// if (saveManager.GetGameSequence() != SaveData.GameSequence.InBattle)
        ///   return false
        ///</code>
        /// Additionally for a clan based hud also check if the current MainClass / SubClass matches the clan you are interested in.
        /// </summary>
        /// <param name="saveManager">SaveManager instance.</param>
        /// <param name="playerManager">PlayerManager instance.</param>
        /// <param name="cardManager">CardManager instance.</param>
        /// <returns>True if the HUD should appear.</returns>
        public virtual bool ShouldShowUI(SaveManager saveManager, PlayerManager playerManager, CardManager cardManager, ScreenManager screenManager)
        {
            if (linkedClass != null && saveManager.GetMainClass() != linkedClass && saveManager.GetSubClass() != linkedClass)
            {
                return false;
            }

            switch (displayMode)
            {
                case HudDisplayMode.BattleOnly:
                    return saveManager.GetGameSequence() == SaveData.GameSequence.InBattle;
                case HudDisplayMode.Run:
                    return IsAppropriateToDisplayDuringRun(screenManager, saveManager);
                default:
                    Plugin.Logger.LogError($"Invalid HUD display mode for hud {mainGameObject.name}");
                    return false;
            }
        }

        /// <summary>
        /// Function to determine if a Hud is appropriate to display during a run.
        /// This function evaluates to Dragon's Hoard Hud display logic.
        /// </summary>
        /// <param name="screenManager"></param>
        /// <returns></returns>
        protected bool IsAppropriateToDisplayDuringRun(ScreenManager screenManager, SaveManager saveManager)
        {
            SaveData.GameSequence gameSequence = saveManager.GetGameSequence();
            if (saveManager.GetVictorySectionState() == SaveManager.VictorySectionState.Victory && gameSequence != SaveData.GameSequence.BattlePostCombatRewards)
            {
                return false;
            }
            if ((uint)gameSequence > 3u && (uint)(gameSequence - 5) > 2u)
            {
                return false;
            }
            switch (screenManager.GetTopScreen())
            {
                case ScreenName.Game:
                case ScreenName.Map:
                case ScreenName.RelicChoice:
                case ScreenName.Draft:
                case ScreenName.Reward:
                case ScreenName.Merchant:
                case ScreenName.Settings:
                case ScreenName.Dialog:
                case ScreenName.BattleIntro:
                case ScreenName.DragonsHoard:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Refreshes the HUD. Called when the main HUD refreshes state.
        /// </summary>
        /// <param name="saveManager">SaveManager instance.</param>
        /// <param name="playerManager">PlayerManager instance.</param>
        /// <param name="cardManager">CardManager instance.</param>
        public virtual void Refresh(SaveManager saveManager, PlayerManager playerManager, CardManager cardManager)
        {
        }

        /// <summary>
        /// Sets the Tooltips for the TooltipProvider.
        /// </summary>
        /// <param name="titleKey">Localization key for the tooltip title.</param>
        /// <param name="bodyKey">Localization key for the tooltip body.</param>
        protected virtual void SetTooltip(string titleKey, string bodyKey)
        {
            tooltipProvider.OrNull()?.SetTooltipLocalized(titleKey.Localize(), bodyKey.Localize());
        }

        /// <summary>
        /// Listener function.
        /// </summary>
        /// <param name="changedParams">TrackedValue changed params.</param>
        protected virtual void HandleValueChanged(TrackedValueChangedParams changedParams)
        {
            if (changedParams.updateUI == TrackedValueChangedParams.UiUpdateMode.Instant)
                ShowCount(changedParams.value);
            else if (changedParams.updateUI == TrackedValueChangedParams.UiUpdateMode.Animated)
                ChangeCount(changedParams.value);
        }

        protected override void DoChangeAnimation(int prevCount, int newCount)
        {
            var replayManager = AllGameManagers.Instance?.GetReplayManager();
            if (replayManager != null && replayManager.IsPlayingBackAReplay())
            {
                return;
            }
            base.DoChangeAnimation(prevCount, newCount);
        }

        /// <summary>
        /// Sets the tracked value handler for the HUD, subscribes to updates for the tracked value
        /// </summary>
        public void SetTrackedValueHandler(AbstractTrackedValueHandler trackedValueHandler)
        {
            this.trackedValueHandler = trackedValueHandler;
            trackedValueHandler.ValueChangedSignal?.AddListener(HandleValueChanged);
        }

        internal static ValueTuple<GameObject, ClassMechanicHud>? ConstructGameObject(string name, Type subclass, HudType type, HudDisplayMode displayMode, ClassData? clan, Sprite backgroundImage, Sprite[] iconImages, string? tooltipTitle, string? tooltipBody, string? labelText, string[]? additionalTexts)
        {
            if (labelFont == null)
            {
                labelFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.creationSettings.sourceFontFileGUID == "5a6b76c262e83fb43b6c10d3a9ffc45b");
                labelMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name.StartsWith(labelFont.name) && m.name.Contains("Outline"));
            }
            if (countFont == null)
            {
                countFont = Resources.FindObjectsOfTypeAll<TMP_FontAsset>().FirstOrDefault(f => f.creationSettings.sourceFontFileGUID == "0278b1f68e1d6494cbe9cf58d1e99df6");
                countMaterial = Resources.FindObjectsOfTypeAll<Material>().FirstOrDefault(m => m.name.StartsWith(countFont.name) && m.name.Contains("Outline"));
            }

            GameObject gameObject = new()
            {
                name = name,
                layer = LayerMask.NameToLayer("UI")
            };
            var ui = gameObject.AddComponent(subclass) as ClassMechanicHud;
            if (ui == null)
            {
                return null;
            }

            var selectable = gameObject.AddComponent<GameUISelectableWithNavigation>();
            var tooltipProvider = gameObject.AddComponent<TooltipProviderComponent>();
            ui.linkedClass = clan;
            ui.tooltipProvider = tooltipProvider;
            ui.tooltipTitleKey = tooltipTitle ?? string.Empty;
            ui.tooltipBodyKey = tooltipBody ?? string.Empty;
            ui.labelTextKey = labelText ?? string.Empty;
            ui.additionalLabelKeys = additionalTexts ?? Array.Empty<string>();
            ui.displayMode = displayMode;
            ui.mainGameObject = gameObject;
            ui.backgroundImage = backgroundImage;
            ui.iconImages = iconImages;
            tooltipProvider.SetTooltipSide(TooltipSide.VerticalAuto);
            tooltipSidePivotField.SetValue(tooltipProvider, 0f);
            var layout = gameObject.AddComponent<LayoutElement>();
            layout.preferredWidth = backgroundImage.texture.width;
            layout.preferredHeight = backgroundImage.texture.height;

            GameObject background = new()
            {
                name = name + "_Background",
                layer = LayerMask.NameToLayer("UI")
            };
            background.transform.parent = gameObject.transform;
            var image = background.AddComponent<Image>();
            image.sprite = backgroundImage;
            var rectTransform = (background.transform as RectTransform);
            rectTransform!.sizeDelta = new Vector2(backgroundImage.texture.width, backgroundImage.texture.height);

            if (type == HudType.Large)
                ConstructLargeHud(name, gameObject, ui, backgroundImage, iconImages);
            else if (type == HudType.Small)
                ConstructSmallHud(name, gameObject, ui, backgroundImage, iconImages);
            else if (type == HudType.Custom)
                ui.Construct();
            else
                Plugin.Logger.LogError($"Invalid Hud Type for HUD {gameObject.name}");

            return (gameObject, ui);
        }

        internal static void ConstructLargeHud(string name, GameObject gameObject, ClassMechanicHud ui, Sprite backgroundImage, Sprite[] iconImages)
        {
            GameObject icon = new()
            {
                name = name + "_Icon",
                layer = LayerMask.NameToLayer("UI")
            };
            icon.transform.parent = gameObject.transform;
            var iconComponentImage = icon.AddComponent<Image>();
            iconComponentImage.sprite = iconImages[0];
            var rectTransform2 = iconComponentImage.transform as RectTransform;
            rectTransform2!.sizeDelta = new Vector2(iconImages[0].textureRect.width, iconImages[0].textureRect.height);
            rectTransform2.offsetMin = new Vector2(-46, -24);
            rectTransform2.offsetMax = new Vector2(46, 68);

            GameObject count = new()
            {
                name = name + "_Count",
                layer = LayerMask.NameToLayer("UI")
            };
            count.transform.parent = icon.transform;
            var countLabelText = count.AddComponent<TextMeshProUGUI>();
            countLabelText.font = countFont;
            countLabelText.alignment = TextAlignmentOptions.Top;
            countLabelText.enableAutoSizing = true;
            countLabelText.fontSize = 40;
            countLabelText.fontSizeMin = 18;
            countLabelText.fontSizeMax = 40;

            countLabelText.vertexBufferAutoSizeReduction = true;
            countLabelText.fontSharedMaterial = countMaterial;
            var rectTransform3 = count.transform as RectTransform;
            rectTransform3!.anchorMin = new Vector2(0.5f, 0f);
            rectTransform3.anchorMax = new Vector2(0.5f, 0f);
            rectTransform3.sizeDelta = new Vector2(40, 96);
            rectTransform3.offsetMin = new Vector2(-20, -30);
            rectTransform3.offsetMax = new Vector2(20, 66);
            count.AddComponent<Localize>();

            GameObject label = new()
            {
                name = name + "_Label",
                layer = LayerMask.NameToLayer("UI")
            };
            label.transform.parent = gameObject.transform;
            var labelText = label.AddComponent<TextMeshProUGUI>();
            labelText.font = labelFont;
            labelText.alignment = TextAlignmentOptions.Top;
            labelText.enableAutoSizing = true;
            labelText.fontSize = 28;
            labelText.fontSizeMin = 24;
            labelText.fontSizeMax = 28;
            labelText.vertexBufferAutoSizeReduction = true;
            labelText.fontSharedMaterial = labelMaterial;
            label.AddComponent<Localize>();
            var rectTransform4 = label.transform as RectTransform;
            rectTransform4!.anchorMin = new Vector2(0, 0.5f);
            rectTransform4.anchorMax = new Vector2(1.0f, 0.5f);
            rectTransform4.sizeDelta = new Vector2(-24, 64);
            rectTransform4.offsetMin = new Vector2(12, -82);
            rectTransform4.offsetMax = new Vector2(-12, -18);

            ui.label = labelText;
            ui.countLabel = countLabelText;
        }

        internal static void ConstructSmallHud(string name, GameObject gameObject, ClassMechanicHud ui, Sprite backgroundImage, Sprite[] iconImages)
        {
            GameObject icon = new()
            {
                name = name + "_Icon",
                layer = LayerMask.NameToLayer("UI")
            };
            icon.transform.parent = gameObject.transform;
            var iconComponentImage = icon.AddComponent<Image>();
            iconComponentImage.sprite = iconImages[0];
            var rectTransform2 = iconComponentImage.transform as RectTransform;
            rectTransform2!.sizeDelta = new Vector2(iconImages[0].textureRect.width, iconImages[0].textureRect.height);
            rectTransform2.offsetMin = new Vector2(-36, -52);
            rectTransform2.offsetMax = new Vector2(36, 36);

            GameObject count = new()
            {
                name = name + "_Count",
                layer = LayerMask.NameToLayer("UI")
            };
            count.transform.parent = icon.transform;
            var countLabelText = count.AddComponent<TextMeshProUGUI>();
            countLabelText.font = countFont;
            countLabelText.alignment = TextAlignmentOptions.Top;
            countLabelText.enableAutoSizing = true;
            countLabelText.fontSize = 40;
            countLabelText.fontSizeMin = 18;
            countLabelText.fontSizeMax = 40;

            countLabelText.vertexBufferAutoSizeReduction = true;
            countLabelText.fontSharedMaterial = countMaterial;
            var rectTransform3 = count.transform as RectTransform;
            rectTransform3!.anchorMin = new Vector2(0.5f, 0f);
            rectTransform3.anchorMax = new Vector2(0.5f, 0f);
            rectTransform3.sizeDelta = new Vector2(40, 96);
            rectTransform3.offsetMin = new Vector2(-20, -24);
            rectTransform3.offsetMax = new Vector2(20, 72);
            count.AddComponent<Localize>();

            ui.countLabel = countLabelText;
        }
    }
}
