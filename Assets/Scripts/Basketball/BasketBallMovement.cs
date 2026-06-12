using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class BasketBallMovement : MonoBehaviour
{

    #region Public Variables

    public float shootingCooldown = 1.0f;
    public float turnSpeed = 3.0f;
    public float ballUpDownSpeed = 5.0f;
    public float targetHeight = 0.0f;
    public float samplingRate = 0.2f;
    public float ballDrag = 0.2f;
    public float crosshairOffset = 0.01f;

    [Header("Power / Shoot Velocity")]
    public float minShootVelocity = 15.0f;   // slider at 0
    public float maxShootVelocity = 40.0f;  // slider at 1

    [Range(0f, 1f)]
    public float power = 0.5f;              // normalized [0,1] — driven by UI slider

    // Read-only computed velocity — always derived from power, never set directly
    public float shootVelocity => Mathf.Lerp(minShootVelocity, maxShootVelocity, power);

    // How many frames to skip between trajectory recalculations (2-4 is good for mobile)
    public int trajectoryUpdateInterval = 3;

    public GameObject crosshair;
    public LineRenderer lineRenderer;
    public Transform ballTransform;
    public Vector2 ballUpDownRange;
    public Vector3 ballInitialPosition;

    #endregion



    #region Private Variables

    private bool isShooting = false;
    private float currentBallAngle = 0.0f;
    private float ballRadius;
    private bool canBallMove = false;

    // Cached components — fetched once in Start, never again
    private Rigidbody ballRigidbody;

    // Reusable array to avoid ToArray() allocation every frame
    private Vector3[] linePositionsBuffer = new Vector3[101];

    // Frame counter for trajectory throttling
    private int frameCounter = 0;

    // Cache last input to skip trajectory update when nothing moved
    private bool trajectoryDirty = true;

    #endregion

    private void Awake()
    {
        ballRigidbody = ballTransform.GetComponent<Rigidbody>();
        ballRadius = ballTransform.GetComponent<SphereCollider>().radius;

        currentBallAngle = 0.0f;
        ballInitialPosition = ballTransform.position;
    }

    private void OnEnable()
    {
        GameManager.OnSimulationStart_Action += SimulationStart;
        GameManager.OnSimulationStop_Action += SimulationStop;

        GameManager.OnBallResetPosition_Action += ballResetPosition;
        GameManager.OnBallShoot_Action += BallShoot;
    }

    private void OnDisable()
    {
        GameManager.OnSimulationStart_Action -= SimulationStart;
        GameManager.OnSimulationStop_Action -= SimulationStop;

        GameManager.OnBallResetPosition_Action -= ballResetPosition;
        GameManager.OnBallShoot_Action -= BallShoot;
    }

    void SimulationStart()
    {
        canBallMove = true;
        ballResetPosition();
    }

    void SimulationStop()
    {
        canBallMove = false;
        ballResetPosition();
    }

    void Start()
    {
        if(GameManager.Instance)
        {
            GameManager.Instance.ballMovement_Script = this;
        }
        
    }

    /// <summary>
    /// Called by the UI slider. value must be in [0, 1].
    /// Automatically marks trajectory dirty so the preview updates instantly.
    /// </summary>
    public void SetPower(float value)
    {
        float newPower = Mathf.Clamp01(value);
        if (!Mathf.Approximately(newPower, power))
        {
            power = newPower;
            trajectoryDirty = true;
        }
    }

    private IEnumerator Shoot()
    {
        isShooting = true;
        Vector3 shootDirection = transform.forward; // turret's forward

        // Capture velocity at shoot moment — perfectly synced with current power
        float velocity = shootVelocity;

        ballRigidbody.isKinematic = false;
        ballRigidbody.WakeUp();
        ballRigidbody.drag = Mathf.Max(ballDrag, 0.0f);
        ballRigidbody.velocity = Vector3.zero;
        ballRigidbody.AddForce(shootVelocity * shootDirection, ForceMode.VelocityChange);
        ballRigidbody.angularVelocity = ballTransform.transform.right * 10f;

        yield return new WaitForSeconds(shootingCooldown);
    }

    private void HandleInput()
    {
        if (!canBallMove) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool hasInput = horizontal != 0f || vertical != 0f;

        if (hasInput)
        {
            trajectoryDirty = true;

            if (horizontal != 0f)
                transform.Rotate(Vector3.up, horizontal * turnSpeed * Time.deltaTime);

            if (vertical != 0f)
            {
                currentBallAngle += vertical * this.ballUpDownSpeed * Time.deltaTime;
                currentBallAngle = Mathf.Clamp(currentBallAngle, ballUpDownRange.y, ballUpDownRange.x);

                Vector3 rot = transform.localEulerAngles;
                rot.x = currentBallAngle;
                transform.localEulerAngles = rot;
            }
        }
    }

    public void BallShoot()
    {
        if (!isShooting)
        {
            StartCoroutine(Shoot());
        }
    }

    private void CalculateTrajectoryLine()
    {
        frameCounter++;

        // If nothing changed AND not enough frames passed — skip
        if (!trajectoryDirty && frameCounter < trajectoryUpdateInterval) return;

        // If enough frames passed without change — also skip
        if (!trajectoryDirty) return;
        trajectoryDirty = false;

        frameCounter = 0;

        List<Vector3> lrPositions = ListPool<Vector3>.Get();

        Vector3 previousPosition = transform.position;
        Vector3 currentPosition = previousPosition;
        float currentTime = 0.0f;

        lrPositions.Add(currentPosition);

        // Use the property so trajectory always reflects current power
        float vel = shootVelocity;

        while (true)
        {
            currentTime += samplingRate;
            currentPosition = Trajectory.GetPosition(
                transform.position,
                vel * transform.forward, 
                ballDrag, 
                currentTime
                );

            if (Physics.Linecast(previousPosition, currentPosition, out RaycastHit hit))
            {
                if (!hit.collider.isTrigger)
                {
                    lrPositions.Add(hit.point);
                    break;
                }
                    
            }

            if (currentPosition.y <= targetHeight)
            {
                lrPositions.Add(currentPosition);
                break;
            }

            lrPositions.Add(currentPosition);
            previousPosition = currentPosition;

            if (lrPositions.Count > linePositionsBuffer.Length) break;

        }

        int count = lrPositions.Count;
        lineRenderer.positionCount = count;

        for (int i = 0; i < count; i++)
            linePositionsBuffer[i] = lrPositions[i];

        lineRenderer.SetPositions(linePositionsBuffer);

        ListPool<Vector3>.Release(lrPositions);
    }

    private void PlaceCrosshair()
    {
        Vector3 shootDirection = transform.forward;
        float vel = shootVelocity;

        Vector3 previousPosition = ballTransform.position;
        Vector3 currentPosition = previousPosition;
        float currentTime = 0f;

        Vector3 hitPoint = Vector3.zero;
        Vector3 hitNormal = Vector3.up; // default for flat ground

        while (true)
        {
            currentTime += samplingRate;
            currentPosition = Trajectory.GetPosition(ballTransform.position, vel * shootDirection, ballDrag, currentTime);

            Vector3 segmentDir = currentPosition - previousPosition;
            float segmentLength = segmentDir.magnitude;

            if (segmentLength > 0f)
            {
                // SphereCast for collision considering ball radius
                if (Physics.SphereCast(previousPosition, ballRadius, segmentDir.normalized, out RaycastHit hit, segmentLength))
                {
                    hitPoint = hit.point;
                    hitNormal = hit.normal;
                    break;
                }
            }

            // Stop if reaching target height
            if (currentPosition.y <= targetHeight)
            {
                hitPoint = currentPosition;
                hitNormal = Vector3.up; // flat ground
                break;
            }

            previousPosition = currentPosition;

            if (currentTime > 5f) break; // safety
        }

        // Place crosshair slightly above the surface
        crosshair.transform.position = hitPoint + Vector3.up * crosshairOffset;

        // Rotate crosshair to align with surface normal
        crosshair.transform.rotation = Quaternion.FromToRotation(Vector3.up, hitNormal) * Quaternion.Euler(90f, 0f, 0f);
    }

    void Update()
    {
        if(isShooting) return;

        HandleInput();

        if (samplingRate > 0.0f)
        {
            CalculateTrajectoryLine();
        }


        //PlaceCrosshair();
    }

    public void ballResetPosition()
    {
        ballRigidbody.isKinematic = true;
        ballTransform.position = ballInitialPosition;
        ballTransform.localRotation = Quaternion.identity;

        isShooting = false;
        trajectoryDirty = true;
    }
}
