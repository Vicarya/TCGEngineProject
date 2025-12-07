using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// 全てのカードの非ジェネリックな基底クラス。
    /// カードの物理的な状態（場所、向き、タップ状態など）や所有者を管理します。
    /// ゲーム固有のデータ（パワー、テキストなど）はジェネリック版のCardBase&lt;T&gt;で扱われます。
    /// </summary>
    public abstract class Card
    {
        /// <summary>
        /// このカードの所有者を取得または設定します。
        /// </summary>
        public Player Owner { get; set; }

        /// <summary>
        /// このカードが現在存在するゾーンを取得または設定します。
        /// </summary>
        public IZone CurrentZone { get; set; }

        /// <summary>
        /// カードがタップ（横向き）されているかどうかを取得または設定します。
        /// </summary>
        public bool IsTapped { get; set; }

        /// <summary>
        /// カードが表向きかどうかを取得または設定します。
        /// </summary>
        public bool IsFaceUp { get; set; }

        /// <summary>
        /// カードがレスト（行動済み）状態かどうかを取得します。
        /// </summary>
        public bool IsRested { get; protected set; }

        /// <summary>
        /// このカードが持つ能力のリスト。
        /// </summary>
        private readonly List<AbilityBase> _abilities = new();

        /// <summary>
        /// このカードが持つ能力の読み取り専用リストを取得します。
        /// </summary>
        public IReadOnlyList<AbilityBase> Abilities => _abilities.AsReadOnly();

        /// <summary>
        /// このカードを指定された新しいゾーンに移動させます。
        /// </summary>
        /// <param name="newZone">移動先のゾーン。</param>
        public virtual void MoveTo(IZone newZone)
        {
            // 現在のゾーンからカードを削除
            CurrentZone?.RemoveCard(this);
            // 新しいゾーンにカードを追加
            newZone.AddCard(this);
            // 現在のゾーンを更新
            CurrentZone = newZone;
        }

        /// <summary>
        /// このカードに新しい能力を追加します。
        /// </summary>
        /// <param name="a">追加する能力。</param>
        public void AddAbility(AbilityBase a)
        {
            a.SourceCard = this;
            _abilities.Add(a);
        }

        /// <summary>
        /// このカードから指定された能力を削除します。
        /// </summary>
        /// <param name="a">削除する能力。</param>
        public void RemoveAbility(AbilityBase a)
        {
            _abilities.Remove(a);
            a.SourceCard = null;
        }

        /// <summary>
        /// このカードが持つ能力をゲームのイベントシステムに登録します。
        /// （リファクタリング後の実装待ち）
        /// </summary>
        /// <param name="state">現在のゲーム状態。</param>
        public void SubscribeAbilities(GameState state)
        {
            // TODO: リファクタリング後に能力の購読ロジックを実装する
        }

        /// <summary>
        /// カードをレスト（行動済み）またはスタンド（未行動）状態に設定します。
        /// </summary>
        /// <param name="rested">レスト状態にする場合はtrue、スタンド状態にする場合はfalse。</param>
        public virtual void SetRested(bool rested)
        {
            IsRested = rested;
        }
    }
}
