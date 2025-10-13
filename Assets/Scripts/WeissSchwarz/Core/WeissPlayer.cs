namespace TCG.Weiss
{
    using Core;

    public class WeissPlayer : Player
    {
        public IWeissPlayerController Controller { get; }
        public int HandLimit { get; set; } = 7;

        public WeissPlayer(string name, IWeissPlayerController controller) : base(name)
        {
            Controller = controller;
        }
    }
}