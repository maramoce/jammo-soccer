using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Field Bounds")]
    public Vector3 boundsCenter = Vector3.zero;
    public Vector3 boundsSize = new Vector3(22f, 0f, 14f);

    [Header("Auto Kick Settings")]
    public float kickRadius = 1.2f;

    private Animator _anim;
    private static readonly int SpeedHash = Animator.StringToHash("Speed");

    private GameObject _targetBall;
    private BallController _targetBallScript;
    private bool _isAutoMoving = false;
    void Awake()
    {
        _anim = GetComponentInChildren<Animator>();
    }

    public void SetAutoKickTarget(GameObject ball)
    {
        if (ball == null) return;
        _targetBall = ball;
        _targetBallScript = ball.GetComponent<BallController>();

        if (_targetBallScript != null)
        {
            _isAutoMoving = true;
        }
    }
    void Update()
    {
        Vector3 dir = Vector3.zero;

        // CHẾ ĐỘ 1: Đang tự động chạy tới bóng (Ưu tiên khi bấm Auto Kick)
        if (_isAutoMoving && _targetBall != null)
        {
            // Lấy vị trí bóng nhưng giữ nguyên độ cao Y của nhân vật để tránh bị lún đất
            Vector3 targetPos = new Vector3(_targetBall.transform.position.x, transform.position.y, _targetBall.transform.position.z);
            Vector3 toTarget = targetPos - transform.position;
            float distance = toTarget.magnitude;

            if (distance <= kickRadius)
            {
                _isAutoMoving = false;
                _targetBallScript.Kick();

                _targetBall = null;
                _targetBallScript = null;
                dir = Vector3.zero;
            }
            else
            {
                // Tính hướng để chạy đến bóng
                dir = toTarget.normalized;

                // Tự động di chuyển
                transform.position += dir * moveSpeed * Time.deltaTime;

                // Tự động xoay mặt nhìn về phía quả bóng
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir),
                    15f * Time.deltaTime
                );
            }
        }
        // CHẾ ĐỘ 2: Di chuyển bằng bàn phím thông thường (Khi không bật Auto Kick)
        else
        {
            float h = Input.GetAxis("Horizontal"); 
            float v = Input.GetAxis("Vertical");   
            dir = new Vector3(h, 0f, v);

            if (dir.sqrMagnitude > 0.01f)
            {
                // Di chuyển tay
                transform.position += dir.normalized * moveSpeed * Time.deltaTime;

                // Nhìn về hướng di chuyển phím
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    Quaternion.LookRotation(dir.normalized),
                    15f * Time.deltaTime
                );
            }
        }

        // Giới hạn sân
        Vector3 pos = transform.position;
        pos.x = Mathf.Clamp(pos.x, boundsCenter.x - boundsSize.x * 0.5f,
                                    boundsCenter.x + boundsSize.x * 0.5f);
        pos.z = Mathf.Clamp(pos.z, boundsCenter.z - boundsSize.z * 0.5f,
                                    boundsCenter.z + boundsSize.z * 0.5f);
        transform.position = pos;

        _anim?.SetFloat(SpeedHash, dir.magnitude);
    }
}