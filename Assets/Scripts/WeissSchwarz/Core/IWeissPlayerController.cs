using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    public interface IWeissPlayerController : IPlayerController<WeissPlayer, WeissCard>
    {
        MainPhaseAction ChooseMainPhaseAction(WeissPlayer player);
        WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional);
        AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender);
    }
}
