using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Invector.vCamera;
public class LoadMainScene : MonoBehaviour
{
    private const string mainSceneTitle = "BattleScene";

    /// <summary>
    /// Awake is called when the script instance is being loaded.
    /// </summary>
    void Awake()
    {
        //前回のカメラが存在していればカメラを消す
        var camera=  FindAnyObjectByType<vThirdPersonCamera>();
        if(camera != null)
        {
            Destroy(camera);
        }
    }
    //　タイトルからゲームシーンへ移動する
    public void OnClickStartButton()
    {
        SceneManager.LoadScene(mainSceneTitle);
    }
}
