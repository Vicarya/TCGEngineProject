using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 汎用的なPlayerクラスを継承した、ヴァイスシュヴァルツ専用のプレイヤークラス。
    /// プレイヤーの意思決定を行うコントローラー(IWeissPlayerController)や、
    /// ゲーム固有のルールに関する状態（手札上限など）を保持します。
    /// </summary>
    public class WeissPlayer : Player
    {
        /// <summary>
        /// このプレイヤーの行動を決定するコントローラー（AI、UI経由の人間プレイヤーなど）への参照。
        /// </summary>
        public IWeissPlayerController Controller { get; }
        
        /// <summary>
        /// エンドフェイズに適用される手札の上限枚数。デフォルトは7枚。
        /// </summary>
        public int HandLimit { get; set; } = 7;

        /// <summary>
        /// 新しいプレイヤーインスタンスを初期化します。
        /// </summary>
        /// <param name="name">プレイヤー名。</param>
        /// <param name="controller">このプレイヤーを操作するコントローラー。</param>
        public WeissPlayer(string name, IWeissPlayerController controller) : base(name)
        {
            Controller = controller;
        }
    }
}