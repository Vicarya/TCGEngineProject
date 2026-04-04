using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// Weiss向けゾーン基底。
    /// 公開情報の可視性(IsPublic)を持つ。
    /// </summary>
    public abstract class WeissZone : ZoneBase<WeissCard>
    {
        public bool IsPublic { get; }

        protected WeissZone(string name, Player owner, bool isPublic)
            : base(name, owner)
        {
            IsPublic = isPublic;
        }
    }
}
