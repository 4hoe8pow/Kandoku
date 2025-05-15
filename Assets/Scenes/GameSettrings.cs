using UnityEngine;

public static class GameSettings
{
    private const string KeyDifficulty = "KandokuDifficulty";

    public static KandokuDifficulty Difficulty
    {
        get
        {
            int stored = PlayerPrefs.GetInt(KeyDifficulty, (int)KandokuDifficulty.VeryEasy);
            return (KandokuDifficulty)stored;
        }
        set
        {
            PlayerPrefs.SetInt(KeyDifficulty, (int)value);
            PlayerPrefs.Save();
        }
    }
}
