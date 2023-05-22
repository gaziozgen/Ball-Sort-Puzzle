using System;
using System.Collections;
using UnityEngine;

public class Move
{
    private Tube tube1 = null;
    private Tube tube2 = null;
    private int count = 0;

    public Move(Tube tube1, Tube tube2, int count)
    {
        this.tube1 = tube1;
        this.tube2 = tube2;
        this.count = count;
    }

    public IEnumerator Execute()
    {
        tube2.SetGettingBall(true);
        for (int i = 0; i < count; i++)
        {
            Action pushAction = null;
            if (i == count - 1) pushAction = () => tube2.SetGettingBall(false);
            tube1.RemoveBall(false, (Ball ball) => tube2.PushBall(ball, false, pushAction));
            yield return new WaitForSeconds(0.1f);
        }
    }

    public void Undo()
    {
        if (tube2.Finished) tube2.RevertClosing();
        for (int i = 0; i < count; i++)
            tube2.RemoveBall(true, (Ball ball) => tube1.PushBall(ball, true));
    }
}
