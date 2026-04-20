namespace MiniIT.ARKANOID
{
    public class GameOverSignal
    {
        public int Score { get; private set; }

        public GameOverSignal(int score)
        {
            Score = score;
        }
    }

    public class LevelCompletedSignal
    {
    }

    public class LevelResetSignal
    {
    }

    public class MazeStartedSignal
    {
    }

    public class MazeCompletedSignal
    {
    }

    public class MazeFailedSignal
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

    public class BallResetSignal
    {
    }

    public class BallLostSignal
    {
    }

    public class BallMergedSignal
    {
        public int Tier { get; private set; }

        public BallMergedSignal(int tier)
        {
            Tier = tier;
        }
    }

    public class BallReachedTopSignal
    {
    }

    public struct BallContactSignal
    {
        public Ball Source;
        public Ball Target;

        public BallContactSignal(Ball source, Ball target)
        {
            Source = source;
            Target = target;
        }
    }

    public class BrickFieldReachedBottomSignal
    {
    }

    public class PlayableAdTimeoutSignal
    {
    }
}
