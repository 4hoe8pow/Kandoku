using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections;

public class TitleController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button newGameButton;
    [SerializeField] private Button continueButton;

    private IEnumerator WaitForDropdownAndButton()
    {
        while (difficultyDropdown == null || newGameButton == null || continueButton == null)
        {
            yield return null;
        }

        // 一時的にプルダウンを非表示
        difficultyDropdown.gameObject.SetActive(false);

        // プルダウンに難易度列挙を登録
        difficultyDropdown.options.Clear();
        // クリア回数を取得
        string[] clearCountsArr = new string[10];
        int[] clearCounts = new int[10];
        string countsStr = PlayerPrefs.GetString("ClearCounts", null);
        if (string.IsNullOrEmpty(countsStr))
        {
            // 0が10個のものを保存
            string zeroStr = string.Join(",", Enumerable.Repeat("0", 10));
            PlayerPrefs.SetString("ClearCounts", zeroStr);
            PlayerPrefs.Save();
            countsStr = zeroStr;
        }
        if (!string.IsNullOrEmpty(countsStr))
        {
            var arr = countsStr.Split(',');
            for (int i = 0; i < 10 && i < arr.Length; i++)
            {
                int.TryParse(arr[i], out clearCounts[i]);
            }
        }
        // 難易度ごとにラベルを作成
        difficultyDropdown.options.AddRange(
            ((KandokuDifficulty[])System.Enum.GetValues(typeof(KandokuDifficulty)))
            .Select(difficulty =>
            {
                int idx = (int)difficulty - 1;
                // KandokuSymbolのenum名を使う
                string label = System.Enum.GetName(typeof(KandokuSymbol), (KandokuSymbol)(idx + 1));
                int count = (idx >= 0 && idx < clearCounts.Length) ? clearCounts[idx] : 0;
                label += $"({count})";
                return new TMP_Dropdown.OptionData(label);
            })
            .ToList()
        );

        // 保存値を初期表示
        difficultyDropdown.value = (int)GameSettings.Difficulty - 1;
        difficultyDropdown.RefreshShownValue();
        // 続きからボタンの表示制御
        bool hasGameState = PlayerPrefs.HasKey("GameState");
        bool hasDifficulty = PlayerPrefs.HasKey("CurrentDifficulty");
        continueButton.gameObject.SetActive(hasGameState && hasDifficulty);

        // プルダウンを再表示
        difficultyDropdown.gameObject.SetActive(true);

        // ボタン押下イベント
        newGameButton.onClick.AddListener(OnNewGameClicked);
        continueButton.onClick.AddListener(OnContinueClicked);
    }

    private void Start()
    {
        StartCoroutine(WaitForDropdownAndButton());
    }

    private void OnNewGameClicked()
    {
        // GameStateがあれば削除
        if (PlayerPrefs.HasKey("GameState"))
        {
            PlayerPrefs.DeleteKey("GameState");
            PlayerPrefs.Save();
        }
        // 選択された値を保存
        GameSettings.Difficulty = (KandokuDifficulty)difficultyDropdown.value + 1;
        // 難易度だけPlayerPrefsに保存
        PlayerPrefs.SetInt("CurrentDifficulty", (int)GameSettings.Difficulty);
        PlayerPrefs.Save();
        // 0.5秒待ってからシーン遷移
        StartCoroutine(WaitAndLoadScene());
    }

    private void OnContinueClicked()
    {
        // 選択された値を保存
        GameSettings.Difficulty = (KandokuDifficulty)difficultyDropdown.value + 1;
        // 0.5秒待ってからシーン遷移
        StartCoroutine(WaitAndLoadScene());
    }

    private IEnumerator WaitAndLoadScene()
    {
        yield return new WaitForSeconds(0.5f);
        SceneManager.LoadScene("InGame");
    }

    [System.Serializable]
    private class MinimalGameState
    {
        public string[] currentBoard = new string[81];
        public string[] solution = new string[81];
        public string[] hintBoard = new string[81];
        public bool isSolved = false;
        public int cont = 0;
        public int difficulty;
        public MinimalGameState(int diff) { difficulty = diff; }
    }
}
