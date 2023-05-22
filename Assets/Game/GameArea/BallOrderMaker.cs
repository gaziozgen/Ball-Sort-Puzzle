using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallOrderMaker : MonoBehaviour
{
    [SerializeField] private int worstCaseTryCount = 1000;
    private int tubeSize = 0;

    public List<List<int>> MakeOrder(int fullTubeCount, int extraTubeCount, int tubeSize, float difficulty)
    {
        print("Deneme baþladý");


        // setup
        this.tubeSize = tubeSize;
        List<List<int>> tubes = new();
        for (int i = 0; i < fullTubeCount; i++)
        {
            List<int> tube = new();
            for (int j = 0; j < tubeSize; j++)
            {
                tube.Add(i);
            }
            tubes.Add(tube);
        }
        for (int i = 0; i < extraTubeCount; i++) tubes.Add(new());


        // boþ tüp sayýsý kadar rastgele rengi sonrasý için ayýrma
        List<List<int>> unselectedTubes = CopyList(tubes);
        for (int i = 0; i < extraTubeCount; i++)
        {
            List<int> giverTube = unselectedTubes[Random.Range(0, unselectedTubes.Count - extraTubeCount)];
            unselectedTubes.Remove(giverTube);
            BallTransition(giverTube, tubes[fullTubeCount + i], AvaliableBallCount(giverTube));
        }


        // temel tüpleri artýk mümkün olmayana kadar karýþtýrma
        int counter = worstCaseTryCount;
        while (HasTubeWithAvaliableBalls(tubes, fullTubeCount) && counter > 0)
        {
            List<int> giver = null;
            int randomOffset = Random.Range(0, fullTubeCount);
            for (int i = 0; i < fullTubeCount; i++)
            {
                int tubeIndex = (i + randomOffset) % fullTubeCount;
                if ((giver == null || AvaliableBallCount(giver) < AvaliableBallCount(tubes[tubeIndex]))
                    && AvaliableBallCount(tubes[tubeIndex]) > 0)
                    giver = tubes[tubeIndex];
            }

            List<int> taker = null;
            randomOffset = Random.Range(0, fullTubeCount);
            for (int i = 0; i < fullTubeCount; i++)
            {
                int tubeIndex = (i + randomOffset) % fullTubeCount;
                if (SpaceInTube(tubes[tubeIndex]) > 0 && (taker == null || (AvaliableBallCount(taker) >= AvaliableBallCount(tubes[tubeIndex]) && !DoesCurrentColorDifferenceDisappear(taker, tubes[tubeIndex], giver))))
                    taker = tubes[tubeIndex];
            }

            int ballCount = Mathf.Min(AvaliableBallCount(giver), SpaceInTube(taker));
            if (Random.Range(0, 1f) > difficulty) ballCount = Random.Range(1, ballCount);

            BallTransition(giver, taker, ballCount);
            counter--;
        }
        print("Counter: " + counter);


        // extra tüplerde kalan toplarý boþluklara yerleþtirme
        for (int i = 0; HasUnfinishedMainTubes(tubes, fullTubeCount); i++)
        {
            for (int j = 0; j < fullTubeCount; j++)
            {
                if (SpaceInTube(tubes[j]) > 0)
                {
                    InsertBallToRandomPlace(tubes[fullTubeCount + (i % extraTubeCount)], tubes[j]);
                    break;
                }
            }
        }

        /*for (int i = 0; i < tubes.Count; i++)
        {
            string str = "";
            for (int j = 0; j < tubes[i].Count; j++)
            {
                str += tubes[i][j].ToString();
            }
            print(str);
        }*/

        return tubes;
    }

    private bool DoesCurrentColorDifferenceDisappear(List<int> previousTube, List<int> newTube, List<int> giver)
    {
        int giverColor = TopColorOfTube(giver);
        return TopColorOfTube(previousTube) != giverColor && TopColorOfTube(newTube) == giverColor;
    }

    private bool HasUnfinishedMainTubes(List<List<int>> tubes, int fullTubeCount)
    {
        for (int i = 0; i < fullTubeCount; i++)
            if (SpaceInTube(tubes[i]) > 0) return true;

        return false;
    }

    private bool HasTubeWithAvaliableBalls(List<List<int>> tubes, int fullTubeCount)
    {
        for (int i = 0; i < fullTubeCount; i++)
            if (AvaliableBallCount(tubes[i]) > 0) return true;

        return false;
    }

    private void BallTransition(List<int> from, List<int> to, int count)
    {
        for (int i = 0; i < count; i++)
        {
            to.Add(from[from.Count - 1]);
            from.RemoveAt(from.Count - 1);
        }
    }

    private void InsertBallToRandomPlace(List<int> from, List<int> to)
    {
        to.Insert(Random.Range(0, to.Count + 1), from[from.Count - 1]);
        from.RemoveAt(from.Count - 1);
    }

    private int TopColorOfTube(List<int> tube)
    {
        return tube[tube.Count - 1];
    }

    private int AvaliableBallCount(List<int> tube)
    {
        int total = -1;
        for (int i = tube.Count - 1; i > -1; i--)
        {
            if (TopColorOfTube(tube) == tube[i]) total++;
            else break;
        }

        return total;
    }

    private int SpaceInTube(List<int> tube)
    {
        return tubeSize - tube.Count;
    }


    private List<T> CopyList<T>(List<T> list)
    {
        List<T> copyList = new();
        for (int i = 0; i < list.Count; i++)
        {
            copyList.Add(list[i]);
        }
        return copyList;
    }
}
