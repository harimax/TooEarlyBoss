using System;
using UnityEngine;
using Cysharp.Threading.Tasks;

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
            RespawnPlayerAsync(player, startPoint.transform.position).Forget();
        }
        else
        {
            // TitleScene など Player/StartPoint が無いシーンでも単に何もしないだけ
            Debug.Log($"PlayerSpawnOnSceneStart: Player or StartPoint not found in scene '{gameObject.scene.name}'");
        }
    }

    private async UniTaskVoid RespawnPlayerAsync(GameObject player, Vector3 spawnPosition)
    {
        // 位置移動の前に制御系を止めて、スポーン直後の押し出し・補正を防ぐ。
        // ここで "controller 側の地面補正" と "カプセル形状" を一旦安定化させる。
        var controller = player.GetComponent<Invector.vCharacterController.vThirdPersonController>();
        if (controller != null)
        {
            // コントローラを止めて、内部の移動/補正処理を止める。
            controller.enabled = false;
            // 地面判定・地面スナップを一時停止（補正が走らないようにする）。
            controller.disableCheckGround = true;
            controller.disableSnapToGround = true;
            // カプセル形状を初期状態に戻し、前シーンの姿勢変更をリセットする。
            controller.ResetCapsule();
            // カプセルを最大サイズに戻して地面貫通を起こしにくくする。
            controller.SetFullCapsuleHeight();
        }

        var rb = player.GetComponent<Rigidbody>();
        RigidbodyConstraints previousConstraints = RigidbodyConstraints.None;
        if (rb != null)
        {
            // 物理系を一時的に停止し、スポーン処理中に押し出しや沈み込みが起きないようにする。
            previousConstraints = rb.constraints;
            rb.isKinematic = true;
            rb.detectCollisions = false;
            rb.constraints = RigidbodyConstraints.FreezeAll;
        }

        // まずは指定位置に移動してから、地面スナップで正確な高さに合わせる。
        player.transform.position = spawnPosition;
        SnapPlayerToGround(player, spawnPosition);
        // Transform と Physics の同期を即時反映しておく。
        Physics.SyncTransforms();

        // 1フレーム待って物理状態を安定させる
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

        if (rb != null)
        {
            // 物理を元に戻して通常の挙動へ復帰させる。
            rb.detectCollisions = true;
            rb.isKinematic = false;
            rb.constraints = previousConstraints;
        }

        // 速度・角速度をゼロにして前シーンの慣性を完全に消す。
        ResetPlayerPhysics(player);

        // コントローラの地面補正を戻す前にもう1フレーム待機する。
        await UniTask.Yield(PlayerLoopTiming.FixedUpdate);

        if (controller != null)
        {
            // 地面判定・地面スナップを再開して通常状態に戻す。
            controller.disableCheckGround = false;
            controller.disableSnapToGround = false;
            // 最後にコントローラを再開する。
            controller.enabled = true;
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
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        // Transform の位置を Rigidbody に反映してズレを解消する。
        rb.position = player.transform.position;

        // 念のためスリープさせて停止状態を安定させる。
        rb.Sleep();
    }

    private void SnapPlayerToGround(GameObject player, Vector3 spawnPosition)
    {
        // スナップ時の高さ計算に使うカプセルコライダーを取得する。
        var collider = player.GetComponent<CapsuleCollider>();
        if (collider == null)
        {
            collider = player.GetComponentInChildren<CapsuleCollider>();
        }

        // スポーン位置より上からレイを飛ばして、最も近い地面を探す。
        float castHeight = 2f;
        float castDistance = 5f;
        var origin = spawnPosition + Vector3.up * castHeight;
        var hits = Physics.RaycastAll(origin, Vector3.down, castHeight + castDistance, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));
        for (int i = 0; i < hits.Length; i++)
        {
            var hit = hits[i];
            // 自分自身（または子階層）のコライダーは無視する。
            if (hit.collider == null || hit.collider.transform.IsChildOf(player.transform)) continue;

            // 地面のヒット位置にカプセル半径分の高さを足して、足元が地面に合う位置へ。
            float halfHeight = collider != null ? collider.bounds.extents.y : 0.5f;
            var snapped = hit.point + Vector3.up * (halfHeight + 0.02f);
            player.transform.position = snapped;
            // スナップ直後の Transform を物理に反映する。
            Physics.SyncTransforms();
            break;
        }
    }
}
