using System;
using System.Collections.Generic;
using System.Linq;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Represents a cost that requires discarding a certain number of cards from hand that meet specific criteria.
    /// </summary>
    public class DiscardCost : ICost
    {
        private readonly int _amount;
        private readonly Predicate<WeissCard> _filter;

        public DiscardCost(int amount, Predicate<WeissCard> filter)
        {
            _amount = amount;
            _filter = filter ?? (card => true); // If no filter, any card is valid
        }

        public bool CanPay(GameState state, Player player)
        {
            var hand = player.GetZone<IHandZone<WeissCard>>();
            if (hand == null) return false;

            return hand.Cards.Count(card => _filter(card)) >= _amount;
        }

        public void Pay(GameState state, Player player)
        {
            if (!CanPay(state, player)) return; // Should not happen if checked before, but as a safeguard

            var hand = player.GetZone<IHandZone<WeissCard>>();
            var waitingRoom = player.GetZone<IDiscardPile<WeissCard>>();
            var controller = (player as WeissPlayer)?.Controller;

            if (hand == null || waitingRoom == null || controller == null) return;

            var validCardsToDiscard = hand.Cards.Where(card => _filter(card)).ToList();

            string reason = $"Choose {_amount} card(s) to discard from your hand.";
            var cardsToDiscard = controller.SelectCardsToPayCost(player as WeissPlayer, validCardsToDiscard, _amount, reason);

            if (cardsToDiscard != null && cardsToDiscard.Count == _amount)
            {
                foreach (var card in cardsToDiscard)
                {
                    hand.RemoveCard(card);
                    waitingRoom.AddCard(card);
                }
            }
            // else: The player failed to select the correct number of cards. In a real interactive scenario,
            // we might loop, but for the simulation we assume the controller returns a valid selection if CanPay was true.
        }
    }
}