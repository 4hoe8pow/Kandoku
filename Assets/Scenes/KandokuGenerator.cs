using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

// Kandoku生成・検証・マスク処理
public static class KandokuGenerator
{
    public static string[,] GenerateKandoku()
    {
        var matrix = new DLXMatrix();
        var solver = new DLXSolver(matrix.Header);
        var solution = new List<DLXNode>();
        if (!solver.Search(solution))
            throw new Exception("生成失敗");
        return solution.Aggregate(new string[9, 9], (result, node) =>
        {
            int id = node.RowID;
            int r = id / 81;
            int c = id / 9 % 9;
            int n = (id % 9) + 1;
            result[r, c] = ((KandokuSymbol)n).ToString();
            return result;
        });
    }

    public static bool IsValidBoard(string[,] board)
    {
        for (int i = 0; i < 9; i++)
        {
            var rowSet = new HashSet<string>();
            var colSet = new HashSet<string>();
            var blockSet = new HashSet<string>();
            for (int j = 0; j < 9; j++)
            {
                var rowVal = board[i, j];
                if (rowVal != null && rowVal != "?" && rowVal != "？" && !rowSet.Add(rowVal))
                    return false;
                var colVal = board[j, i];
                if (colVal != null && colVal != "?" && colVal != "？" && !colSet.Add(colVal))
                    return false;
                int br = i / 3 * 3 + (j / 3);
                int bc = i % 3 * 3 + (j % 3);
                var blockVal = board[br, bc];
                if (blockVal != null && blockVal != "?" && blockVal != "？" && !blockSet.Add(blockVal))
                    return false;
            }
        }
        return true;
    }

    private static int GetMaskCount(KandokuDifficulty difficulty) => difficulty switch
    {
        KandokuDifficulty.VeryEasy => 38,
        KandokuDifficulty.Easy => 41,
        KandokuDifficulty.Normal => 43,
        KandokuDifficulty.Hard => 45,
        KandokuDifficulty.VeryHard => 48,
        KandokuDifficulty.Extreme => 50,
        KandokuDifficulty.Spicy => 53,
        KandokuDifficulty.Insane => 57,
        KandokuDifficulty.Nightmare => 60,
        KandokuDifficulty.Meteo => 64,
        _ => throw new ArgumentOutOfRangeException(nameof(difficulty))
    };

    public static string[,] MaskKandokuUniqueParallel(string[,] board, KandokuDifficulty difficulty)
    {
        int targetHints = 81 - GetMaskCount(difficulty);
        var masked = (string[,])board.Clone();
        var cells = Enumerable.Range(0, 81)
                               .OrderBy(_ => Guid.NewGuid())
                               .ToList();

        int currentHints = 81;
        var options = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount };

        while (currentHints > targetHints)
        {
            int removePos = -1;
            var found = new ManualResetEventSlim(false);

            Parallel.ForEach(cells, options, (pos, state) =>
            {
                if (found.IsSet) { state.Stop(); return; }

                int r = pos / 9, c = pos % 9;
                if (masked[r, c] == "？") return;

                var localBoard = (string[,])masked.Clone();
                localBoard[r, c] = "？";

                if (HasUniqueSolution(localBoard))
                {
                    Interlocked.Exchange(ref removePos, pos);
                    found.Set();
                    state.Stop();
                }
            });

            if (removePos < 0) break;

            masked[removePos / 9, removePos % 9] = "？";
            cells.Remove(removePos);
            currentHints--;
        }

        return masked;
    }

    /// <summary>
    /// 部分盤面 (maskedBoard) のヒントを DLX のノードリストとして返します。
    /// </summary>
    /// <param name="matrix">事前に生成した DLXMatrix</param>
    /// <param name="board">"？" 以外の文字列が入った部分盤面 9×9</param>
    /// <returns>制約適用すべき DLXNode の列挙</returns>
    public static IEnumerable<DLXNode> EncodePartialBoard(DLXMatrix matrix, string[,] board)
    {
        var columnList = matrix.ColumnList;   // 直接アクセス
        for (int r = 0; r < 9; r++)
        {
            for (int c = 0; c < 9; c++)
            {
                string symbol = board[r, c]?.Trim();
                if (symbol == "？" || string.IsNullOrEmpty(symbol))
                    continue;

                // Kanji → 数字 1～9
                if (!Enum.TryParse<KandokuSymbol>(symbol, out var sym))
                    throw new InvalidOperationException($"Invalid symbol '{symbol}' at {r},{c}");

                int n = (int)sym;
                int baseBlock = (r / 3) * 3 + (c / 3);

                int[] colIdxs = {
                r * 9 + c,
                81 + r * 9 + (n - 1),
                2*81 + c * 9 + (n - 1),
                3*81 + baseBlock * 9 + (n - 1)
            };

                foreach (var idx in colIdxs)
                {
                    if (idx < 0 || idx >= columnList.Length)
                        throw new InvalidOperationException($"Column index out of range: {idx}");

                    var col = columnList[idx];
                    // col から下方向にたどり、RowID が一致するノードを返す
                    for (var node = col.Down; node != col; node = node.Down)
                    {
                        if (node.RowID == r * 81 + c * 9 + (n - 1))
                        {
                            yield return node;
                            break;
                        }
                    }
                }
            }
        }

    }

    // HasUniqueSolution の修正例
    private static bool HasUniqueSolution(string[,] maskedBoard)
    {
        var matrix = new DLXMatrix();
        var solver = new DLXSolver(matrix.Header);

        // 部分盤面の制約ノードを取得して Cover
        foreach (var node in EncodePartialBoard(matrix, maskedBoard))
        {
            // その行の全ノードに対して Cover
            foreach (var j in DLXMatrix.ToEnumerable(node.Right, x => x != node, x => x.Right))
                DLXMatrix.Cover(j.Column);
        }

        int count = 0;
        CountSolutions(matrix.Header, 2, ref count);
        return count == 1;
    }

    private static int CountSolutions(ColumnNode header, int limit, ref int count)
    {
        if (header.Right == header)
            return ++count;
        ColumnNode c = (ColumnNode)header.Right;
        for (ColumnNode j = (ColumnNode)c.Right; j != header; j = (ColumnNode)j.Right)
            if (j.Size < c.Size) c = j;
        DLXMatrix.Cover(c);
        for (DLXNode r = c.Down; r != c; r = r.Down)
        {
            foreach (var j in DLXMatrix.ToEnumerable(r.Right, x => x != r, x => x.Right))
                DLXMatrix.Cover(j.Column);
            if (CountSolutions(header, limit, ref count) >= limit)
                return count;
            foreach (var j in DLXMatrix.ToEnumerable(r.Left, x => x != r, x => x.Left))
                DLXMatrix.Uncover(j.Column);
        }
        DLXMatrix.Uncover(c);
        return count;
    }
}