using System.Collections.Generic;
using UnityEngine;

namespace GameCore.Database
{
    /// <summary>
    /// デッキ情報を表すクラス。
    /// </summary>
    [System.Serializable]
    public class Deck
    {
        public string DeckName;
        public List<string> CardIds = new List<string>();
    }

    /// <summary>
    /// デッキやお気に入りを管理する汎用クラス。
    /// PlayerPrefsやファイル保存などで永続化することを想定しています。
    /// </summary>
    public class DeckManager
    {
        private const string DecksKey = "UserDecks";
        private const string FavoritesKey = "UserFavorites";

        public void SaveDecks(List<Deck> decks)
        {
            string json = JsonUtility.ToJson(new Serialization<Deck>(decks));
            PlayerPrefs.SetString(DecksKey, json);
            PlayerPrefs.Save();
        }

        public List<Deck> LoadDecks()
        {
            if (PlayerPrefs.HasKey(DecksKey))
            {
                string json = PlayerPrefs.GetString(DecksKey);
                return JsonUtility.FromJson<Serialization<Deck>>(json).ToList();
            }
            return new List<Deck>();
        }

        // お気に入り機能も同様に実装可能
        // public void SaveFavorites(List<string> cardIds) { ... }
        // public List<string> LoadFavorites() { ... }
    }

    /// <summary>
    /// JsonUtilityでListをシリアライズするためのヘルパークラス
    /// </summary>
    [System.Serializable]
    internal class Serialization<T>
    {
        public List<T> Target;
        public List<T> ToList() => Target;
        public Serialization(List<T> target) => Target = target;
    }
}