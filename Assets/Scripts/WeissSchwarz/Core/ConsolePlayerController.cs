using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TCG.Core;
using UnityEngine;

namespace TCG.Weiss
{
    /// <summary>
    /// IWeissPlayerControllerインターフェースの実装。
    /// 実際のプレイヤー入力の代わりに、あらかじめ定義された単純なロジックに基づいて自動的に行動を選択する。
    /// 主に、UIを介さないゲームロジックのテストやデバッグを目的として使用される「ボット」または「AI」の役割を果たす。
    /// </summary>
    public class ConsolePlayerController : IWeissPlayerController
    {
        // ターンごとに一度だけ起動能力を使うためのフラグ
        private bool _hasAutoUsedAbilityThisTurn = false;
        // ターンごとに一度だけアタックするためのフラグ
        private int _attacksChosenThisTurn = 0;

        /// <summary>
        /// ターン開始時に内部状態をリセットします。
        /// </summary>
        public void ResetTurnState()
        {
            _hasAutoUsedAbilityThisTurn = false;
            _attacksChosenThisTurn = 0;
        }

        /// <summary>
        /// メインフェイズにどのアクション（カードプレイ、能力起動、フェイズ終了）を実行するか選択します。
        /// </summary>
        /// <param name="player">アクションを選択するプレイヤー。</param>
        /// <returns>選択されたメインフェイズのアクション。</returns>
        public MainPhaseAction ChooseMainPhaseAction(WeissPlayer player)
        {
            Debug.Log($"--- {player.Name}のメインフェイズ ---");
            Debug.Log("アクションを選択してください:");
            Debug.Log("1: 手札からカードをプレイする");
            Debug.Log("2: 起動能力を使う");
            Debug.Log("3: メインフェイズを終了する");

            // AIロジック: このターンまだ能力を使っていなければ、能力使用を試みる
            if (!_hasAutoUsedAbilityThisTurn)
            {
                _hasAutoUsedAbilityThisTurn = true;
                Debug.Log("プレイヤー入力をシミュレート: 2 (能力を使用)");
                return MainPhaseAction.UseAbility;
            }
            
            // それ以外の場合はフェイズを終了する
            Debug.Log("プレイヤー入力をシミュレート: 3 (フェイズ終了)");
            return MainPhaseAction.EndPhase;
        }

        /// <summary>
        /// 起動する能力を選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="activatableAbilities">起動可能な能力のリスト。</param>
        /// <returns>選択された能力（カードと能力テキストのペア）。</returns>
        public KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities)
        {
            Debug.Log("--- " + player.Name + ": 起動する能力を選択 ---");
            if (activatableAbilities == null || !activatableAbilities.Any())
            {
                Debug.Log("起動可能な能力はありません。");
                return default;
            }

            for (int i = 0; i < activatableAbilities.Count; i++)
            {
                var ability = activatableAbilities[i];
                Debug.Log($"{i + 1}: [{ability.Key.Data.CardCode} {ability.Key.Data.Name}] - {ability.Value}");
            }
            Debug.Log($"{activatableAbilities.Count + 1}: 能力を起動しない");

            // AIロジック: 最初の能力を選択する
            var choice = activatableAbilities.FirstOrDefault();
            Debug.Log($"プレイヤー入力をシミュレート: 1 ({choice.Value})");

            return choice;
        }

        /// <summary>
        /// 手札からプレイするカードを1枚選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたカード。選択しない場合はnull。</returns>
        public WeissCard ChooseCardFromHand(WeissPlayer player, bool optional)
        {
            Debug.Log($"--- {player.Name}: 手札からカードを選択 ---");
            var hand = player.GetZone<IHandZone<WeissCard>>().Cards;

            if (hand == null || !hand.Any())
            {
                Debug.Log("手札にカードがありません。");
                return null;
            }

            Debug.Log("あなたの手札:");
            for (int i = 0; i < hand.Count; i++)
            {
                Debug.Log($"{i + 1}: [{hand[i].Data.CardCode}] {hand[i].Data.Name}");
            }

            // AIロジック: 任意の場合は何も選択せず、必須の場合は最初のカードを選択する
            if (optional)
            {
                Debug.Log("プレイヤー入力をシミュレート: 何も選択しない。");
                return null;
            }
            else
            {
                var choice = hand.First();
                Debug.Log($"プレイヤー入力をシミュレート: 最初のカードを選択 [{choice.Data.Name}]");
                return choice;
            }
        }

