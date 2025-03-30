using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening; // DOTween を使用
using System.Linq; // ★ これを追加！

public class SplineMoveSphere : MonoBehaviour
{
    // 親オブジェクト（インスペクターで設定）
    [SerializeField] private float moveDuration; // 全体の移動時間
    [SerializeField] private float rotateSpeed = 360f; // 回転速度（1秒あたりの回転角度）
    [SerializeField] private GameObject exprosion;

    void Start()
    {
        // 名前で親オブジェクトを取得
        GameObject _parentObject = GameObject.Find("movepoint");
        if (_parentObject == null) return;  // 親オブジェクトが設定されていない場合は処理を終了

        // 親オブジェクトの子オブジェクトを順番に取得
        Transform[] childTransforms = _parentObject.GetComponentsInChildren<Transform>();

        List<Vector3> path = new List<Vector3>();
        foreach (var childpoint in childTransforms)
        {
            if (childpoint != _parentObject.transform)  // 親オブジェクト自身は除外
            {
                path.Add(childpoint.position);  // 子オブジェクトの位置をリストに追加
            }
        }
        if (path.Count < 2) return;  // 子オブジェクトが2つ未満の場合は処理を終了

        // DOTweenのシーケンスを作成
        Sequence sequence = DOTween.Sequence();

        // 移動処理 子の位置を順番に等速で移動する
        sequence.Append(transform.DOPath(path.ToArray(), moveDuration, PathType.CatmullRom)
            .SetEase(Ease.Linear).SetLookAt(0.01f)); // 進行方向を向かせる;

        // 回転処理（X軸を一定速度で回転し続ける）
        sequence.Join(transform.DORotate(new Vector3(rotateSpeed * moveDuration, 0, rotateSpeed * moveDuration),
        moveDuration, RotateMode.LocalAxisAdd).SetEase(Ease.Linear));

        // アニメーション終了後、2〜3秒待って削除
        sequence.OnComplete(() =>
        {
            DOVirtual.DelayedCall(Random.Range(2f, 3f), () => DestroySphire());
        });
    }
    //爆発して消えるメソッド
    private void DestroySphire()
    {
        Instantiate(exprosion, gameObject.transform.position, gameObject.transform.rotation);
        Destroy(gameObject);
    }
}
