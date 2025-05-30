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

    [Header("Difficulty Display")]
    [SerializeField] private TMP_Text difficultyText; // 難易度表示用TMP_Text

    public KandokuDifficulty difficulty = KandokuDifficulty.VeryEasy;

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
        // AdMobインターサスティシャル広告の処理はGoogleAdMobSupplierに一任

        AdjustGridPanelSize();

        bool useSaved = false;
        string json = PlayerPrefs.GetString("GameState", null);
        SerializableGameState loaded = null;
        if (!string.IsNullOrEmpty(json))
        {
            loaded = JsonUtility.FromJson<SerializableGameState>(json);
            if (loaded != null && loaded.cont != 0)
            {
                useSaved = true;
            }
        }

        // 難易度はPlayerPrefsから取得
        if (PlayerPrefs.HasKey("CurrentDifficulty"))
        {
            difficulty = (KandokuDifficulty)PlayerPrefs.GetInt("CurrentDifficulty");
            Debug.Log($"Loaded difficulty from PlayerPrefs: {difficulty}");
        }
        else
        {
            difficulty = GameSettings.Difficulty;
            Debug.Log($"Loaded difficulty from GameSettings: {difficulty}");
        }
        SetDifficulty(difficulty);

        if (!useSaved)
        {
            // セーブデータなし or continue==0 の場合
            PlayerPrefs.DeleteKey("GameState");
            solution = KandokuGenerator.GenerateKandoku();

            problem = KandokuGenerator.MaskKandokuUniqueParallel(solution, difficulty);

            CellSelectionManager.Instance.SetInitialBoard(problem);
            CellSelectionManager.Instance.SetSolution(solution);
            CellSelectionManager.Instance.SetHintBoard(problem);

            BuildUIGrid();
        }
        else
        {
            // セーブデータあり
            Debug.Log($"Loaded JSON: {json}");
            solution = Unflatten(loaded.solution, 9, 9);
            Debug.Log("Unflattened solution:");
            for (int r = 0; r < 9; r++)
            {
                string row = "";
                for (int c = 0; c < 9; c++)
                    row += solution[r, c] + " ";
                Debug.Log($"solution[{r}]: {row}");
            }

            problem = Unflatten(loaded.hintBoard, 9, 9);
            for (int r = 0; r < 9; r++)
            {
                string row = "";
                for (int c = 0; c < 9; c++)
                    row += problem[r, c] + " ";
            }

            CellSelectionManager.Instance.SetInitialBoard(problem);
            CellSelectionManager.Instance.SetSolution(solution);
            CellSelectionManager.Instance.SetHintBoard(problem);

            BuildUIGrid();

            // currentBoardリストア
            Debug.Log("Restoring currentBoard...");
            RestoreCurrentBoard(loaded.currentBoard);
            Debug.Log("currentBoard restored.");
        }

        BuildKeyPanel();

        // RainbowWave参照取得
        rainbowWave = hintParent.GetComponentInParent<RainbowWave>();

        // 初期状態で正解判定
        CheckSolved();
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
            OnGameCleared();
        }

        prevSolved = IsSolved;
    }

    // ゲームクリア時の処理をまとめた関数
    private void OnGameCleared()
    {
        if (solveEffect != null)
            solveEffect.Play();

        if (rainbowWave != null)
            StartCoroutine(RainbowWaveAnimationCoroutine());

        if (solvedButton != null)
            solvedButton.SetActive(true); // ボタンをアクティブにする

        // --- クリア回数管理 ---
        // 難易度は1〜10（KandokuDifficultyのenum値）
        int diffIndex = (int)difficulty - 1; // 0〜9
        string key = "ClearCounts";
        string countsStr = PlayerPrefs.GetString(key, null);
        int[] counts;
        var arr = countsStr.Split(',');
        counts = new int[10];
        for (int i = 0; i < 10 && i < arr.Length; i++)
        {
            int.TryParse(arr[i], out counts[i]);
        }

        // インクリメント
        if (diffIndex >= 0 && diffIndex < 10)
            counts[diffIndex]++;
        // 保存
        string newStr = string.Join(",", counts);
        PlayerPrefs.SetString(key, newStr);
        PlayerPrefs.Save();

        // ClearCountsの保存に成功したらGameStateを削除
        if (PlayerPrefs.HasKey("GameState"))
        {
            PlayerPrefs.DeleteKey("GameState");
            PlayerPrefs.Save();
        }
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

                // --- ここから枠線制御 ---
                if (go.TryGetComponent<CellBorder>(out var border))
                {
                    bool thickTop = r % 3 == 0;
                    bool thickLeft = c % 3 == 0;
                    bool thickBottom = r == size - 1;
                    bool thickRight = c == size - 1;
                    border.SetBorder(thickTop, thickBottom, thickLeft, thickRight);
                }
                // --- ここまで枠線制御 ---
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

    // --- currentBoardリストア関数 ---
    private void RestoreCurrentBoard(string[] flatBoard)
    {
        if (flatBoard == null)
        {
            Debug.Log("RestoreCurrentBoard: flatBoard is null");
            return;
        }
        if (flatBoard.Length != 81)
        {
            Debug.Log($"RestoreCurrentBoard: flatBoard length is {flatBoard.Length}, expected 81");
            return;
        }
        var board = new string[9, 9];
        for (int r = 0; r < 9; r++)
            for (int c = 0; c < 9; c++)
                board[r, c] = flatBoard[r * 9 + c];

        CellSelectionManager.Instance.currentBoard = board;

        // UI上の盤面も更新
        var answerCells = hintParent.GetComponentsInChildren<AnswerCell>(true);
        foreach (var cell in answerCells)
        {
            int row = cell.row;
            int col = cell.col;
            string newValue = board[row, col];
            string currentValue = cell.label != null ? cell.label.text : string.Empty;
            if (currentValue != newValue && newValue != "？")
            {
                cell.SetSymbol(newValue);
            }
        }
    }

    // --- Unflatten関数 ---
    private string[,] Unflatten(string[] flat, int rows, int cols)
    {
        var arr = new string[rows, cols];
        if (flat == null || flat.Length != rows * cols) return arr;
        for (int r = 0; r < rows; r++)
            for (int c = 0; c < cols; c++)
                arr[r, c] = flat[r * cols + c];
        return arr;
    }

    // --- SerializableGameStateクラス（CellSelectionManagerと同じもの） ---
    [System.Serializable]
    private class SerializableGameState
    {
        public string[] currentBoard;
        public string[] solution;
        public string[] hintBoard;
        public bool isSolved;
        public int cont;
        public int difficulty; // 追加: 難易度
    }

    // 難易度と表示を同時に更新するメソッド
    private void SetDifficulty(KandokuDifficulty diff)
    {
        difficulty = diff;
        // TMP_Textに漢字を表示
        if (difficultyText != null)
        {
            int idx = (int)diff - 1;
            if (idx >= 0 && idx < KandokuSymbols.Length)
                difficultyText.text = KandokuSymbols[idx];
            else
                difficultyText.text = "?";
        }
    }
}
