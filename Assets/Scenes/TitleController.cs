using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using System.Linq;
using System.Collections;

public class TitleController : MonoBehaviour
{
    [SerializeField] private Dropdown difficultyDropdown;
    [SerializeField] private Button startButton;

    private IEnumerator WaitForDropdownAndButton()
    {
        while (difficultyDropdown == null || startButton == null)
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
            .Select(difficulty => new Dropdown.OptionData(
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
        startButton.onClick.AddListener(OnStartClicked);
    }

    private void Start()
    {
        StartCoroutine(WaitForDropdownAndButton());
    }

    private void OnStartClicked()
    {
        // 選択された値を保存
        GameSettings.Difficulty = (KandokuDifficulty)difficultyDropdown.value + 1;

        // InGame シーンへ遷移
        SceneManager.LoadScene("InGame");
    }
}
