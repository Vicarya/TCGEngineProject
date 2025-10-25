
using System.Collections.Generic;
using System.Linq;

namespace TCG.Weiss
{
    /// <summary>
    /// Manages a queue of pending abilities waiting for resolution.
    /// </summary>
    public class AbilityQueue
    {
        private readonly List<PendingAbility> _queue = new List<PendingAbility>();

        /// <summary>
        /// Adds a new pending ability to the queue.
        /// </summary>
        public void Add(PendingAbility ability)
        {
            _queue.Add(ability);
        }

        /// <summary>
        /// Gets all pending abilities for a specific player.
        /// </summary>
        public List<PendingAbility> GetPendingAbilitiesForPlayer(WeissPlayer player)
        {
            return _queue.Where(pa => pa.ResolvingPlayer == player).ToList();
        }

        /// <summary>
        /// Removes a resolved ability from the queue.
        /// </summary>
        public void Remove(PendingAbility ability)
        {
            _queue.Remove(ability);
        }

        /// <summary>
        /// Checks if there are any abilities pending in the queue.
        /// </summary>
        public bool HasPendingAbilities()
        {
            return _queue.Any();
        }
    }
}
