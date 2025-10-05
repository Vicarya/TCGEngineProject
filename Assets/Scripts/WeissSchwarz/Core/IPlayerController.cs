using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    public interface IPlayerController
    {
        WeissCard ChooseCardFromHand(Player player, bool optional);
        WeissCard ChooseClimaxFromHand(Player player, bool optional);
    }
}