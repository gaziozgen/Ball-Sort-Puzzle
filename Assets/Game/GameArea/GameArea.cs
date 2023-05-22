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
        // bitmiþ veya top alma iþlemi devam eden tüp seçildiyse
        if (tube.Finished || tube.GettingBall)
        {
            if (tube.Finished) print("bitmiþ tüp seçildi");
            if (tube.GettingBall) print("iþlemi devam eden tüp seçildi");
            return;
        }


        if (tube == lastSelectedTube) // ayný tüp seçilmiþse
        {
            print("ayný tüp seçildi");
            lastSelectedTube.DropFirstBall();
            lastSelectedTube = null;
            return;
        }

        if (lastSelectedTube) // seçilmiþ bir tüp varsa
        {
            if (HasMove(lastSelectedTube, tube))
            {
                print("hareket uygun ve yapýlýyor");
                Move move = new Move(lastSelectedTube, tube,
                        Mathf.Min(lastSelectedTube.NumberOfMovableBalls(), tube.SpaceInTube));

                StartCoroutine(move.Execute());
                moves.Push(move);
                lastSelectedTube = null;
            }
            else
            {
                print("hareket uygun deðil diðer tüp seçiliyor");
                lastSelectedTube.DropFirstBall();
                lastSelectedTube = tube;
                tube.RiseFirstBall();
            }
        }
        else  // seçilmiþ tüp yoksa
        {
            if (!tube.IsEmpty)
            {
                print("yeni tüp seçiliyor");
                lastSelectedTube = tube;
                tube.RiseFirstBall();
            }
            else print("boþ tüp");
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
