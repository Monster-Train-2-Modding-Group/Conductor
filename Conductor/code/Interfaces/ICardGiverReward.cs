using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{

    [Obsolete("Use Conductor.MonsterTrain2 instead. Interface name changing to IRewardWithCards.")]
    public interface ICardGiverReward
    {
        List<CardData> GetAllCardData();
    }

    public interface IRewardWithCards
    {
        List<CardData> GetAllCardData();
    }
}
