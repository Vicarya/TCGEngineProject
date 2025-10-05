using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// 特定のロジックを持たない、単純なフェーズを表すクラス。
    /// </summary>
    public class SimplePhase : PhaseBase
    {
        public SimplePhase(string id, string name) : base(id, name)
        {
        }

        // OnEnter, OnExit, Execute などのロジックは基底クラスの実装をそのまま使う
    }
}
