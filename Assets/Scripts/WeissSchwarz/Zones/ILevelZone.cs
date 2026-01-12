using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    /// <summary>
    /// レベル置場ゾーンが公開する機能を定義するインターフェース。
    /// </summary>
    public interface ILevelZone<TCard> : IZone<TCard> where TCard : Card
    {
        /// <summary>
        /// レベル置場にあるカードと、その向き（表向きか裏向きか）のリストを取得します。
        /// </summary>
        IReadOnlyList<(TCard Card, bool FaceUp)> LevelCards { get; }
    }
}
