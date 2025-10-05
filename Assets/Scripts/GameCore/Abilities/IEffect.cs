namespace TCG.Core
{
    // 効果
    public interface IEffect
    {
        void Resolve(GameEvent e, GameState state, Card source);
    }
}