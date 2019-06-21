using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private EnemySegment enemySegmentPrefab;
    [SerializeField]
    private BoxCollider2D collider;
    [SerializeField]
    private Rigidbody2D rb;

    
    private RectTransform rectTransfrom;

    private List<EnemySegment> segments = new List<EnemySegment>();

    private string targetString;
    private int currentNumberOfSegments;
    private int currentActiveSegmentIndex;

    // Movement
    private float nextChangeTargetPositionTime;
    private Vector2 targetPosition;
    private float currentSpeed;

    private Vector2 targetDirection;
    private Plane[] frustrumPlanes;

    private bool usePhysics;

    #endregion

    #region Properties


    #endregion

    #region Unity Lifecycle

    private void Awake()
    {
        rectTransfrom = (RectTransform)transform;
        frustrumPlanes = GeometryUtility.CalculateFrustumPlanes(Camera.main);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            usePhysics = !usePhysics;
        }

        if (!usePhysics)
        {
            rb.isKinematic = true;

            if (Time.time > nextChangeTargetPositionTime)
            {
                // TODO: Don't hard code the min and max position values, calculate based on screen size
                targetPosition = new Vector2(Random.Range(-7f, 7f), Random.Range(-4.5f, 4.5f));

                // TODO: Also don't hard code the min and max here
                nextChangeTargetPositionTime = Time.time + Random.Range(1f, 3f);

                // TODO: Ditto
                currentSpeed = Random.Range(1f, 3f);
            }

            transform.position = Vector2.MoveTowards(transform.position, targetPosition, currentSpeed * Time.deltaTime);
        }
    }

    private void FixedUpdate()
    {
        if (usePhysics)
        {
            rb.isKinematic = false;

            // Also check if it's within the camera's frustrum planes, ie is it visible
            if (Time.time > nextChangeTargetPositionTime || !GeometryUtility.TestPlanesAABB(frustrumPlanes, collider.bounds))
            {
                rb.velocity = Vector2.zero;

                // TODO: Don't hard code the min and max position values, calculate based on screen size
                var targetPosition = new Vector3(Random.Range(-7f, 7f), Random.Range(-4.5f, 4.5f), 0);
                targetDirection = targetPosition - transform.position;
                targetDirection.Normalize();

                // TODO: Also don't hard code the min and max here
                nextChangeTargetPositionTime = Time.time + Random.Range(1f, 3f);

                // TODO: Ditto
                currentSpeed = Random.Range(1f, 3f);
            }

            rb.AddForce(targetDirection * currentSpeed);
        }
    }

    #endregion

    public void ActivateEnemy(Vector2 position, string newTarget)
    {
        if (string.IsNullOrEmpty(newTarget))
        {
            Debug.LogError("ENEMY TARGET STRING IS NULL! :(", this);
            return;
        }

        transform.position = position;
        targetString = newTarget;

        // TODO: Randomize the length instead of hardcoding the value 3 everywhere
        currentNumberOfSegments = Mathf.CeilToInt(targetString.Length / 3f);
        for(int i = 0; i < currentNumberOfSegments; i++)
        {
            if(segments.Count == i)
            {
                var newSegment = Instantiate(enemySegmentPrefab);
                newSegment.transform.SetParent(transform, worldPositionStays: false);
                segments.Add(newSegment);
            }

            segments[i].EnemySegmentStateChangeEvent += OnSegmentStateChanged;

            var substring = targetString.Substring(i * 3, Mathf.Min(3, targetString.Length - (i*3)));
            segments[i].SetTargetString(substring);
        }

        currentActiveSegmentIndex = 0;
        segments[currentActiveSegmentIndex].ActivateSegment();

        // Adjust collider size
        StartCoroutine(UpdateColliderSize());

        IEnumerator UpdateColliderSize()
        {
            // Need to wait for the GUI to render first, so that the rect transform will have the updated bounds
            yield return new WaitForEndOfFrame();
            collider.size = rectTransfrom.sizeDelta;
        }
    }

    public void OnAlphabetImpact(char charToTry)
    {
        segments[currentActiveSegmentIndex].TryMarkChar(charToTry);
    }

    private void OnSegmentStateChanged(EnemySegment segment)
    {
        // Activate the next segment
        if(segment.CurrentState == EnemySegment.EnemySegmentState.Completed)
        {
            if (currentActiveSegmentIndex + 1 < currentNumberOfSegments)
            {
                segments[++currentActiveSegmentIndex].ActivateSegment();
            }
            else
            {
                // TODO: All completed, now what
            }
        }
        else if(segment.CurrentState == EnemySegment.EnemySegmentState.Destroyed)
        {
            // TODO: Once all segments are destroyed, we need to disable them and stop listening for this event
        }

    }
}
