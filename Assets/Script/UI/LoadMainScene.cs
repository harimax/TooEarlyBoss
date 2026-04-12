using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Invector.vCamera;
using UnityEngine.EventSystems;
public class LoadMainScene : MonoBehaviour
{
    private const string mainSceneTitle = "BattleScene";
    private GameObject lastSelected;
    public GameObject StartButton;
    private Button startButtonComponent;

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        //前回のカメラが存在していればカメラを消す
        var camera = FindAnyObjectByType<vThirdPersonCamera>();
        if (camera != null)
        {
            Destroy(camera);
        }
        if (StartButton != null)
        {
            startButtonComponent = StartButton.GetComponentInChildren<Button>(true);
        }
    }
    void Update()
    {
        // 表示中のボタンを選択状態にして「Aボタン=Submit」で押せるようにする
        UpdateDefaultSelection();
    }
    //　タイトルからゲームシーンへ移動する
    public void OnClickStartButton()
    {
        PlayerGrowRepository.DeleteParameters();
        SceneManager.LoadScene(mainSceneTitle);
    }
    private bool IsSubmitPressed()
    {
        // InputManagerのSubmitはゲームパッドAボタンと紐づいていることが多い
        return Input.GetButtonDown("Submit") || Input.GetKeyDown(KeyCode.JoystickButton0);
    }
    private void UpdateDefaultSelection()
    {
        // EventSystem が無い場合は何もしない
        if (EventSystem.current == null)
        {
            return;
        }

        // ボス戦開始ボタンが表示中なら、そのボタンを優先して選択
        if (StartButton != null && StartButton.activeSelf && startButtonComponent != null)
        {
            SelectButtonIfNeeded(startButtonComponent.gameObject);
            return;
        }
    }

    private void SelectButtonIfNeeded(GameObject target)
    {
        // 既に選択されている場合は何もしない
        if (lastSelected == target)
        {
            return;
        }

        lastSelected = target;
        EventSystem.current.SetSelectedGameObject(target);
    }
}
