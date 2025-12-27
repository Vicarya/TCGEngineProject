using System.Collections.Generic;
using System.Threading.Tasks;

namespace TCG.Weiss
{
    /// <summary>
    /// プレイヤーの意思決定を抽象化するインターフェース。
    /// ゲームエンジンは、このインターフェースを介してプレイヤーに選択を要求します。
    /// これにより、UIを持つ人間プレイヤー、AI、デバッグ用のコンソールなど、
    /// 異なるタイプのプレイヤーコントローラーを同じように扱うことができます（ストラテジーパターン）。
    /// </summary>
    public interface IWeissPlayerController
    {
        /// <summary>
        /// ターン開始時にコントローラーの内部状態をリセットします。
        /// </summary>
        void ResetTurnState();

        /// <summary>
        /// メインフェイズにどのアクション（カードプレイ、能力起動、フェイズ終了）を実行するか選択します。
        /// </summary>
        MainPhaseAction ChooseMainPhaseAction(WeissPlayer player);

        /// <summary>
        /// 手札からプレイまたは効果で使用するカードを1枚選択します。
        /// </summary>
        WeissCard ChooseCardFromHand(WeissPlayer player, bool optional);

        /// <summary>
        /// 手札からプレイするクライマックスカードを1枚選択します。
        /// </summary>
        WeissCard ChooseClimaxFromHand(WeissPlayer player, bool optional);

        /// <summary>
        /// ゲーム開始時のマリガンフェイズで、手札から交換するカード（複数選択可）を非同期で選択します。
        /// </summary>
        Task<List<WeissCard>> ChooseMulliganCards(WeissPlayer player, List<WeissCard> hand);

        /// <summary>
        /// アタックフェイズに、攻撃を行うキャラを1枚選択します。
        /// </summary>
        WeissCard ChooseAttacker(WeissPlayer player, List<WeissCard> attackableCharacters);

        /// <summary>
        /// アタックの種類（フロントアタック、サイドアタックなど）を選択します。
        /// </summary>
        AttackType ChooseAttackType(WeissPlayer player, WeissCard attacker, WeissCard defender);

        /// <summary>
        /// アタックフェイズを終了するか、次の攻撃を続けるかを選択します。
        /// </summary>
        bool ChooseToEndAttack(WeissPlayer player);

        /// <summary>
        /// 手札から使用する助太刀（カウンター）カードを選択します。
        /// </summary>
        WeissCard ChooseCounterCardFromHand(WeissPlayer player, List<WeissCard> counterCards);

        /// <summary>
        /// 控え室からカードを1枚選択します。
        /// </summary>
        WeissCard ChooseCardFromWaitingRoom(WeissPlayer player, List<WeissCard> cards, bool optional);

        /// <summary>
        /// 起動（【起】）能力を使用する場合、どのカードのどの能力を起動するかを選択します。
        /// </summary>
        KeyValuePair<WeissCard, string> ChooseAbilityToActivate(WeissPlayer player, List<KeyValuePair<WeissCard, string>> activatableAbilities);

        /// <summary>
        /// キャラがリバースした際に、アンコールを行うかどうか、どの方法で行うかを選択します。
        /// </summary>
        EncoreChoice ChooseToEncore(WeissPlayer player, WeissCard character);

        /// <summary>
        /// レベルアップ時に、クロック置場からレベル置場に置くカードを1枚選択します。
        /// </summary>
        WeissCard ChooseLevelUpCard(WeissPlayer player, List<WeissCard> cards);

        /// <summary>
        /// カードの効果などによって発生する、Yes/Noの選択をプレイヤーに要求します。
        /// </summary>
        bool AskYesNo(WeissPlayer player, string question);

        /// <summary>
        /// カードの効果の対象となるカードを1枚選択します。
        /// </summary>
        WeissCard ChooseTargetCard(WeissPlayer player, List<WeissCard> validTargets, string prompt, bool optional);

        /// <summary>
        /// 同時に誘発した複数の自動能力（【自】）のうち、どれを先に解決するかを選択します。
        /// </summary>
        PendingAbility ChooseAbilityToResolve(WeissPlayer player, List<PendingAbility> abilities);

        /// <summary>
        /// 能力のコストとして、指定された場所から指定された枚数のカードを選択します。
        /// </summary>
        List<WeissCard> SelectCardsToPayCost(WeissPlayer player, List<WeissCard> options, int amount, string reason);
    }
}
