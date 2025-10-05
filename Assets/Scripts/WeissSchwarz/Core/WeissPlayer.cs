namespace TCG.Weiss
{
    using Core;

    public class WeissPlayer : Player
    {
        public IPlayerController Controller { get; }
        public int HandLimit { get; set; } = 7;

        public WeissPlayer(string name, IPlayerController controller) : base(name)
        {
            Controller = controller;
        }
    }
}