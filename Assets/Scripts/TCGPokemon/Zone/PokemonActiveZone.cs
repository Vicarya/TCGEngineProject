// namespace TCG.Pokemon
// {
//     using TCG.Core;

//     public class PokemonActiveZone : ZoneBase
//     {
//         public PokemonActiveZone(Player owner) : base("Active", owner) {}

//         public override void AddCard(CardBase card)
//         {
//             if (cards.Count >= 1)
//                 throw new System.InvalidOperationException("Active slot already occupied!");
//             base.AddCard(card);
//         }
//     }
// }
