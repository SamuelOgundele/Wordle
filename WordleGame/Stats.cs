namespace WordleGame
{
    // Game Stats for StatsPage output
    public class Stats
    {
        public DateTime Timestamp
        {
            get;
            set;
        }
        public string CorrectWord
        {
            get;
            set;
        }
        public int NumberOfTries
        {
            get;
            set;
        }
        public string GameResult
        {
            get;
            set;
        }
        public List<string> Colors
        {
            get;
            set;
        }
    }
}
