// public class PokemonCard : CardBase<PokemonCardData>
// {
//     public int CurrentHP { get; private set; }

//     public PokemonCard(PokemonCardData data, Player owner) : base(data, owner)
//     {
//         CurrentHP = data.HP;
//     }

//     public void TakeDamage(int damage)
//     {
//         CurrentHP = Mathf.Max(0, CurrentHP - damage);
//     }

//     public bool IsKnockedOut => CurrentHP <= 0;
// }
