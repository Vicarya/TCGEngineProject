using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// ゲーム進行の骨組みを定義する、全てのゲームロジックの抽象基底クラス。
    /// これを継承して、特定のゲーム（例：ヴァイスシュヴァルツ）のルールやフローを実装します。
    /// </summary>
    public abstract class GameBase
    {
        /// <summary>
        /// このゲームの現在の状態を取得または設定します。
        /// </summary>
        public GameState GameState { get; protected set; }

        // TODO: フェーズ管理もGameStateに移管予定
        /// <summary>
        /// 現在のフェーズ。将来的にはGameStateに管理を移管することが検討されています。
        /// </summary>
        protected PhaseBase currentPhase;

        /// <summary>
        /// GameBaseの新しいインスタンスを初期化します。
        /// </summary>
        protected GameBase()
        {
            GameState = CreateGameState();
        }

        /// <summary>
        /// このゲームに対応するGameStateのインスタンスを生成します。
        /// </summary>
        /// <returns>生成されたGameStateインスタンス。</returns>
        protected virtual GameState CreateGameState()
        {
            return new GameState(this);
        }

        /// <summary>
        /// ゲームを開始します。
        /// </summary>
        public virtual void StartGame()
        {
            SetupGame(this.GameState);
        }

        /// <summary>
        /// ゲームのフレーム更新処理。継続的な効果や時間経過の処理をここに実装できます。
        /// </summary>
        public virtual void Update() { }

        /// <summary>
        /// ゲームの初期セットアップを行います。プレイヤーの配置や山札の準備など。
        /// </summary>
        /// <param name="state">セットアップ対象のゲーム状態。</param>
        protected abstract void SetupGame(GameState state);

        /// <summary>
        /// 次のプレイヤーのターンに進めます。
        /// </summary>
        /// <param name="state">更新対象のゲーム状態。</param>
        public virtual void NextTurn(GameState state)
        {
            state.CurrentPlayerIndex = (state.CurrentPlayerIndex + 1) % state.Players.Count;
        }

        /// <summary>
        /// 次のフェーズに進めます。
        /// </summary>
        /// <param name="state">更新対象のゲーム状態。</param>
        public virtual void NextPhase(GameState state)
        {
            if (currentPhase != null)
            {
                // TODO: フェーズ遷移のロジックをGameStateと連携する形に修正
                // currentPhase = currentPhase.GetNextPhase(); 
            }
        }
    }
}
