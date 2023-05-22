using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using FateGames.Core;

public class GameArea : FateMonoBehaviour
{
    public static GameArea Instance = null;
    public Transform MovingBallParent = null;
    [SerializeField] private Transform parent = null;


    private Stack<Move> moves = new();
    private List<Tube> tubes = new();
    private Tube lastSelectedTube = null;

    private void Awake()
    {
        Instance = this;
        for (int i = 0; i < parent.childCount; i++)
            tubes.Add(parent.GetChild(i).GetComponent<Tube>());
    }

    public bool Undo()
    {
        if (!IsAllMovesFinished()) return false;

        if (moves.Count > 0)
        {
            DropRaisedBallIfAny();
            moves.Pop().Undo();
            return true;
        }
        else
        {
            print("move list is empty");
            return false;
        }
    }

    public void UndoAll()
    {
        if (!IsAllMovesFinished()) return;

        DropRaisedBallIfAny();
        while (moves.Count > 0)
            moves.Pop().Undo();

        for (int i = 0; i < tubes.Count; i++)
        {
            tubes[i].Highlight();
        }
    }

    public void AddTube()
    {
        if (!IsAllMovesFinished()) return;

        GetComponent<LevelGenerator>().AddTube(tubes);
    }

    public bool IsAllMovesFinished()
    {
        for (int i = 0; i < tubes.Count; i++)
            if (tubes[i].GettingBall) return false;
        return true;
    }

    public void OnTubeSelected(Tube tube)
    {
        GameManager.Instance.PlayHaptic();
        print("OnTubeSelected ##################");
        // bitmi� veya top alma i�lemi devam eden t�p se�ildiyse
        if (tube.Finished || tube.GettingBall)
        {
            if (tube.Finished) print("bitmi� t�p se�ildi");
            if (tube.GettingBall) print("i�lemi devam eden t�p se�ildi");
            return;
        }


        if (tube == lastSelectedTube) // ayn� t�p se�ilmi�se
        {
            print("ayn� t�p se�ildi");
            lastSelectedTube.DropFirstBall();
            lastSelectedTube = null;
            return;
        }

        if (lastSelectedTube) // se�ilmi� bir t�p varsa
        {
            if (HasMove(lastSelectedTube, tube))
            {
                print("hareket uygun ve yap�l�yor");
                Move move = new Move(lastSelectedTube, tube,
                        Mathf.Min(lastSelectedTube.NumberOfMovableBalls(), tube.SpaceInTube));

                StartCoroutine(move.Execute());
                moves.Push(move);
                lastSelectedTube = null;
            }
            else
            {
                print("hareket uygun de�il di�er t�p se�iliyor");
                lastSelectedTube.DropFirstBall();
                lastSelectedTube = tube;
                tube.RiseFirstBall();
            }
        }
        else  // se�ilmi� t�p yoksa
        {
            if (!tube.IsEmpty)
            {
                print("yeni t�p se�iliyor");
                lastSelectedTube = tube;
                tube.RiseFirstBall();
            }
            else print("bo� t�p");
        }
    }

    public void CheckFinish()
    {
        if (!IsAllMovesFinished()) return;

        for (int i = 0; i < tubes.Count; i++)
            if (!(tubes[i].Finished || tubes[i].IsEmpty))
                return;
        
        DOVirtual.DelayedCall(0.5f, () => GameManager.Instance.FinishLevel(true));
    }

    private void DropRaisedBallIfAny()
    {
        if (lastSelectedTube)
        {
            lastSelectedTube.DropFirstBall(true);
            lastSelectedTube = null;
        }
    }

    private bool HasMove(Tube tube1, Tube tube2)
    {
        bool hasMove = !tube2.IsFull && (tube2.IsEmpty || tube1.TopColor == tube2.TopColor);
        //print("Move: " + hasMove);
        return hasMove;
    }


}
