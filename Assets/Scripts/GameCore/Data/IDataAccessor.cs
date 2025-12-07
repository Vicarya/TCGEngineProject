using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// データアクセスコンポーネントのためのインターフェース。
    /// データベース、ファイル、ネットワークなど、特定のデータソースからデータを取得する責務を抽象化します。
    /// </summary>
    /// <typeparam name="T">アクセスするデータの型。</typeparam>
    public interface IDataAccessor<T>
    {
        /// <summary>
        /// 全てのデータ項目を取得します。
        /// </summary>
        /// <returns>データ項目の列挙可能なコレクション。</returns>
        IEnumerable<T> GetAll();
    }
}
