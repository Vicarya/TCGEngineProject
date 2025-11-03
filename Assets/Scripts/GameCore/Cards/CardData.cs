using UnityEngine;
using System.Collections.Generic;

namespace TCG.Core {
    [System.Serializable]
    public abstract class CardData
    {
        public string CardGuid;
        public string CardCode;
        public string Name;
        public string WorkId; // Renamed from Set
        public string Rarity;
        public string Illustration;
        public string ImagePath;
        public Dictionary<string, object> Metadata = new();
    }
}