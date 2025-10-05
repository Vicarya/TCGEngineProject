namespace TCG.Core {
    public class GameEventType
    {
        private static int nextIndex = 0;
        public int Index { get; }
        public string Name { get; }

        public GameEventType(string name)
        {
            Index = nextIndex++;
            Name = name;
        }
    }
}