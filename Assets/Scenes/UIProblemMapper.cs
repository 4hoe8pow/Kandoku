using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIProblemMapper : MonoBehaviour
{
    [Header("Problem Panel")]
    [SerializeField] private Transform hintParent; // 問題用親パネル
    [SerializeField] private GameObject hintPrefab; // 出題用ヒント

    [Header("Answer Keys")]
    [SerializeField] private GameObject answerPrefab; // ユーザ解答用

    [Header("Key Panel")]
    [SerializeField] private GameObject keyPrefab;       // 入力キー用のボタンPrefab
    [SerializeField] private Transform keyPanelParent;

    private KandokuDifficulty difficulty = KandokuDifficulty.Normal;

    private string[,] solution;
    private string[,] problem;

    private static readonly string[] KandokuSymbols = new string[]{
        "臨","兵","闘","者","皆","陣","烈","在","前"
    };
    void Start()
    {
        try
        {
            AdjustGridPanelSize();
            solution = KandokuGenerator.GenerateKandoku();
            difficulty = GameSettings.Difficulty;
            problem = KandokuGenerator.MaskKandoku(solution, difficulty);

            BuildUIGrid();
            BuildKeyPanel();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Kandoku生成エラー: {ex.Message}");
        }
    }

    private void BuildUIGrid()
    {
        const int size = 9;

        // 既存セルの破棄
        foreach (Transform child in hintParent)
            Destroy(child.gameObject);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                string symbol = problem[r, c];

                // 「?」ならキー用Prefab、そうでなければセル用Prefab を生成
                GameObject go = Instantiate(
                    symbol == "？" ? answerPrefab : hintPrefab,
                    hintParent
                );

                var text = go.GetComponentInChildren<TMP_Text>();

                // 「?」でない場合のみ中のTMP_Text にシンボルをセット
                if (symbol != "？")
                {
                    if (text != null)
                        text.text = symbol;
                }
                else
                {
                    // 「?」の場合はテキストをクリア
                    if (text != null)
                        text.text = string.Empty;
                }
            }
        }

        AdjustGridCellSize();
    }

    private void BuildKeyPanel()
    {
        // 既存のキーをクリア
        foreach (Transform child in keyPanelParent)
            Destroy(child.gameObject);

        // 九字を並べる
        foreach (var symbol in KandokuSymbols)
        {
            // Instantiate して親パネルにぶら下げ
            var go = Instantiate(keyPrefab, keyPanelParent);

            // 中の TMP_Text に文字セット
            var text = go.GetComponentInChildren<TMPro.TMP_Text>();
            if (text != null)
                text.text = symbol;
        }
    }

    private void AdjustGridCellSize()
    {
        GridLayoutGroup grid = hintParent.GetComponent<GridLayoutGroup>();
        RectTransform rt = hintParent.GetComponent<RectTransform>();

        float width = rt.rect.width;
        float height = rt.rect.height;

        float cellSize = Mathf.Min(width / 9f, height / 9f);
        grid.cellSize = new Vector2(cellSize, cellSize);
    }

    private void AdjustGridPanelSize()
    {
        RectTransform panelRT = hintParent.GetComponent<RectTransform>();
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;

        float size = Mathf.Min(screenWidth, screenHeight);
        panelRT.sizeDelta = new Vector2(size, size);
    }

}
