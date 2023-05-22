using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FateGames.Core;
using UnityEditor;

public class LevelGenerator : FateMonoBehaviour
{
    public static LevelGenerator Instance = null;

    static public float Difficulty { get; set; } = 0.0f;
    [SerializeField] private int fullTubeCount = 5;
    [SerializeField] private int emptyTubeCount = 2;
    [SerializeField] private int tubeSize = 4;
    [SerializeField] private float widthPerTube = 180;
    [SerializeField] private float rowHeight = 600;
    [SerializeField] private int maxCountInRow = 6;
    [SerializeField] private GameObject tubePrefab = null;
    [SerializeField] private GameObject ballPrefab = null;
    [SerializeField] private Transform parent = null;
    [SerializeField] private BallOrderMaker ballOrderMaker = null;

    private void Awake()
    {

        Instance = this;
    }

    public void AddTube(List<Tube> tubes)
    {
        List<int> rowLengths = CreateRowOrder(tubes.Count + 1);
        tubes.Add(CreateTube());
        PlaceTubes(tubes, rowLengths);
    }

    public void GenerateLevel()
    {
        ClearArea();

        List<int> rowLengths = CreateRowOrder(fullTubeCount + emptyTubeCount);
        List<Tube> tubes = CreateTubes(fullTubeCount + emptyTubeCount);
        PlaceTubes(tubes, rowLengths);

        List<List<Ball>> balls = CreateBalls(fullTubeCount, tubeSize);
        List<List<int>> ballOrder = ballOrderMaker.MakeOrder(fullTubeCount, emptyTubeCount, tubeSize, Difficulty);
        PlaceBalls(tubes, balls, ballOrder);
    }

    private List<int> CreateRowOrder(int tubeCount)
    {
        List<int> rowOrder = new();

        int rowCount = Mathf.CeilToInt((float)tubeCount / maxCountInRow);

        int minTubeCountInRows = tubeCount / rowCount;
        int additionalTubesInFirstRows = tubeCount % rowCount;

        for (int i = 0; i < rowCount; i++)
        {
            rowOrder.Add(minTubeCountInRows + ((additionalTubesInFirstRows > 0) ? 1 : 0));
            additionalTubesInFirstRows--;
        }
        return rowOrder;
    }

    private void PlaceTubes(List<Tube> tubes, List<int> rowLengths)
    {
        int tubeIndex = 0;
        for (int i = 0; i < rowLengths.Count; i++)
        {
            for (int j = 0; j < rowLengths[i]; j++)
            {
                tubes[tubeIndex].transform.localPosition = (j + 0.5f - rowLengths[i] / 2f) * widthPerTube * Vector3.right
                    + (rowLengths.Count / 2f - (i + 0.5f)) * rowHeight * Vector3.up;
                tubeIndex++;
            }
        }
    }

    private List<Tube> CreateTubes(int count)
    {
        List<Tube> tubes = new();

        for (int i = 0; i < count; i++)
            tubes.Add(CreateTube());

        return tubes;
    }

    private Tube CreateTube()
    {
        Tube tube = null;
#if UNITY_EDITOR
        tube = (PrefabUtility.InstantiatePrefab(tubePrefab) as GameObject).GetComponent<Tube>();
#else
        tube = Instantiate(tubePrefab).GetComponent<Tube>();
#endif
        tube.SetSize(tubeSize);
        tube.transform.SetParent(parent);
        tube.transform.localScale = Vector3.one;
        return tube;
    }

    private void PlaceBalls(List<Tube> tubes, List<List<Ball>> balls, List<List<int>> ballOrder)
    {
        for (int i = 0; i < ballOrder.Count; i++)
        {
            for (int j = 0; j < ballOrder[i].Count; j++)
            {
                int ballIndex = ballOrder[i][j];
                Ball ball = balls[ballIndex][0];
                balls[ballIndex].RemoveAt(0);
                tubes[i].PutBallOnGeneration(ball.transform);
                ball.transform.localScale = Vector3.one;
            }
        }
    }

    private List<List<Ball>> CreateBalls(int tubeCount, int tubeSize)
    {
        List<List<Ball>> balls = new();
        for (int i = 0; i < tubeCount; i++)
        {
            List<Ball> column = new();
            for (int j = 0; j < tubeSize; j++)
            {
                Ball ball = CreateBall();
                ball.SetColor((Ball_Type)i);
                column.Add(ball);
            }
            balls.Add(column);
        }
        return balls;
    }

    private Ball CreateBall()
    {
        Ball ball = null;
#if UNITY_EDITOR
        ball = (PrefabUtility.InstantiatePrefab(ballPrefab) as GameObject).GetComponent<Ball>();
#else
        ball = Instantiate(ballPrefab).GetComponent<Ball>();
#endif
        return ball;
    }

    private void ClearArea()
    {
        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(parent.GetChild(i).gameObject);
        }
    }
}


