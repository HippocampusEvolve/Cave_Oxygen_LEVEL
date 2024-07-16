using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaveGenerator : MonoBehaviour
{
    public int width = 10;
    public int height = 10;
    public GameObject wallPrefab;
    public GameObject floorPrefab;
    public GameObject wallEndPrefab; // Префаб для закрывающих частей стен
    public float cellSize = 1.0f;
    public float wallTiltAngle = 0.0f; // Угол наклона стен
    public float floorTiltAngle = 0.0f; // Угол наклона пола

    private int[,] maze;
    private List<Vector2Int> stack = new List<Vector2Int>();

    void Start()
    {
        GenerateMaze();
        DrawMaze();
    }

    void GenerateMaze()
    {
        maze = new int[width, height];
        Vector2Int startPos = new Vector2Int(0, 0);
        stack.Add(startPos);
        maze[startPos.x, startPos.y] = 1;

        while (stack.Count > 0)
        {
            Vector2Int current = stack[stack.Count - 1];
            List<Vector2Int> neighbors = GetUnvisitedNeighbors(current);

            if (neighbors.Count > 0)
            {
                Vector2Int chosen = neighbors[Random.Range(0, neighbors.Count)];
                stack.Add(chosen);
                maze[chosen.x, chosen.y] = 1;
                RemoveWall(current, chosen);
            }
            else
            {
                stack.RemoveAt(stack.Count - 1);
            }
        }
    }

    List<Vector2Int> GetUnvisitedNeighbors(Vector2Int cell)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();

        if (cell.x > 1 && maze[cell.x - 2, cell.y] == 0)
            neighbors.Add(new Vector2Int(cell.x - 2, cell.y));
        if (cell.x < width - 2 && maze[cell.x + 2, cell.y] == 0)
            neighbors.Add(new Vector2Int(cell.x + 2, cell.y));
        if (cell.y > 1 && maze[cell.x, cell.y - 2] == 0)
            neighbors.Add(new Vector2Int(cell.x, cell.y - 2));
        if (cell.y < height - 2 && maze[cell.x, cell.y + 2] == 0)
            neighbors.Add(new Vector2Int(cell.x, cell.y + 2));

        return neighbors;
    }

    void RemoveWall(Vector2Int a, Vector2Int b)
    {
        Vector2Int wall = (a + b) / 2;
        maze[wall.x, wall.y] = 1;
    }

    void DrawMaze()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                Vector3 position = new Vector3(x * cellSize, 0, y * cellSize);
                if (maze[x, y] == 1)
                {
                    GameObject floor = Instantiate(floorPrefab, position, Quaternion.identity);
                    floor.transform.Rotate(Vector3.right, floorTiltAngle);
                }
                else
                {
                    Vector3 wallPosition = new Vector3(x * cellSize, 0.5f * cellSize, y * cellSize);
                    GameObject wall = Instantiate(wallPrefab, wallPosition, Quaternion.identity);
                    wall.transform.Rotate(Vector3.right, wallTiltAngle);

                    // Добавление закрывающих частей стен
                    if (x > 0 && maze[x - 1, y] == 1)
                    {
                        Vector3 wallEndPosition = new Vector3((x - 0.5f) * cellSize, 0.5f * cellSize, y * cellSize);
                        Instantiate(wallEndPrefab, wallEndPosition, Quaternion.identity);
                    }
                    if (x < width - 1 && maze[x + 1, y] == 1)
                    {
                        Vector3 wallEndPosition = new Vector3((x + 0.5f) * cellSize, 0.5f * cellSize, y * cellSize);
                        Instantiate(wallEndPrefab, wallEndPosition, Quaternion.identity);
                    }
                    if (y > 0 && maze[x, y - 1] == 1)
                    {
                        Vector3 wallEndPosition = new Vector3(x * cellSize, 0.5f * cellSize, (y - 0.5f) * cellSize);
                        Instantiate(wallEndPrefab, wallEndPosition, Quaternion.identity);
                    }
                    if (y < height - 1 && maze[x, y + 1] == 1)
                    {
                        Vector3 wallEndPosition = new Vector3(x * cellSize, 0.5f * cellSize, (y + 0.5f) * cellSize);
                        Instantiate(wallEndPrefab, wallEndPosition, Quaternion.identity);
                    }
                }
            }
        }
    }
}
