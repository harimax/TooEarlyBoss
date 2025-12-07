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
            player.transform.position = startPoint.transform.position;
        }
        else
        {
            // TitleScene など Player/StartPoint が無いシーンでも単に何もしないだけ
            Debug.Log($"PlayerSpawnOnSceneStart: Player or StartPoint not found in scene '{gameObject.scene.name}'");
        }
    }
}
