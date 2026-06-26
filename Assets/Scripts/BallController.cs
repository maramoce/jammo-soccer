using System.Collections;
using UnityEngine;

public class BallController : MonoBehaviour
{
    [Header("Animation Curve & Speed")]
    public AnimationCurve kickCurve; 
    public float flyDuration = 1f;   
    public float heightMultiplier = 3f; 

    [Header("Effects")]
    public GameObject confettiPrefab; 

    private Rigidbody _rb;
    private CameraController _cam;
    private bool _isFlying = false;
    public bool isInGoal = false;
    public bool isFlying => _isFlying;

    [Header("Field Bounds")]
    public Vector3 boundsCenter = Vector3.zero;
    public Vector3 boundsSize = new Vector3(22f, 0f, 14f);

    void Update()
    {
        //CHẶN BÓNG khi bóng KHÔNG trong trạng thái bay vào gôn VÀ CHƯA ở trong lưới
        if (!_isFlying && !isInGoal)
        {
            Vector3 pos = transform.position;
            float minX = boundsCenter.x - boundsSize.x * 0.5f;
            float maxX = boundsCenter.x + boundsSize.x * 0.5f;
            float minZ = boundsCenter.z - boundsSize.z * 0.5f;
            float maxZ = boundsCenter.z + boundsSize.z * 0.5f;

            // Nếu bóng lăn quá biên, triệt tiêu vận tốc Rigidbody để bóng dừng 
            if (pos.x < minX || pos.x > maxX || pos.z < minZ || pos.z > maxZ)
            {
                if (_rb != null)
                {
                    Vector3 currentVelocity = _rb.velocity;
                    if (pos.x < minX || pos.x > maxX) currentVelocity.x = 0f; // Dừng trục X
                    if (pos.z < minZ || pos.z > maxZ) currentVelocity.z = 0f; // Dừng trục Z
                    _rb.velocity = currentVelocity;
                }
            }

            // Kẹp tọa độ bóng lại trong sân
            pos.x = Mathf.Clamp(pos.x, minX, maxX);
            pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
            transform.position = pos;
        }
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _cam = FindObjectOfType<CameraController>();

        kickCurve = new AnimationCurve(
            new Keyframe(0f, 0f, 0f, 4f),
            new Keyframe(0.5f, 1f, 0f, 0f),
            new Keyframe(1f, 0f, -4f, 0f)
        );
    }

    public void Kick()
    {
        if (_isFlying) return;

        //Tìm khung thành gần nhất có Tag là "Goal"
        GameObject[] goals = GameObject.FindGameObjectsWithTag("Goal");
        GameObject nearestGoal = null;
        float shortestDistance = Mathf.Infinity;

        foreach (GameObject goal in goals)
        {
            float distance = Vector3.Distance(transform.position, goal.transform.position);
            if (distance < shortestDistance)
            {
                shortestDistance = distance;
                nearestGoal = goal;
            }
        }

        if (nearestGoal != null)
        {
            // Bắt đầu luồng xử lý bay bổng dùng AnimationCurve theo sơ đồ của bạn
            StartCoroutine(FlyToGoalRoutine(nearestGoal.transform.position));
        }
    }

    private IEnumerator FlyToGoalRoutine(Vector3 targetPosition)
    {
        _isFlying = true;
        _rb.isKinematic = true; 

        // Ép Camera theo bóng 
        if (_cam != null) _cam.target = this.transform;

        Vector3 startPosition = transform.position;
        float elapsedTime = 0f;

        while (elapsedTime < flyDuration)
        {
            elapsedTime += Time.deltaTime;
            float timeRatio = elapsedTime / flyDuration; 

            // Tính tọa độ phẳng X và Z đi tới khung thành
            Vector3 currentPos = Vector3.Lerp(startPosition, targetPosition, timeRatio);

            if (kickCurve != null)
            {
                currentPos.y += kickCurve.Evaluate(timeRatio) * heightMultiplier;
            }

            transform.position = currentPos;
            yield return null;
        }

        transform.position = targetPosition; 


        if (confettiPrefab != null)
        {
            GameObject spawnedConfetti = Instantiate(confettiPrefab, transform.position, Quaternion.identity);

            ParticleSystem ps = spawnedConfetti.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                ps.Play();
            }
            else
            {
                ParticleSystem[] childPS = spawnedConfetti.GetComponentsInChildren<ParticleSystem>();
                foreach (var child in childPS)
                {
                    child.Play();
                }
            }

            Destroy(spawnedConfetti, 3f);
        }

        _rb.isKinematic = true; 

        yield return new WaitForSeconds(2f);

        GameObject player = GameObject.FindWithTag("Player");
        if (player != null && _cam != null)
        {
            _cam.target = player.transform;
        }
        isInGoal = true;
        _isFlying = false;
    }
}