using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

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

    [Header("Solve Effect")]
    [SerializeField] private ParticleSystem solveEffect;

    [Header("Solved Button")]
    [SerializeField] private GameObject solvedButton; // 正解時に表示するボタン

    private KandokuDifficulty difficulty = KandokuDifficulty.Normal;

    private string[,] solution;
    private string[,] problem;

    /// <summary>
    /// 最新の solved 状態を保持（前回チェック分）
    /// </summary>
    private bool prevSolved = false;

    public bool IsSolved { get; private set; } = false;

    private static readonly string[] KandokuSymbols = new string[]{
        "臨","兵","闘","者","皆","陣","烈","在","前"
    };

    private RainbowWave rainbowWave; // 参照保持用

    void Start()
    {
        // AdMobバナー広告をロード
        var adSupplier = FindFirstObjectByType<GoogleAdMobSupplier>();
        if (adSupplier != null)
        {
            adSupplier.ShowInterstitialAd();
        }
        try
        {
            AdjustGridPanelSize();
            solution = KandokuGenerator.GenerateKandoku();
            difficulty = GameSettings.Difficulty;
            problem = KandokuGenerator.MaskKandokuUniqueParallel(solution, difficulty);

            // ここで初期盤面と正解盤面をCellSelectionManagerに共有
            CellSelectionManager.Instance.SetInitialBoard(problem);
            CellSelectionManager.Instance.SetSolution(solution);

            BuildUIGrid();
            BuildKeyPanel();

            // RainbowWave参照取得
            rainbowWave = hintParent.GetComponentInParent<RainbowWave>();

            // 初期状態で正解判定
            CheckSolved();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Kandoku生成エラー: {ex.Message}");
        }
    }

    // 正解判定時にアニメーションを呼ぶ
    public void CheckSolved()
    {
        var board = CellSelectionManager.Instance.currentBoard;
        if (board == null || solution == null)
        {
            IsSolved = false;
            return;
        }

        // 正解判定
        int size = solution.GetLength(0);
        bool correct = true;
        for (int r = 0; r < size && correct; r++)
        {
            for (int c = 0; c < size && correct; c++)
            {
                var cur = board[r, c];
                var sol = solution[r, c];
                if (string.IsNullOrEmpty(cur) || cur == "？" || cur != sol)
                    correct = false;
            }
        }
        IsSolved = correct;
        Debug.Log(IsSolved ? "正解！" : "未完成または不正解");

        // 正解時のエフェクト
        if (!prevSolved && IsSolved)
        {
            if (solveEffect != null)
                solveEffect.Play();

            if (rainbowWave != null)
                StartCoroutine(RainbowWaveAnimationCoroutine());

            if (solvedButton != null)
                solvedButton.SetActive(true); // ボタンをアクティブにする
        }

        prevSolved = IsSolved;
    }

    // アニメーション用コルーチン
    private IEnumerator RainbowWaveAnimationCoroutine()
    {
        float duration = 3.0f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rainbowWave != null)
                rainbowWave.AnimateRainbow(Time.time);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    private void BuildUIGrid()
    {
        int size = problem.GetLength(0);

        // 既存セルの破棄
        foreach (Transform child in hintParent)
            Destroy(child.gameObject);

        for (int r = 0; r < size; r++)
        {
            for (int c = 0; c < size; c++)
            {
                string symbol = problem[r, c];

                GameObject go = Instantiate(
                    symbol == "？" ? answerPrefab : hintPrefab,
                    hintParent
                );

                var text = go.GetComponentInChildren<TMP_Text>();

                if (symbol != "？")
                {
                    if (text != null)
                        text.text = symbol;
                }
                else
                {
                    if (text != null)
                        text.text = string.Empty;

                    if (go.TryGetComponent<AnswerCell>(out var answerCell))
                    {
                        answerCell.row = r;
                        answerCell.col = c;
                    }
                }
            }
        }

        AdjustGridCellSize();

        // RainbowWaveを探してRefreshImagesを呼ぶ
        var rainbow = hintParent.GetComponentInParent<RainbowWave>();
        if (rainbow != null)
        {
            rainbow.RefreshImages();
        }
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
