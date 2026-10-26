using System;
using System.Collections.Generic;
using System.Text;

namespace Conductor.Extensions
{
    public static class DeckScreenExtensions
    {
        internal static ICardEffect? RequestingCardEffect = null;

        public static void SetRequestingCardEffect(this DeckScreen @params, ICardEffect requestingCardEffect)
        {
            RequestingCardEffect = requestingCardEffect;
        }
    }
}
