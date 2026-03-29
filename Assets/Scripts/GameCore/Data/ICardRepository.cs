using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// Runtimeカード永続化の共通契約。
    /// 他テーブル実装はこのインターフェースを満たすことで同一運用に揃える。
    /// </summary>
    public interface ICardRepository<TCardData> where TCardData : CardData
    {
        List<TCardData> LoadAll();
        void SaveAll(IEnumerable<TCardData> cards);
        void SaveOne(TCardData card);
    }
}
