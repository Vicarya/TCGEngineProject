using System;
using System.Collections.Generic;

namespace TCG.Core
{
    /// <summary>
    /// ゲームに参加するプレイヤーを表すクラス。
    /// プレイヤーの名前、ID、そしてカードが存在する様々なゾーン（手札、山札など）を管理します。
    /// </summary>
    public class Player {
        /// <summary>
        /// プレイヤーを一位に識別するためのID。
        /// </summary>
        public string PlayerId { get; } = System.Guid.NewGuid().ToString();

        /// <summary>
        /// プレイヤーの名前。
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// プレイヤーが持つゾーンを、ゾーンの役割を示すインターフェースの型をキーにして管理します。
        /// これにより、GameCoreは具体的なゾーンの実装を知ることなく、抽象的な役割（例：IDeckZone）を通じてゾーンを操作できます。
        /// </summary>
        private readonly Dictionary<Type, object> _zones = new();

        /// <summary>
        /// Playerクラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="name">プレイヤーの名前。</param>
        public Player(string name) { Name = name; }

        /// <summary>
        /// 指定されたインターフェース型に紐づけてゾーンのインスタンスを登録します。
        /// </summary>
        /// <typeparam name="TZoneInterface">登録するゾーンの役割を示すインターフェース型（例: IDeckZone）。</typeparam>
        /// <param name="zoneInstance">登録するゾーンの実際のインスタンス。</param>
        public void RegisterZone<TZoneInterface>(object zoneInstance)
        {
            // 指定されたインターフェースの型をキーとして、ゾーンのインスタンスを辞書に格納する
            _zones[typeof(TZoneInterface)] = zoneInstance;
        }

        /// <summary>
        /// 登録されたゾーンを、役割を示すインターフェース型を使って取得します。
        /// </summary>
        /// <typeparam name="TZoneInterface">取得したいゾーンの役割を示すインターフェース型（例: IDeckZone）。</typeparam>
        /// <returns>見つかったゾーンのインスタンス。見つからない場合はnullを返します。</returns>
        public TZoneInterface GetZone<TZoneInterface>() where TZoneInterface : class
        {
            if (_zones.TryGetValue(typeof(TZoneInterface), out var zone))
            {
                return zone as TZoneInterface;
            }
            // ゾーンが見つからない場合はnullを返す。
            // 設計によっては、必須のゾーンが見つからない場合に例外をスローすることも考えられる。
            return null;
        }
    }
}