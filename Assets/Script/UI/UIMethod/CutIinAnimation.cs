using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class CutIinAnimation : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Image cutInImage;
    [SerializeField] private GameObject cutInPanel;
    [Header("Timing")]
    [SerializeField] private float inDuration = 0.35f;
    [SerializeField] private float holdDuration = 0.6f;
    [SerializeField] private float outDuration = 0.25f;
    [Header("Positions (Anchored X)")]
    [SerializeField] private float startX = 400f; // 右画面外
    [SerializeField] private float centerX = 0f;   // 画面内（表示位置）
    [SerializeField] private float endX = -400f;  // 左画面外
    [SerializeField] private float y = 0f;         // Y固定したい場合
    [Header("Ease")]
    [SerializeField] private Ease inEase = Ease.OutCubic;
    [SerializeField] private Ease outEase = Ease.InCubic;

    private Vector2 _baseAnchoredPos; // 起動時の基準位置
    // 連打対策：前回の演出をキャンセルする
    private CancellationTokenSource _cts;

    private void Awake()
    {
        // 起動時の正しい位置を保存
        _baseAnchoredPos = cutInImage.rectTransform.anchoredPosition;
    }

    /// <summary>
    /// UIボタンから呼ぶ用（InspectorでOnClickにこのメソッドを登録）
    /// </summary>
    public void PlayFromButton()
    {
        Play().Forget();
    }
    /// <summary>
    /// 外部からも呼べる再生API
    /// </summary>
    public async UniTask Play()
    {
        // 連打されたら前の演出を止める
        CancelCurrent();

        _cts = new CancellationTokenSource();
        try
        {
            await StartCutInAnimation(_cts.Token);
        }
        finally
        {
            CancelCurrent(); // 後始末
        }
    }
    private void CancelCurrent()
    {
        if (_cts == null) return;

        if (!_cts.IsCancellationRequested) _cts.Cancel();
        _cts.Dispose();
        _cts = null;
    }
    /// <summary>
    /// 実際の演出本体
    /// </summary>
    public async UniTask StartCutInAnimation(CancellationToken token)
    {
        // DOTween の前回Tweenが残ってたら止める（対象を指定してKill）
        cutInImage.DOKill();

        var rect = cutInImage.rectTransform;

        // Panel ON（表示開始）
        cutInPanel.SetActive(true);

        // 右 → 中央（カットイン）
        await rect
            .DOAnchorPosX(centerX, inDuration)
            .SetEase(inEase)
            .SetUpdate(true) // timeScale=0でも動く
            .ToUniTask(cancellationToken: token);

        // 少し止める
        await UniTask.Delay((int)(holdDuration * 1500f), ignoreTimeScale: true, cancellationToken: token);

        // 中央 → 左（カットアウト）
        await rect
            .DOAnchorPosX(endX, outDuration)
            .SetEase(outEase)
            .SetUpdate(true)
            .ToUniTask(cancellationToken: token);


        // Panel OFF（終了）
        cutInPanel.SetActive(false);
        rect.anchoredPosition = _baseAnchoredPos; // 初期位置に戻す
    }
}
