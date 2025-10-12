namespace TCG.Core
{
    public abstract class CardBase<TData> : Card where TData : CardData
    {
        public TData Data { get; private set; }

        protected CardBase(TData data, Player owner)
        {
            Data = data;
            Owner = owner;
            IsTapped = false;
            IsFaceUp = true;
        }
    }
}
