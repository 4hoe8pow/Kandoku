using UnityEngine;
using System; // for Serializable

public class CellSelectionManager : MonoBehaviour
{
    public static CellSelectionManager Instance { get; private set; }

    public AnswerCell selectedCell;

    public string[,] currentBoard = new string[9, 9];
    public string[,] Solution { get; private set; }

    // 追加: hintBoard
    public string[,] HintBoard { get; private set; } = new string[9, 9];

    public UIProblemMapper ProblemMapper { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // UIProblemMapperをキャッシュ
        ProblemMapper = FindFirstObjectByType<UIProblemMapper>();
    }

    /// <summary>
    /// 盤面初期化用
    /// </summary>
    public void SetInitialBoard(string[,] board)
    {
        currentBoard = (string[,])board.Clone();
    }

    /// <summary>
    /// 正解盤面セット用
    /// </summary>
    public void SetSolution(string[,] solutionBoard)
    {
        Solution = (string[,])solutionBoard.Clone();

        // 正解盤面を表形式でDebug.Logに出力
        string boardStr = "";
        for (int i = 0; i < Solution.GetLength(0); i++)
        {
            for (int j = 0; j < Solution.GetLength(1); j++)
            {
                boardStr += Solution[i, j] + (j < Solution.GetLength(1) - 1 ? " " : "");
            }
            boardStr += "\n";
        }
        Debug.Log("Solution Board:\n" + boardStr);
    }

    /// <summary>
    /// ヒント盤面セット用
    /// </summary>
    public void SetHintBoard(string[,] hint)
    {
        HintBoard = (string[,])hint.Clone();
    }

    /// <summary>
    /// セルを選択状態にする。前のセルはハイライト解除。
    /// </summary>
    public void SelectCell(AnswerCell cell)
    {
        if (selectedCell == cell) return;

        if (selectedCell != null)
            selectedCell.SetSelected(false);

        selectedCell = cell;

        if (selectedCell != null)
            selectedCell.SetSelected(true);
    }

    // ゲーム状態保存用データクラス
    [Serializable]
    private class GameState
    {
        public string[,] currentBoard;
        public string[,] solution;
        public string[,] hintBoard;
        public bool isSolved;
        public int cont;
    }

    /// <summary>
    /// ゲーム状態をPlayerPrefsにJSONで保存
    /// </summary>
    private void SaveGameStateToPlayerPrefs()
    {
        var state = new GameState
        {
            currentBoard = (string[,])currentBoard.Clone(),
            solution = (string[,])Solution.Clone(),
            hintBoard = (string[,])HintBoard.Clone(),
            isSolved = ProblemMapper != null ? ProblemMapper.IsSolved : false,
            cont = 1 // 固定値
        };
        string json = JsonUtility.ToJson(new SerializableGameState(state));
        PlayerPrefs.SetString("GameState", json);
        PlayerPrefs.Save();
    }

    // JsonUtilityは多次元配列を直接シリアライズできないためラッパー
    [Serializable]
    private class SerializableGameState
    {
        public string[] currentBoard;
        public string[] solution;
        public string[] hintBoard;
        public bool isSolved;
        public int cont;

        public SerializableGameState(GameState state)
        {
            currentBoard = Flatten(state.currentBoard);
            solution = Flatten(state.solution);
            hintBoard = Flatten(state.hintBoard);
            isSolved = state.isSolved;
            cont = state.cont;
        }

        private string[] Flatten(string[,] arr)
        {
            int rows = arr.GetLength(0);
            int cols = arr.GetLength(1);
            string[] flat = new string[rows * cols];
            for (int r = 0; r < rows; r++)
                for (int c = 0; c < cols; c++)
                    flat[r * cols + c] = arr[r, c];
            return flat;
        }
    }
}
