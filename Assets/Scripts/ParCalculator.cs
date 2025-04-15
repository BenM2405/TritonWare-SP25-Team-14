using System.Collections.Generic;
using UnityEngine;

public class ParCalculator : MonoBehaviour
{
    public int CalculatePar(Tile[,] start, Tile[,] goal)
    {
        int width = start.GetLength(0);
        int height = start.GetLength(1);

        string startKey = SerializeState(start);
        string goalKey = SerializeState(goal);

        Queue<(string state, int moves)> queue = new Queue<(string, int)>();
        HashSet<string> visited = new HashSet<string>();

        queue.Enqueue((startKey, 0));
        visited.Add(startKey);

        while (queue.Count > 0)
        {
            var (state, moves) = queue.Dequeue();

            if (state == goalKey)
                return moves;

            foreach (string neighbor in GetNeighbors(state, width, height))
            {
                if (!visited.Contains(neighbor))
                {
                    visited.Add(neighbor);
                    queue.Enqueue((neighbor, moves + 1));
                }
            }
        }

        return -1;
    }

    private string SerializeState(Tile[,] state)
    {
        string result = "";
        for (int x = 0; x < state.GetLength(0); x++)
        {
            for (int y = 0; y < state.GetLength(1); y++)
            {
                result += state[x, y].currentSymbol + ",";
            }
        }
        return result.TrimEnd(',');
    }

    private IEnumerable<string> GetNeighbors(string state, int width, int height)
    {
        string[] cells = state.Split(',');
        Tile.SymbolType[,] grid = new Tile.SymbolType[width, height];

        int i = 0;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                grid[x, y] = (Tile.SymbolType)System.Enum.Parse(typeof(Tile.SymbolType), cells[i++]);

        List<string> neighbors = new List<string>();

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };

                foreach (Vector2Int dir in directions)
                {
                    int nx = x + dir.x;
                    int ny = y + dir.y;
                    if (nx >= 0 && nx < width && ny >= 0 && ny < height)
                    {
                        var temp = grid[x, y];
                        grid[x, y] = grid[nx, ny];
                        grid[nx, ny] = temp;

                        List<string> serialized = new List<string>();
                        for (int sx = 0; sx < width; sx++)
                            for (int sy = 0; sy < height; sy++)
                                serialized.Add(grid[sx, sy].ToString());
                        neighbors.Add(string.Join(",", serialized));

                        temp = grid[x, y];
                        grid[x, y] = grid[nx, ny];
                        grid[nx, ny] = temp;
                    }
                }
            }
        }

        return neighbors;
    }
}
