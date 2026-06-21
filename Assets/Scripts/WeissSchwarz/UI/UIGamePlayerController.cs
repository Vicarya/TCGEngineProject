using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TCG.Core;
using TCG.Weiss.UI;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// ヴァイスシュヴァルツのゲームにおいて、プレイヤーの操作をUIを通じて受け付けるコントローラー。
    /// ゲームのロジック層からの要求（例: カードの選択、行動の決定）に対し、
    /// UIを介した入力待ちを行い、結果をゲームロジックに返します。
    /// IWeissPlayerControllerインターフェースを実装しており、実際のプレイヤー操作をUnity UIにマッピングします。
    /// </summary>
    public class UIGamePlayerController : IWeissPlayerController
    {
        /// <summary>
        /// ターン状態をリセットする際に呼び出されます。
        /// 必要に応じて、UI固有のリセット処理をここに追加できます。
        /// </summary>
        public void ResetTurnState()
        {
            Debug.Log("UIGamePlayerController: ResetTurnState called.");
            // UI-specific reset logic here
        }

        /// <summary>
        /// メインフェイズ中のプレイヤーの行動選択を要求します。
        /// GameViewの中央モーダルに委譲し、プレイヤーが選択するまで非同期で待機します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <returns>プレイヤーが選択したメインフェイズのアクション。</returns>
        public async Task<MainPhaseAction> ChooseMainPhaseAction(WeissPlayer player)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}'s Main Phase. Waiting for UI input.");
            if (GameView.Instance == null)
            {
                Debug.LogWarning("UIGamePlayerController: GameView.Instance is null. EndPhase will be returned.");
                return MainPhaseAction.EndPhase;
            }

            return await GameView.Instance.RequestMainPhaseActionAsync();
        }

        /// <summary>
        /// アクティブにする能力の選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="activatableAbilities">選択可能な能力のリスト。</param>
        /// <returns>選択された能力と、その能力テキスト。</returns>
        public KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose an ability to activate. Waiting for UI input.");
            return default;
        }

        /// <summary>
        /// 手札からのカード選択を要求します。
        /// 手札全体を選択候補としてGameViewへ渡し、任意選択時だけ未選択での進行を許可します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたカード、または任意の場合はnull。</returns>
        public async Task<WeissCard> ChooseCardFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a card from hand. Waiting for UI input.");
            if (GameView.Instance == null)
            {
                Debug.LogWarning("UIGamePlayerController: GameView.Instance is null. No hand card will be selected.");
                return null;
            }

            var handCards = player.GetZone<IHandZone<WeissCard>>()?.Cards ?? new List<WeissCard>();
            return await GameView.Instance.RequestHandCardSelectionAsync(
                player,
                handCards,
                optional,
                title: "手札選択",
                message: optional
                    ? "手札からカードを1枚選択してください。選ばずに進むこともできます。"
                    : "手札からカードを1枚選択してください。");
        }

        /// <summary>
        /// 手札からのクライマックスカード選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたクライマックスカード、または任意の場合はnull。</returns>
        public WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a climax card from hand. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// アタックを継続するかどうかの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <returns>アタックを継続する場合はtrue、終了する場合はfalse。</returns>
        public bool ChooseToEndAttack(WeissPlayer player)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Continue attacking? Waiting for UI input.");
            return true; // Simulate ending attack for now
        }

        /// <summary>
        /// アタッカーとなるキャラクターの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="attackableCharacters">攻撃可能なキャラクターのリスト。</param>
        /// <returns>選択されたアタッカーカード。</returns>
        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose an attacker. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// アタックのタイプ（例: フロントアタック）の選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="attacker">アタッカーのカード。</param>
        /// <param name="defender">ディフェンダーのカード（対戦相手のキャラやプレイヤーなど）。</param>
        /// <returns>選択されたアタックタイプ。</returns>
        public AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose attack type. Waiting for UI input.");
            return AttackType.Front; // Simulate Front Attack
        }

        /// <summary>
        /// マリガンするカードの選択を非同期で要求します。
        /// UIにマリガン選択モードへの移行を指示し、プレイヤーの選択結果を待ちます。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="hand">現在の手札のカードリスト。</param>
        /// <returns>マリガンするために選択されたカードのリスト。</returns>
        public async Task<List<WeissCard>> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}'s Mulligan Phase. Waiting for UI input.");
            if (GameView.Instance == null)
            {
                Debug.LogWarning("UIGamePlayerController: GameView.Instance is null. Mulligan will be skipped.");
                return new List<WeissCard>();
            }

            // 中央モーダルにマリガン入力を委譲して待機する。
            var selectedCards = await GameView.Instance.RequestMulliganAsync(player);

            Debug.Log($"UIGamePlayerController: Mulligan confirmed with {selectedCards.Count} cards.");
            return selectedCards;
        }

        /// <summary>
        /// 控え室からのカード選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="cards">控え室にあるカードのリスト。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたカード、または任意の場合はnull。</returns>
        public WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose a card from waiting room. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// 手札からのカウンターカード選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="counterCards">選択可能なカウンターカードのリスト。</param>
        /// <returns>選択されたカウンターカード。</returns>
        public WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose counter card. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// アンコールオプションの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="character">アンコール対象のキャラクター。</param>
        /// <returns>選択されたアンコールオプション。</returns>
        public EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose encore option. Waiting for UI input.");
            return EncoreChoice.None;
        }

        /// <summary>
        /// レベルアップ時に控え室からレベルゾーンに置くカードの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="cards">選択可能なカードのリスト（通常は控え室のカード）。</param>
        /// <returns>レベルアップのために選択されたカード。</returns>
        public WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose level up card. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// プレイヤーにYes/Noの質問を行い、その回答を待ちます。
        /// </summary>
        /// <param name="player">質問対象のプレイヤー。</param>
        /// <param name="question">プレイヤーに提示する質問文。</param>
        /// <returns>プレイヤーがYesを選択した場合はtrue、Noの場合はfalse。</returns>
        public async Task<bool> AskYesNo(WeissPlayer player, string question)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Ask Yes/No: {question}. Waiting for UI input.");
            if (GameView.Instance == null)
            {
                Debug.LogWarning("UIGamePlayerController: GameView.Instance is null. No will be returned.");
                return false;
            }

            return await GameView.Instance.RequestYesNoAsync("確認", question, false);
        }

        /// <summary>
        /// ターゲットカードの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="validTargets">選択可能なターゲットカードのリスト。</param>
        /// <param name="prompt">プレイヤーに表示するプロンプトメッセージ。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたターゲットカード、または任意の場合はnull。</returns>
        public WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose target card: {prompt}. Waiting for UI input.");
            return null;
        }

        /// <summary>
        /// 解決すべき能力の順序選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="abilities">選択可能な保留中の能力のリスト。</param>
        /// <returns>プレイヤーが選択した能力。</returns>
        public PendingAbility ChooseAbilityToResolve(WeissPlayer player, List<PendingAbility> abilities)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Choose ability order. Waiting for UI input.");
            return default;
        }

        /// <summary>
        /// コスト支払いのためカードの選択を要求します。
        /// </summary>
        /// <param name="player">現在のプレイヤー。</param>
        /// <param name="options">選択可能なカードのリスト。</param>
        /// <param name="amount">選択する必要があるカードの枚数。</param>
        /// <param name="reason">コスト支払いの理由。</param>
        /// <returns>コストとして選択されたカードのリスト。</returns>
        public List<WeissCard> SelectCardsToPayCost(WeissPlayer player, List<WeissCard> options, int amount, string reason)
        {
            Debug.Log($"UIGamePlayerController: {player.Name}: Select cards for cost: {reason}. Waiting for UI input.");
            return new List<WeissCard>();
        }
    }
}
