using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TCG.Weiss
{
    public static class WeissDeckLoader
    {
        public static WeissDeckDefinition LoadDeck(string deckId, WeissCardDatabase cardDatabase, int targetDeckSize, int maxCopiesPerCard)
        {
            if (string.IsNullOrWhiteSpace(deckId) || deckId == WeissBattleSetupOptions.BuiltinAutoSampleDeckId)
            {
                return BuildAutoSampleDeck(cardDatabase, targetDeckSize, maxCopiesPerCard);
            }

            return LoadDeckFromTextFile(deckId);
        }

        public static WeissDeckDefinition LoadDeckFromTextFile(string deckIdOrFileName)
        {
            string fileName = deckIdOrFileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)
                ? deckIdOrFileName
                : $"{deckIdOrFileName}.txt";

            string deckFilePath = Path.Combine(Application.streamingAssetsPath, "WeissSchwarz", "Decks", fileName);
            if (!File.Exists(deckFilePath))
            {
                throw new FileNotFoundException($"Deck file not found: {deckFilePath}", deckFilePath);
            }

            var deck = new WeissDeckDefinition
            {
                DeckId = Path.GetFileNameWithoutExtension(fileName),
                DisplayName = Path.GetFileNameWithoutExtension(fileName)
            };

            foreach (string rawLine in File.ReadAllLines(deckFilePath))
            {
                string line = rawLine.Trim();
                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#", StringComparison.Ordinal))
                {
                    continue;
                }

                string[] parts = line.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length != 2 || !int.TryParse(parts[1], out int count))
                {
                    throw new InvalidDataException($"Invalid deck line: '{rawLine}'");
                }

                deck.Entries.Add(new WeissDeckEntry
                {
                    CardCode = parts[0],
                    Count = count
                });
            }

            return deck;
        }

        public static WeissDeckDefinition BuildAutoSampleDeck(WeissCardDatabase cardDatabase, int targetDeckSize, int maxCopiesPerCard)
        {
            if (cardDatabase?.AllCards == null || cardDatabase.AllCards.Count == 0)
            {
                throw new InvalidOperationException("Card database is empty. Cannot build sample deck.");
            }

            var uniqueCards = cardDatabase.AllCards
                .Where(card => card != null && !string.IsNullOrWhiteSpace(card.CardCode))
                .GroupBy(card => card.CardCode)
                .Select(group => group.First())
                .Take(Mathf.CeilToInt(targetDeckSize / (float)maxCopiesPerCard))
                .ToList();

            if (uniqueCards.Count == 0)
            {
                throw new InvalidOperationException("No cards were available to build the sample deck.");
            }

            var deck = new WeissDeckDefinition
            {
                DeckId = WeissBattleSetupOptions.BuiltinAutoSampleDeckId,
                DisplayName = "Auto Sample 50"
            };

            int remaining = targetDeckSize;
            int index = 0;
            while (remaining > 0)
            {
                var card = uniqueCards[index % uniqueCards.Count];
                int count = Mathf.Min(maxCopiesPerCard, remaining);
                deck.Entries.Add(new WeissDeckEntry
                {
                    CardCode = card.CardCode,
                    Count = count
                });

                remaining -= count;
                index++;
            }

            return deck;
        }
    }
}
