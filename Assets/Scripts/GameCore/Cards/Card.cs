using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// 全てのカードの非ジェネリックな基底クラス。
    /// カードの物理的な状態や所有者を管理します。
    /// </summary>
    public abstract class Card
    {
        public Player Owner { get; set; }
        public IZone CurrentZone { get; set; }
        public bool IsTapped { get; set; }
        public bool IsFaceUp { get; set; }
    public bool IsRested { get; protected set; }
        
        private readonly List<AbilityBase> _abilities = new();
        public IReadOnlyList<AbilityBase> Abilities => _abilities.AsReadOnly();

        public virtual void MoveTo(IZone newZone)
        {
            CurrentZone?.RemoveCard(this);
            newZone.AddCard(this);
            CurrentZone = newZone;
        }

        public void AddAbility(AbilityBase a)
        {
            a.SourceCard = this;
            _abilities.Add(a);
        }

        public void RemoveAbility(AbilityBase a)
        {
            _abilities.Remove(a);
            a.SourceCard = null;
        }

        public void SubscribeAbilities(GameState state)
        {
            // TODO: Implement ability subscription logic after refactoring.
        }

        public virtual void SetRested(bool rested)
        {
            IsRested = rested;
        }
    }
}
