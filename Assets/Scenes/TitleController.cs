using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections;

public class TitleController : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown difficultyDropdown;
    [SerializeField] private Button newGameButton; // startButton→newGameButton
    [SerializeField] private Button continueButton; // 追加

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
        var difficultyNames = new[] { "臨", "兵", "闘", "者", "皆", "陣", "烈", "在", "前" };
        difficultyDropdown.options.AddRange(
            ((KandokuDifficulty[])System.Enum.GetValues(typeof(KandokuDifficulty)))
            .Select(difficulty => new TMP_Dropdown.OptionData(
                difficulty == KandokuDifficulty.Unknown
                ? string.Join("", difficultyNames)
                : difficultyNames[(int)difficulty - 1]))
            .ToList()
        );

        // 保存値を初期表示
        difficultyDropdown.value = (int)GameSettings.Difficulty;

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
}
