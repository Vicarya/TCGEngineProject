using System.Collections.Generic;
using System.IO;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Core
{
    public static class CardLoader
    {
        public static List<WeissCardData> AllCards { get; private set; } = new List<WeissCardData>();

        public static void LoadCards(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Card data file not found at: {filePath}");
                return;
            }

            string json = File.ReadAllText(filePath);
            CardList cardList = JsonUtility.FromJson<CardList>(json);
            AllCards = cardList.cards;

            Debug.Log($"Loaded {AllCards.Count} cards from {filePath}");
        }

        [System.Serializable]
        private class CardList
        {
            public List<WeissCardData> cards;
        }
    }
}
