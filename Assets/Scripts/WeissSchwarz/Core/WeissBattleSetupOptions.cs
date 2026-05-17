using System;
using System.Collections.Generic;
using TCG.Core;

namespace TCG.Weiss
{
    [Serializable]
    public class WeissBattleSetupOptions
    {
        public const string BuiltinAutoSampleDeckId = "builtin:auto-sample";
        public const string DefaultSceneDeckId = "rz_s116_001_050";

        public WeissPlayerSetupOptions Player1 = new WeissPlayerSetupOptions
        {
            Name = "Player1",
            ControlMode = PlayerControlMode.Human,
            DeckId = DefaultSceneDeckId
        };

        public WeissPlayerSetupOptions Player2 = new WeissPlayerSetupOptions
        {
            Name = "Player2",
            ControlMode = PlayerControlMode.Npc,
            DeckId = DefaultSceneDeckId
        };

        public bool ValidateDecks = true;
        public int InitialHandSize = 5;
        public int DeckSize = 50;
        public int MaxCopiesPerCard = 4;

        public static WeissBattleSetupOptions CreateDefault()
        {
            return new WeissBattleSetupOptions();
        }
    }

    [Serializable]
    public class WeissPlayerSetupOptions
    {
        public string Name = "Player";
        public PlayerControlMode ControlMode = PlayerControlMode.Human;
        public string DeckId = WeissBattleSetupOptions.BuiltinAutoSampleDeckId;
    }

    [Serializable]
    public class WeissDeckDefinition
    {
        public string DeckId;
        public string DisplayName;
        public List<WeissDeckEntry> Entries = new List<WeissDeckEntry>();

        public int TotalCards
        {
            get
            {
                int total = 0;
                foreach (var entry in Entries)
                {
                    total += entry.Count;
                }

                return total;
            }
        }
    }

    [Serializable]
    public class WeissDeckEntry
    {
        public string CardCode;
        public int Count;
    }
}
