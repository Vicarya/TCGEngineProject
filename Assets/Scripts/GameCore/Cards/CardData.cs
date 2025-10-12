using UnityEngine;
using System.Collections.Generic;

namespace TCG.Core {
    public abstract class CardData
    {
        public string CardGuid;
        public string CardCode;
        public string Name;
        public string Set;
        public string Rarity;
        public string Atrribute;
        public string[] Description;
        public string Illustration;
        public string ImagePath;
        public Dictionary<string, object> Metadata = new();
    }
}