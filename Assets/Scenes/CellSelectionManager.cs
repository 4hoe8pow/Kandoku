using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class CellSelectionManager : MonoBehaviour
{
    public static CellSelectionManager Instance { get; private set; }
    public AnswerCell selectedCell;
    public string[,] currentBoard = new string[9, 9];
    public string[,] Solution { get; private set; }
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

        // 選択解除時は全ボタン活性化
        if (selectedCell != null)
            selectedCell.SetSelected(false);

        selectedCell = cell;

        if (selectedCell != null)
        {
            selectedCell.SetSelected(true);
        }
        else
        {
            ResetInputKeyButtonsInteractable();
        }
    }

    public void UpdateInputKeyButtonsInteractable()
    {
        var buttons = GetAllInputKeyButtons();
        foreach (var btn in buttons)
        {
            string symbol = btn.label.text;
            // 盤面上でこのsymbolが9個使われているか
            int count = 0;
            bool isValid = true;
            // 行・列・マスで重複がないかチェック用
            for (int i = 0; i < 9; i++)
            {
                bool rowFound = false, colFound = false;
                for (int j = 0; j < 9; j++)
                {
                    if (currentBoard[i, j] == symbol)
                    {
                        if (rowFound) { isValid = false; break; }
                        rowFound = true;
                        count++;
                    }
                    if (currentBoard[j, i] == symbol)
                    {
                        if (colFound) { isValid = false; break; }
                        colFound = true;
                    }
                }
                if (!isValid) break;
            }
            // 3x3マスで重複がないか
            for (int block = 0; block < 9 && isValid; block++)
            {
                int br = (block / 3) * 3;
                int bc = (block % 3) * 3;
                bool found = false;
                for (int r = br; r < br + 3; r++)
                {
                    for (int c = bc; c < bc + 3; c++)
                    {
                        if (currentBoard[r, c] == symbol)
                        {
                            if (found) { isValid = false; break; }
                            found = true;
                        }
                    }
                    if (!isValid) break;
                }
            }
            // 9個使われていて、かつ重複がなければ非活性
            if (count == 9 && isValid)
                btn.SetInteractable(false);
            else
                btn.SetInteractable(true);
        }
    }

    private void ResetInputKeyButtonsInteractable()
    {
        foreach (var btn in GetAllInputKeyButtons())
            btn.SetInteractable(true);
    }

    private List<InputKeyButton> GetAllInputKeyButtons()
    {
        if (ProblemMapper == null) return new List<InputKeyButton>();
        var keyPanel = ProblemMapper.GetType().GetField("keyPanelParent", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.GetValue(ProblemMapper) as Transform;
        if (keyPanel == null) return new List<InputKeyButton>();
        return keyPanel.GetComponentsInChildren<InputKeyButton>(true).ToList();
    }

    /// <summary>
    /// ゲーム状態をPlayerPrefsにJSONで保存
    /// </summary>
    public void SaveGameStateToPlayerPrefs()
    {
        var state = new GameState
        {
            currentBoard = (string[,])currentBoard.Clone(),
            solution = (string[,])Solution.Clone(),
            hintBoard = (string[,])HintBoard.Clone(),
            isSolved = ProblemMapper != null && ProblemMapper.IsSolved,
            cont = 1 // 固定値
            // difficultyは保存しない
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
        // public int difficulty; // 削除

        public SerializableGameState(GameState state)
        {
            currentBoard = Flatten(state.currentBoard);
            solution = Flatten(state.solution);
            hintBoard = Flatten(state.hintBoard);
            isSolved = state.isSolved;
            cont = state.cont;
            // difficulty = state.difficulty; // 削除
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

    // ゲーム状態保存用データクラス
    [Serializable]
    private class GameState
    {
        public string[,] currentBoard;
        public string[,] solution;
        public string[,] hintBoard;
        public bool isSolved;
        public int cont;
        // public int difficulty; // 削除
    }
}
