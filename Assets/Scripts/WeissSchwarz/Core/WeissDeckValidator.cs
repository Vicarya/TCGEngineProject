using System.Collections.Generic;
using System.Linq;

namespace TCG.Weiss
{
    public static class WeissDeckValidator
    {
        public static List<string> Validate(WeissDeckDefinition deck, WeissCardDatabase cardDatabase, int expectedDeckSize, int maxCopiesPerCard)
        {
            var errors = new List<string>();
            if (deck == null)
            {
                errors.Add("Deck definition is null.");
                return errors;
            }

            if (deck.Entries == null || deck.Entries.Count == 0)
            {
                errors.Add($"Deck '{deck.DisplayName ?? deck.DeckId}' has no cards.");
                return errors;
            }

            if (deck.TotalCards != expectedDeckSize)
            {
                errors.Add($"Deck '{deck.DisplayName ?? deck.DeckId}' must contain exactly {expectedDeckSize} cards, but has {deck.TotalCards}.");
            }

            foreach (var entry in deck.Entries)
            {
                if (entry == null)
                {
                    errors.Add("Deck contains an empty entry.");
                    continue;
                }

                if (string.IsNullOrWhiteSpace(entry.CardCode))
                {
                    errors.Add("Deck contains an entry without a card code.");
                }

                if (entry.Count <= 0)
                {
                    errors.Add($"Card '{entry.CardCode}' must have a positive count.");
                }

                if (entry.Count > maxCopiesPerCard)
                {
                    errors.Add($"Card '{entry.CardCode}' exceeds the max copy limit of {maxCopiesPerCard}.");
                }
            }

            var duplicates = deck.Entries
                .GroupBy(entry => entry.CardCode)
                .Where(group => group.Sum(entry => entry.Count) > maxCopiesPerCard);

            foreach (var group in duplicates)
            {
                errors.Add($"Card '{group.Key}' appears {group.Sum(entry => entry.Count)} times, exceeding the max copy limit of {maxCopiesPerCard}.");
            }

            var knownCards = new HashSet<string>(cardDatabase.AllCards.Select(card => card.CardCode));
            foreach (var entry in deck.Entries.Where(entry => entry != null))
            {
                if (!string.IsNullOrWhiteSpace(entry.CardCode) && !knownCards.Contains(entry.CardCode))
                {
                    errors.Add($"Card '{entry.CardCode}' does not exist in the card database.");
                }
            }

            return errors;
        }
    }
}
