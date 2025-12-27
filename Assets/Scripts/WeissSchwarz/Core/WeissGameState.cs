
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 汎用的なGameStateを継承し、ヴァイスシュヴァルツのゲーム状態全体を管理するクラス。
    /// ヴァイスシュヴァルツ固有のゲーム状態（この場合はAbilityQueue）を追加で保持します。
    /// </summary>
    public class WeissGameState : GameState
    {
        /// <summary>
        /// 解決待ちの自動能力を管理するキュー。
        /// ヴァイスシュヴァルツの割り込み処理を実現するための重要な要素です。
        /// </summary>
        public AbilityQueue AbilityQueue { get; }

        /// <summary>
        /// WeissGameStateの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="game">このゲーム状態が属するGameインスタンス。</param>
        public WeissGameState(GameBase game) : base(game)
        {
            AbilityQueue = new AbilityQueue();
        }
    }
}
