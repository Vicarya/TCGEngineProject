using TCG.Core;
using System;

namespace TCG.Weiss {
    /// <summary>
    /// ヴァイスシュヴァルツのゲーム中におけるカード1枚の実体（インスタンス）を表すクラス。
    /// 汎用的な CardBase&lt;WeissCardData&gt; を継承しています。
    /// カードの永続的なデータ（WeissCardData）とは異なり、
    /// 場所、向き（スタンド/レスト）、リバース状態、一時的なパワー修整など、ゲーム中の動的な状態を管理します。
    /// </summary>
    public class WeissCard : CardBase<WeissCardData>
    {
        /// <summary>
        /// カードがバトルで負けてリバースしている状態かどうかを取得します。
        /// </summary>
        public bool IsReversed { get; private set; }
        
        /// <summary>
        /// カード効果によって一時的に増減したパワーの値。
        /// この値はターン終了時などにリセットされます。永続的なパワーは `Data.Power` を参照。
        /// </summary>
        public int TemporaryPower { get; set; }

        /// <summary>
        /// カードデータとオーナープレイヤーから、新しいカードインスタンスを生成します。
        /// </summary>
        /// <param name="data">このカードの永続的なカードデータ。</param>
        /// <param name="owner">このカードを所有するプレイヤー。</param>
        public WeissCard(WeissCardData data, Player owner) : base(data, owner) 
        {
            IsReversed = false;
            TemporaryPower = 0;

            // データ駆動設計の核心部分：
            // AbilityFactoryを使い、カードデータに定義されたテキストから能力オブジェクトを動的に生成し、
            // このカードインスタンスにアタッチ（追加）します。
            var createdAbilities = AbilityFactory.CreateAbilitiesForCard(this);
            foreach (var ability in createdAbilities)
            {
                this.AddAbility(ability);
            }
        }

        /// <summary>
        /// カードをレスト状態（横向き）にします。
        /// これはGameCoreの「タップ」状態に相当します。
        /// </summary>
        public void Rest()
        {
            SetRested(true); // 内部状態の更新
            IsTapped = true; // GameCoreの基底クラスの状態も更新
        }

        /// <summary>
        /// カードをスタンド状態（縦向き）にします。
        /// </summary>
        public void Stand()
        {
            SetRested(false);
            IsTapped = false;
        }

        /// <summary>
        /// カードのリバース状態を設定します。
        /// </summary>
        /// <param name="reversed">リバース状態にする場合はtrue、そうでない場合はfalse。</param>
        public void SetReversed(bool reversed)
        {
            IsReversed = reversed;
        }
    }
}
