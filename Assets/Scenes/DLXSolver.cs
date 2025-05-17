using System;
using System.Collections.Generic;
using System.Linq;

public enum KandokuSymbol
{
    臨 = 1, 兵 = 2, 闘 = 3, 者 = 4, 皆 = 5, 陣 = 6, 烈 = 7, 在 = 8, 前 = 9
}

public enum KandokuDifficulty
{
    VeryEasy = 1, // 臨
    Easy = 2,     // 兵
    Normal = 3,   // 闘
    Hard = 4,     // 者
    VeryHard = 5, // 皆
    Extreme = 6,  // 陣
    Spicy = 7,    // 烈
    Insane = 8,   // 在
    Nightmare = 9,// 前
    Unknown = 10  // 臨兵闘者皆陣烈在前
}

// DLXノード
public class DLXNode
{
    public DLXNode Left, Right, Up, Down;
    public ColumnNode Column { get; set; } = null!;
    public int RowID;
    public DLXNode() => Left = Right = Up = Down = this;
}

// 列ノード
public class ColumnNode : DLXNode
{
    public int Size;
    public string Name;
    public ColumnNode(string name)
    {
        Column = this;
        Name = name;
        Size = 0;
    }
}

// DLX基盤（Exact Cover Matrix構築・DLX操作のみ）
public class DLXMatrix
{
    public ColumnNode Header { get; }
    // BuildExactCoverMatrix で作った配列をそのまま持ち回る
    public ColumnNode[] ColumnList { get; }

    public DLXMatrix()
    {
        ColumnList = BuildExactCoverMatrix(out var header);
        Header = header;
    }
    private static ColumnNode[] BuildExactCoverMatrix(out ColumnNode head)
    {
        const int totalColumns = 4 * 81;
        var columnList = new ColumnNode[totalColumns];
        head = new ColumnNode("head");
        ColumnNode previous = head;
        for (int i = 0; i < totalColumns; i++)
        {
            var col = new ColumnNode(i.ToString());
            columnList[i] = col;
            previous.Right = col;
            col.Left = previous;
            previous = col;
        }
        previous.Right = head;
        head.Left = previous;
        foreach (var (row, col, num) in
           from row in Enumerable.Range(0, 9)
           from col in Enumerable.Range(0, 9)
           from num in Enumerable.Range(1, 9)
           select (row, col, num))
        {
            int block = row / 3 * 3 + (col / 3);
            int[] columnIndices = new int[] {
                  row * 9 + col,
                    81 + row * 9 + (num - 1),
                    2 * 81 + col * 9 + (num - 1),
                    3 * 81 + block * 9 + (num - 1),
                };
            AddDLXRow(columnList, row, col, num, columnIndices);
        }
        return columnList;
    }


    private static void AddDLXRow(ColumnNode[] columnList, int r, int c, int n, int[] colIdx)
    {
        DLXNode first = null;
        foreach (var idx in colIdx)
        {
            var colNode = columnList[idx];
            var node = new DLXNode
            {
                Column = colNode,
                RowID = r * 81 + c * 9 + (n - 1),
                Down = colNode,
                Up = colNode.Up
            };
            colNode.Up.Down = node;
            colNode.Up = node;
            colNode.Size++;
            if (first == null)
            {
                first = node;
                node.Left = node.Right = node;
            }
            else
            {
                node.Right = first;
                node.Left = first.Left;
                first.Left.Right = node;
                first.Left = node;
            }
        }
    }

    public static void Cover(ColumnNode col)
    {
        col.Right.Left = col.Left;
        col.Left.Right = col.Right;
        for (DLXNode row = col.Down; row != col; row = row.Down)
            for (DLXNode j = row.Right; j != row; j = j.Right)
            {
                j.Down.Up = j.Up;
                j.Up.Down = j.Down;
                j.Column.Size--;
            }
    }

    public static void Uncover(ColumnNode col)
    {
        for (DLXNode row = col.Up; row != col; row = row.Up)
            for (DLXNode j = row.Left; j != row; j = j.Left)
            {
                j.Column.Size++;
                j.Down.Up = j;
                j.Up.Down = j;
            }
        col.Right.Left = col;
        col.Left.Right = col;
    }

    public static IEnumerable<DLXNode> ToEnumerable(DLXNode start, Func<DLXNode, bool> pred, Func<DLXNode, DLXNode> next)
    {
        for (var x = start; pred(x); x = next(x))
            yield return x;
    }
}

// DLX探索（解探索・シャッフルのみ）
public class DLXSolver
{
    private readonly ColumnNode header;

    public DLXSolver(ColumnNode header)
    {
        this.header = header;
    }

    private readonly Random rng = new();

    public bool Search(List<DLXNode> solution)
    {
        if (header.Right == header) return true;
        ColumnNode c = (ColumnNode)header.Right;
        for (ColumnNode j = (ColumnNode)c.Right; j != header; j = (ColumnNode)j.Right)
            if (j.Size < c.Size) c = j;
        DLXMatrix.Cover(c);
        var rows = new List<DLXNode>();
        for (DLXNode r = c.Down; r != c; r = r.Down)
            rows.Add(r);
        Shuffle(rows);
        foreach (var r in rows)
        {
            solution.Add(r);
            foreach (var j in DLXMatrix.ToEnumerable(r.Right, x => x != r, x => x.Right))
                DLXMatrix.Cover(j.Column);
            if (Search(solution)) return true;
            solution.RemoveAt(solution.Count - 1);
            foreach (var j in DLXMatrix.ToEnumerable(r.Left, x => x != r, x => x.Left))
                DLXMatrix.Uncover(j.Column);
        }
        DLXMatrix.Uncover(c);
        return false;
    }

    private void Shuffle<T>(IList<T> list)
    {
        if (list == null || list.Count <= 1)
            return;
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}
