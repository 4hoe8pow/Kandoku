using UnityEngine;

public class CellSelectionManager : MonoBehaviour
{
    public static CellSelectionManager Instance { get; private set; }

    public AnswerCell selectedCell;

    // 盤面状態を管理
    public string[,] currentBoard = new string[9, 9];

    // 正解盤面も公開
    public string[,] Solution { get; private set; }

    private void Awake()
    {
        if (Instance != null) Destroy(gameObject);
        else Instance = this;
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
}
