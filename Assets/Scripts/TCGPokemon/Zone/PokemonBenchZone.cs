// namespace TCG.Pokemon
// {
//     using TCG.Core;

//     public class PokemonBenchZone : ZoneBase
//     {
//         private const int MaxBenchSize = 5;

//         public PokemonBenchZone(Player owner) : base("Bench", owner) {}

//         public override void AddCard(CardBase card)
//         {
//             if (cards.Count >= MaxBenchSize)
//             {
//                 throw new System.InvalidOperationException("Bench is full!");
//             }
//             base.AddCard(card);
//         }
//     }
// }
