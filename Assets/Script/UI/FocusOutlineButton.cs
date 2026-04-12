using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class FocusOutlineButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    private Outline outline;
    // Unity Editor上で「Reset」が呼ばれるタイミングで実行されるメソッド
    // コンポーネントを追加した直後
    // Inspectorの三点メニューからResetした時 など
    private void Reset()
    {
        // 同じGameObjectについているOutlineを自動取得して
        outline = GetComponent<Outline>();
    }
    // GameObjectが生成されて有効化された時に一度だけ呼ばれる
    private void Awake()
    {
        // もしInspectorでoutlineが未設定なら、自動でOutlineを探して入れる
        if (outline == null) outline = GetComponent<Outline>();

        // Outlineが存在する場合は、最初は非表示（無効）にしておく
        if (outline != null) outline.enabled = false; // 初期は消す
    }
    // EventSystemによって「このUIが選択された」ときに呼ばれる（ゲームパッド/キーボードのフォーカス移動など）
    public void OnSelect(BaseEventData eventData)
    {
        // Outlineが存在するなら有効化して、選択中であることを可視化する
        if (outline != null) outline.enabled = true;
    }

    // EventSystemによって「このUIの選択が外れた」ときに呼ばれる
    public void OnDeselect(BaseEventData eventData)
    {
        // Outlineが存在するなら無効化して、選択枠を消す
        if (outline != null) outline.enabled = false;
    }
}
