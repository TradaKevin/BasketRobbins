using BasketRobbins;
using UnityEngine;

public class HoopBottomTrigger : MonoBehaviour
{
    private GameManager gameManager;

    private void Start()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        if (gameManager == null)
            Debug.LogWarning("HoopBottomTrigger: GameManager not found!", this);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag(GameConstants.Tag.Ball)) return;

        if (!TryGetBallAndRb(other, out BallController ball, out Rigidbody rb)) return;

        if (!IsValidScore(ball, rb)) return;

        RegisterScore(ball);
    }

    private bool IsValidScore(BallController ball, Rigidbody rb)
    {
        if (!ball.scoringInProgress) return false; // never passed top trigger
        if (!ball.passedTopDownward) return false; // wasn't going down at top
        if (!ball.approachedFromAbove) return false; // didn't come from above zone
        if (ball.enteredFromBelow) return false; // exploit: thrown from below
        if (rb.velocity.y >= 0f) return false; // not going downward right now

        return true;
    }

    private void RegisterScore(BallController ball)
    {
        bool isSwish = !ball.touchedRim;

        if (isSwish)
        {
            Debug.Log("SWISH!");
            //gameManager.AudioPlay_Swish();
            gameManager.ScoreManage_AddScore(true);
            gameManager.PlayEffect_SwishScore();
        }
        else
        {
            Debug.Log("NORMAL SCORE");
            //gameManager.AudioPlay_Normal();
            gameManager.ScoreManage_AddScore(false);
            gameManager.PlayEffect_NormalScore();
        }

        ball.ResetState();
    }

    private bool TryGetBallAndRb(Collider col, out BallController ball, out Rigidbody rb)
    {
        ball = col.GetComponent<BallController>();
        rb = col.GetComponent<Rigidbody>();
        return ball != null && rb != null;
    }
}