using System.Collections.Generic;
using System.Linq;
using TCG.Weiss;
using UnityEngine;

namespace TCG.Core
{
    /// <summary>
    /// Loads all card data from ScriptableObject assets in the Resources folder.
    /// </summary>
    public static class CardLoader
    {
        public static List<WeissCardData> AllCards { get; private set; } = new List<WeissCardData>();

        private const string CardDataPath = "CardData";

        /// <summary>
        /// Loads all WeissCardDataAsset files from the Resources/CardData folder.
        /// </summary>
        public static void LoadAllCardAssets()
        {
            var cardAssets = Resources.LoadAll<WeissCardDataAsset>(CardDataPath);
            
            AllCards = cardAssets.Select(asset => asset.Data).ToList();

            Debug.Log($"Loaded {AllCards.Count} cards from {CardDataPath} folder.");
        }
    }
}