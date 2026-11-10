using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Interfaces
{

    [Obsolete("Use IRewardWithCards instead. This interface will be removed on 11/24/2026.")]
    public interface ICardGiverReward
    {
        List<CardData> GetAllCardData();
    }

    public interface IRewardWithCards
    {
        List<CardData> GetAllCardData();
    }
}
