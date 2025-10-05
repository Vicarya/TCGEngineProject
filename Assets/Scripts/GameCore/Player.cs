using System;
using System.Collections.Generic;

namespace TCG.Core
{
    public class Player {
        public string PlayerId { get; } = System.Guid.NewGuid().ToString();
        public string Name { get; set; }

        // 型をキーにしてゾーンを管理する新しい仕組み
        private readonly Dictionary<Type, object> _zones = new();

        public Player(string name) { Name = name; }

        /// <summary>
        /// ゾーンをインターフェース型に紐づけて登録します
        /// </summary>
        public void RegisterZone<TZoneInterface>(object zoneInstance)
        {
            _zones[typeof(TZoneInterface)] = zoneInstance;
        }

        /// <summary>
        /// 登録されたゾーンをインターフェース型で取得します
        /// </summary>
        public TZoneInterface GetZone<TZoneInterface>() where TZoneInterface : class
        {
            if (_zones.TryGetValue(typeof(TZoneInterface), out var zone))
            {
                return zone as TZoneInterface;
            }
            return null; // or throw an exception if the zone is mandatory
        }
    }
}