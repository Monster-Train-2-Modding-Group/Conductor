namespace Conductor.Interfaces
{
    public interface IRewardPreviewProvider
    {
        public struct RewardPreviewInfo
        {
            public CardData? card;
            public List<CardUpgradeData>? upgrades;
            public CardStateModifiers? cardStateModifiers;
        }
        RewardPreviewInfo ProvidePreview(SaveManager saveManager, RelicManager relicManager);

        string GetSpecificRewardName();
    }
}
