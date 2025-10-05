using System.Collections.Generic;
using System.Linq;
using TCG.Core;
using TCG.Weiss;

namespace WeissSchwarz.Abilities
{
    public class ConcentrateEffect : IEffect
    {
        public void Resolve(GameEvent e, GameState state, Card source)
        {
            var player = state.ActivePlayer as WeissPlayer;
            if (player == null) return;

            // TODO: player.Deck は WeissPlayer にあるべきプロパティ。一旦仮定。
            // var deck = player.Deck;
            // var discarded = new List<Card>();

            // for (int i = 0; i < 4; i++)
            // {
            //     var card = deck.DrawTop();
            //     if (card != null) discarded.Add(card);
            // }

            // int climaxCount = discarded.Count(c => (c.Data as WeissCardData)?.IsClimax ?? false);
            // for (int i = 0; i < climaxCount; i++)
            // {
            //     // TODO: Player.Draw() は存在しない。RuleEngineなどを介して実装する必要がある。
            //     // player.Draw(1);
            // }
        }
    }
}
