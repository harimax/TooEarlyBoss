using UnityEngine;

public class PlayerSpawnOnSceneStart : MonoBehaviour
{
    /// <summary>
    /// シーン開始時に Player を StartPoint にワープさせるだけのコンポーネント
    /// ボスシーン・バトルシーンの両方に使える
    /// </summary>
    private void Start()
    {
        var player = GameObject.FindWithTag("Player");
        var startPoint = GameObject.FindWithTag("StartPoint");

        if (player != null && startPoint != null)
        {
            // シーン遷移後に開始位置へワープするだけだと、前シーンの速度が残り続ける。
            // そこで「位置移動 + 物理の初期化」をセットで行い、
            // ボス/実践クリア後に育成へ戻っても勝手に動かないようにする。
            player.transform.position = startPoint.transform.position;
            ResetPlayerRotationY(player);
            ResetPlayerPhysics(player);
        }
        else
        {
            // TitleScene など Player/StartPoint が無いシーンでも単に何もしないだけ
            Debug.Log($"PlayerSpawnOnSceneStart: Player or StartPoint not found in scene '{gameObject.scene.name}'");
        }
    }

    /// <summary>
    /// プレイヤーの物理速度をリセットして、前シーンの慣性が残らないようにする。
    /// </summary>
    /// <param name="player">対象のプレイヤー</param>
    private void ResetPlayerPhysics(GameObject player)
    {
        var rb = player.GetComponent<Rigidbody>();
        if (rb == null) return;

        // 速度・角速度をゼロにして、慣性による勝手な移動を止める。
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 念のためスリープさせて停止状態を安定させる。
        rb.Sleep();
    }

    /// <summary>
    /// プレイヤーのY軸回転だけを0に戻す。
    /// </summary>
    /// <param name="player">対象のプレイヤー</param>
    private void ResetPlayerRotationY(GameObject player)
    {
        var euler = player.transform.eulerAngles;
        euler.y = 0f;
        player.transform.eulerAngles = euler;
    }
}
