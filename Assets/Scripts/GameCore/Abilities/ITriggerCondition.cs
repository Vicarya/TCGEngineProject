namespace TCG.Core
{
    // 発動条件
    public interface ITriggerCondition
    {
        bool IsSatisfied(GameEvent e, GameState state, Card source);
    }
}