        /// <summary>
        /// 手札からプレイするクライマックスカードを1枚選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたクライマックスカード。</returns>
        public WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional)
        {
            // (略) AIロジック: 常に何も選択しない
            return null;
        }

        /// <summary>
        /// アタックフェイズを終了するか、攻撃を続行するか選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <returns>アタックを終了する場合はtrue。</returns>
        public bool ChooseToEndAttack(WeissPlayer player)
        {
            Debug.Log("--- " + player.Name + ": 攻撃を続けますか？ ---");
            Debug.Log("1: アタックを実行");
            Debug.Log("2: アタックフェイズを終了");

            // AIロジック: 1回だけアタックを試みる
            if (_attacksChosenThisTurn < 1)
            {
                Debug.Log("プレイヤー入力をシミュレート: 1 (アタックを実行)");
                _attacksChosenThisTurn++;
                return false; 
            }
            
            Debug.Log("プレイヤー入力をシミュレート: 2 (アタックフェイズを終了)");
            return true; 
        }

        /// <summary>
        /// アタックするキャラを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="attackableCharacters">アタック可能なキャラのリスト。</param>
        /// <returns>選択されたアタッカー。</returns>
        public WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters)
        {
            // (略) AIロジック: 最初にアタック可能なキャラを選択
            var choice = attackableCharacters.First();
            Debug.Log($"プレイヤー入力をシミュレート: [{choice.Data.Name}] を選択");
            return choice;
        }

        /// <summary>
        /// アタックの種類（フロントアタック、サイドアタック）を選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="attacker">アタッカー。</param>
        /// <param name="defender">ディフェンダー。</param>
        /// <returns>選択されたアタックタイプ。</returns>
        public AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender)
        {
            // (略) AIロジック: 常にフロントアタックを選択
            Debug.Log("プレイヤー入力をシミュレート: 1 (フロントアタック)");
            return AttackType.Front;
        }

        /// <summary>
        /// ゲーム開始時のマリガンで、手札から交換するカードを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="hand">初期手札。</param>
        /// <returns>交換するカードのリスト。</returns>
        public Task<List<WeissCard>> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand)
        {
            // (略) AIロジック: 手札にクライマックスカードがあれば、それをマリガンする
            var cardsToMulligan = new List<WeissCard>();
            var climaxInHand = hand.FirstOrDefault(c => ((c.Data as WeissCardData)?.CardType == WeissCardType.Climax.ToString()));

            if (climaxInHand != null)
            {
                Debug.Log($"プレイヤー入力をシミュレート: クライマックスカード1枚をマリガン: [{climaxInHand.Data.Name}]");
                cardsToMulligan.Add(climaxInHand);
            }
            else
            {
                Debug.Log("プレイヤー入力をシミュレート: 手札をキープ。");
            }

            return Task.FromResult(cardsToMulligan);
        }

        /// <summary>
        /// 控え室からカードを1枚選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="cards">選択肢となるカードのリスト。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択されたカード。</returns>
        public WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional)
        {
            // (略) AIロジック: 任意なら何も選ばず、必須なら最初のカードを選ぶ
            if (optional)
            {
                Debug.Log("プレイヤー入力をシミュレート: 何も選択しない。");
                return null;
            }
            else
            {
                var choice = cards.First();
                Debug.Log($"プレイヤー入力をシミュレート: 最初のカードを選択 [{choice.Data.Name}]");
                return choice;
            }
        }

        /// <summary>
        /// 手札から使用する助太刀（カウンター）カードを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="counterCards">使用可能な助太刀カードのリスト。</param>
        /// <returns>選択された助太刀カード。</returns>
        public WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards)
        {
            // (略) AIロジック: 常に助太刀を使用しない
            Debug.Log("プレイヤー入力をシミュレート: 何も選択しない。");
            return null;
        }

        /// <summary>
        /// キャラがリバースした際に、アンコールを行うかどうか、どの方法で行うかを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="character">リバースしたキャラ。</param>
        /// <returns>選択されたアンコール方法。</returns>
        public EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character)
        {
            // (略) AIロジック: 特殊アンコールがあればそれを、なければ通常のアンコールを試みる
            if (options.Any(o => o.Key == EncoreChoice.Special))
            {
                Debug.Log("プレイヤー入力をシミュレート: 1 (特殊アンコール)");
                return EncoreChoice.Special;
            }
            if (options.Any(o => o.Key == EncoreChoice.Standard))
            {
                Debug.Log("プレイヤー入力をシミュレート: 1 (通常アンコール)");
                return EncoreChoice.Standard;
            }

            Debug.Log("プレイヤー入力をシミュレート: 何も選択しない。");
            return EncoreChoice.None;
        }

        /// <summary>
        /// レベルアップ時に、クロック置場からレベル置場に置くカードを1枚選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="cards">クロック置場のカードリスト。</param>
        /// <returns>選択されたカード。</returns>
        public WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards)
        {
            // (略) AIロジック: クライマックス以外を優先して選択する
            var choice = cards.FirstOrDefault(c => ((c.Data as WeissCardData)?.CardType != WeissCardType.Climax.ToString())) ?? cards.First();
            Debug.Log($"プレイヤー入力をシミュレート: [{choice.Data.Name}] を選択");
            return choice;
        }

        /// <summary>
        /// プレイヤーにYes/Noの質問を問いかけ、その選択を取得します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="question">問いかける質問文。</param>
        /// <returns>プレイヤーの選択（Yesならtrue）。</returns>
        public bool AskYesNo(WeissPlayer player, string question)
        {
            // (略) AIロジック: 常にYesと答える
            Debug.Log("プレイヤー入力をシミュレート: 1 (Yes)");
            return true;
        }

        /// <summary>
        /// 効果の対象となるカードを1枚選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="validTargets">選択可能な対象カードのリスト。</param>
        /// <param name="prompt">選択を促すメッセージ。</param>
        /// <param name="optional">選択が任意かどうか。</param>
        /// <returns>選択された対象カード。</returns>
        public WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional)
        {
            // (略) AIロジック: 任意なら何も選ばず、必須なら最初の対象を選ぶ
            if (optional)
            {
                Debug.Log("プレイヤー入力をシミュレート: 何も選択しない。");
                return null;
            }
            else
            {
                var choice = validTargets.First();
                Debug.Log($"プレイヤー入力をシミュレート: 最初の対象を選択 [{choice.Data.Name}]");
                return choice;
            }
        }

        /// <summary>
        /// 同時に誘発した複数の自動能力のうち、どれを先に解決するかを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="abilities">解決待ちの能力のリスト。</param>
        /// <returns>先に解決する能力。</returns>
        public PendingAbility ChooseAbilityToResolve(WeissPlayer player, List<PendingAbility> abilities)
        {
            // (略) AIロジック: 常にリストの最初の能力を選択する
            var choice = abilities.First();
            Debug.Log($"プレイヤー入力をシミュレート: 1 (最初に '{choice.AbilityText}' を解決)");
            return choice;
        }

        /// <summary>
        /// コストとして支払うカードを選択します。
        /// </summary>
        /// <param name="player">プレイヤー。</param>
        /// <param name="options">選択肢となるカード。</param>
        /// <param name="amount">選択する枚数。</param>
        /// <param name="reason">コスト支払いの理由。</param>
        /// <returns>コストとして支払うために選択されたカードのリスト。</returns>
        public List<WeissCard> SelectCardsToPayCost(WeissPlayer player, List<WeissCard> options, int amount, string reason)
        {
            // (略) AIロジック: リストの先頭から必要な枚数だけ選択する
            string simulatedInput = string.Join(" ", Enumerable.Range(1, amount).Select(i => i.ToString()));
            Debug.Log($"プレイヤー入力をシミュレート: {simulatedInput}");
            
            // (略)
            var selection = indices.Select(i => options[i]).ToList();
            return selection;
        }
    }
}
