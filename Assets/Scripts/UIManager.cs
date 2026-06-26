using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("UI Buttons")]
    public GameObject kickButton;     
    public Button autoKickButton;  
    public Button resetButton;       

    [Header("Settings")]
    public float detectRadius = 3f;    // Khoảng cách đứng gần bóng để hiện nút Kick

    private Transform _playerTransform;
    private BallController _closestBall;
    private GameObject[] _balls;

    void Start()
    {
        _balls = GameObject.FindGameObjectsWithTag("Ball");
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null) _playerTransform = player.transform;

        if (kickButton != null) kickButton.GetComponent<Button>().onClick.AddListener(OnKickClick);
        if (autoKickButton != null) autoKickButton.onClick.AddListener(OnAutoKickClick);
        if (resetButton != null) resetButton.onClick.AddListener(OnResetClick);

        if (kickButton != null) kickButton.SetActive(false); // Mặc định ẩn nút Kick khi mới vào game
    }

    void Update()
    {
        if (_playerTransform == null) return;
        FindClosestBall();
    }

    void FindClosestBall()
    {
        GameObject closest = null;
        float minDistance = Mathf.Infinity;

        foreach (GameObject ball in _balls)
        {
            float distance = Vector3.Distance(_playerTransform.position, ball.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = ball;
            }
        }

        if (minDistance <= detectRadius && closest != null)
        {
            kickButton.SetActive(true);
            _closestBall = closest.GetComponent<BallController>();
        }
        else
        {
            kickButton.SetActive(false);
            _closestBall = null;
        }
    }

    void OnKickClick()
    {
        if (_closestBall != null)
        {
            _closestBall.Kick();
        }
    }


void OnAutoKickClick()
{
    if (_playerTransform == null) return;

    PlayerController playerScript = _playerTransform.GetComponent<PlayerController>();
    if (playerScript == null)
    {
        return;
    }

    GameObject farthestValidBall = null;
    float maxDistance = -1f;

    foreach (GameObject ball in _balls)
    {
        float distance = Vector3.Distance(_playerTransform.position, ball.transform.position);
        BallController ballScript = ball.GetComponent<BallController>();

        if (ballScript != null && !ballScript.isFlying && !ballScript.isInGoal && distance > maxDistance)
        {
            maxDistance = distance;
            farthestValidBall = ball;
        }
    }

    if (farthestValidBall != null)
    {
        playerScript.SetAutoKickTarget(farthestValidBall);
    }
}

    void OnResetClick()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}