public enum GameMode { LocalMulti, SoloVsBot }
public enum BotDifficulty { Easy, Medium, Hard }

public static class GameSession
{
    public static GameMode mode = GameMode.LocalMulti;
    public static BotDifficulty difficulty = BotDifficulty.Medium;
}
