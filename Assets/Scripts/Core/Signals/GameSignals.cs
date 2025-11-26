namespace MiniIT.ARKANOID
{
    public class GameOverSignal
    {
    }

    public class LevelCompletedSignal
    {
    }

    public class LevelResetSignal
    {
    }

    public class ScoreChangedSignal
    {
        public int Score { get; private set; }

        public ScoreChangedSignal(int score)
        {
            Score = score;
        }
    }

    public class LivesChangedSignal
    {
        public int Lives { get; private set; }

        public LivesChangedSignal(int lives)
        {
            Lives = lives;
        }
    }

    public struct BrickDestroyedSignal
    {
        public BrickBase Brick;
        public int Reward;

        public BrickDestroyedSignal(BrickBase brick, int reward)
        {
            Brick = brick;
            Reward = reward;
        }
    }

    public class BallLostSignal
    {
    }
}
