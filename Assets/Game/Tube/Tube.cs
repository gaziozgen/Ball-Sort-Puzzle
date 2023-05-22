using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using FateGames.Core;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class Tube : FateMonoBehaviour
{
    [SerializeField] private int size = 4;
    [SerializeField] private float ballDistance = 100;
    [SerializeField] private RectTransform ballParent = null;
    [SerializeField] private RectTransform peekPosition = null;
    [SerializeField] private Transform plugTarget = null;
    [SerializeField] private Transform plugParent = null;
    [SerializeField] private GameObject[] tubeSizes = null;
    [SerializeField] private RectTransform tubeTransform = null;
    [SerializeField] private SoundEntity TubeFinishSound = null;
    [SerializeField] private SoundEntity ballMoveSound = null;

    [SerializeField] private ParticleSystem finishEffect = null;
    [SerializeField] private Camera effectCamera = null;
    [SerializeField] private RawImage renderImage = null;
    [SerializeField] private Transform effectSetup = null;

    [SerializeField] private Color baseColor;
    [SerializeField] private Color highlightColor;
    [SerializeField] private List<Image> tubeImages = null;

    private RenderTexture renderTexture;
    private List<Ball> balls = new();

    public bool IsEmpty { get => balls.Count == 0; }
    public bool IsFull { get => balls.Count == size; }
    public int SpaceInTube { get => size - balls.Count; }
    public Ball_Type TopColor { get => balls[balls.Count - 1].GetColor(); }
    public bool Finished { get; private set; } = false;
    public bool GettingBall { get; private set; } = false;

    private Ball TopBall { get => balls[balls.Count - 1]; }


    private Vector3 plugParentInitialLocalPos;

    private void Awake()
    {
        SetupRenderTexture();
        plugParentInitialLocalPos = plugParent.localPosition;
        for (int i = 0; i < ballParent.childCount; i++)
            balls.Add(ballParent.GetChild(i).GetComponent<Ball>());
    }

    private void SetupRenderTexture()
    {
        effectSetup.parent = null;
        effectSetup.localScale = Vector3.one;

        renderImage.gameObject.SetActive(true);
        renderTexture = new RenderTexture(1024, 1024, 0);
        //renderTexture.depthStencilFormat = UnityEngine.Experimental.Rendering.GraphicsFormat.D16_UNorm;
        renderTexture.Create();

        effectCamera.targetTexture = renderTexture;
        renderImage.texture = renderTexture;
    }

    private void PlayConfettiEffect()
    {
        effectCamera.gameObject.SetActive(true);
        finishEffect.Play();
        DOVirtual.DelayedCall(3f, () => { effectCamera.gameObject.SetActive(false);}, false);
    }


    public int NumberOfMovableBalls()
    {
        int total = 0;
        for (int i = balls.Count - 1; i > -1; i--)
        {
            if (TopColor == balls[i].GetColor()) total++;
            else break;
        }
        return total;
    }

    public void RiseFirstBall()
    {
        GameManager.Instance.PlaySound(ballMoveSound);
        TopBall.transform.DOMove(peekPosition.position, 0.1f).OnComplete(() =>
        {
            TopBall.BounceFromTop();
        });
    }

    public void DropFirstBall(bool instant = false)
    {
        GameManager.Instance.PlaySound(ballMoveSound);
        Vector3 localTarget = LocalBallPosition(balls.Count - 1);
        if (instant) TopBall.transform.localPosition = localTarget;
        else
        {
            TopBall.transform.DOLocalMove(localTarget, 0.1f).OnComplete(() =>
            {
                TopBall.BounceFromBot();
            });
        }
    }

    public void PushBall(Ball ball, bool instant, Action callBack = null)
    {
        GameManager.Instance.PlaySound(ballMoveSound);
        balls.Add(ball);

        if (instant)
        {
            ball.transform.SetParent(ballParent);
            ball.transform.localPosition = LocalBallPosition(ballParent.childCount - 1);
        }
        else
        {
            ball.transform.DOMove(peekPosition.position, 0.1f).OnComplete(() =>
            {
                Vector3 target = /*ballParent.position +*/ LocalBallPosition(balls.Count - 1);

                ball.transform.SetParent(ballParent);
                ball.transform.DOLocalMove(target, 0.1f).OnComplete(() =>
                {
                    ball.BounceFromBot();
                    callBack?.Invoke();
                    CheckFinish();
                });
            });
        }
    }

    public void RemoveBall(bool instant, Action<Ball> callBack)
    {
        Ball ball = balls[balls.Count - 1];
        balls.Remove(ball);

        if (instant)
        {
            callBack(ball);
        }
        else
        {
            ball.transform.DOKill();
            ball.transform.DOMove(peekPosition.position, 0.1f).OnComplete(() =>
            {
                ball.transform.SetParent(GameArea.Instance.MovingBallParent);
                callBack(ball);
            });
        }
    }

    public void SetGettingBall(bool gettingBall)
    {
        GettingBall = gettingBall;
    }

    public void Select()
    {
        GameArea.Instance.OnTubeSelected(this);
        Highlight();
    }

    public void Highlight()
    {
        for (int i = 0; i < tubeImages.Count; i++)
        {
            if (tubeImages[i].isActiveAndEnabled)
            {
                tubeImages[i].DOColor(highlightColor, 0.1f).OnComplete(() =>
                {
                    tubeImages[i].DOColor(baseColor, 0.1f);
                });
                return;
            }
        }
    }

    public void SetSize(int size)
    {
        tubeSizes[this.size - 1].SetActive(false);
        tubeSizes[size - 1].SetActive(true);

        this.size = size;

        Vector2 currentSize = tubeTransform.sizeDelta;
        currentSize.y = size * ballDistance;
        tubeTransform.sizeDelta = currentSize;
    }

    private void CheckFinish()
    {
        if (!IsFull || GettingBall) return;

        for (int i = 0; i < size - 1; i++)
            if (balls[i].GetColor() != TopColor)
                return;

        CloseTube();
    }

    private void CloseTube()
    {
        GameManager.Instance.PlayHaptic();
        GameManager.Instance.PlaySound(TubeFinishSound);
        PlayConfettiEffect();
        Finished = true;
        plugParent.gameObject.SetActive(true);
        plugParent.DOMove(plugTarget.position, 0.2f).SetEase(Ease.InSine);
        GameArea.Instance.CheckFinish();
    }

    public void RevertClosing()
    {
        Finished = false;
        plugParent.localPosition = plugParentInitialLocalPos;
        plugParent.gameObject.SetActive(false);
    }

    private Vector3 LocalBallPosition(int ballIndex)
    {
        return (ballIndex + 0.5f) * ballDistance * Vector3.up;
    }

    public void PutBallOnGeneration(Transform ball)
    {
        ball.SetParent(ballParent);
        ball.localPosition = LocalBallPosition(ballParent.childCount - 1);
    }
}
