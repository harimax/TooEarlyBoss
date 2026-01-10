using UnityEngine;

public class UpArrowAnimation : MonoBehaviour
{
     [SerializeField] private float moveSpeed = 10f; // 1秒で10上がる
    [SerializeField] private float duration = 1.0f; // 上昇し続ける時間
    private Vector3 initialPosition;
    private float time;
    void Start()
    {
        initialPosition = transform.position;
    }
    void Update()
    {
        if (time < duration)
        {
            time += Time.deltaTime;
            transform.Translate(0f, moveSpeed * Time.deltaTime, 0f);
            return;
        }
        ResetPosition();
    }
    /// <summary>矢印の位置とタイマーを初期状態に戻す。</summary>
    private void ResetPosition()
    {
        transform.position = initialPosition;
        time = 0f;
    }
}
