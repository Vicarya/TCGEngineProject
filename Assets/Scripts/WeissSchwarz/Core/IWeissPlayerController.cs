using System.Collections.Generic;

namespace TCG.Weiss
{
    public interface IWeissPlayerController
    {
        void ResetTurnState();
        MainPhaseAction ChooseMainPhaseAction(WeissPlayer player);
        WeissCard ChooseCardFromHand(WeissPlayer player, bool optional);
        WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional);
        List<WeissCard> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand);
        WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters);
        AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender);
        bool ChooseToEndAttack(WeissPlayer player);
        WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards);
        WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional);
        KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities);
        EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character);
        WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards);
        bool AskYesNo(WeissPlayer player, string question);
        WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional);
    }
}
