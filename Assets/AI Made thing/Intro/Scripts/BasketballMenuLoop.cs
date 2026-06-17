using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Loops a decorative UI basketball shot over the menu background.
/// The green dots light up as the ball travels toward the hoop.
/// </summary>
public class BasketballMenuLoop : MonoBehaviour
{
    public RectTransform ball;
    public RectTransform[] trajectoryDots;
    public Image scoreFlash;

    public Vector2 startPoint = new Vector2(420f, -265f);
    public Vector2 controlPoint = new Vector2(610f, 120f);
    public Vector2 endPoint = new Vector2(790f, 170f);
    public float loopDuration = 3.4f;
    public float ballSpinSpeed = 420f;

    private void Update()
    {
        if (ball == null || loopDuration <= 0.01f)
        {
            return;
        }

        float time = Mathf.Repeat(Time.unscaledTime, loopDuration) / loopDuration;
        float shotT = Mathf.Clamp01((time - 0.08f) / 0.62f);
        Vector2 position = Bezier(startPoint, controlPoint, endPoint, shotT);
        ball.anchoredPosition = position;
        ball.localEulerAngles = new Vector3(0f, 0f, -Time.unscaledTime * ballSpinSpeed);

        float scale = Mathf.Lerp(1f, 0.62f, shotT);
        ball.localScale = new Vector3(scale, scale, 1f);

        UpdateDots(shotT, time);
        UpdateScoreFlash(time);
    }

    private void UpdateDots(float shotT, float loopT)
    {
        if (trajectoryDots == null)
        {
            return;
        }

        bool visible = loopT > 0.05f && loopT < 0.78f;
        for (int i = 0; i < trajectoryDots.Length; i++)
        {
            RectTransform dot = trajectoryDots[i];
            if (dot == null)
            {
                continue;
            }

            float dotT = trajectoryDots.Length <= 1 ? 0f : i / (float)(trajectoryDots.Length - 1);
            dot.anchoredPosition = Bezier(startPoint, controlPoint, endPoint, dotT);
            dot.gameObject.SetActive(visible && dotT <= shotT + 0.15f);

            Image image = dot.GetComponent<Image>();
            if (image != null)
            {
                float alpha = Mathf.Clamp01(1.2f - Mathf.Abs(shotT - dotT) * 2.8f);
                image.color = new Color(0.2f, 0.8f, 0.25f, alpha);
            }
        }
    }

    private void UpdateScoreFlash(float loopT)
    {
        if (scoreFlash == null)
        {
            return;
        }

        float flash = Mathf.Clamp01(1f - Mathf.Abs(loopT - 0.72f) * 12f);
        scoreFlash.color = new Color(1f, 0.8f, 0.2f, flash);
        float scale = 0.8f + flash * 0.7f;
        scoreFlash.rectTransform.localScale = new Vector3(scale, scale, 1f);
    }

    private Vector2 Bezier(Vector2 a, Vector2 b, Vector2 c, float t)
    {
        float oneMinusT = 1f - t;
        return oneMinusT * oneMinusT * a + 2f * oneMinusT * t * b + t * t * c;
    }
}
