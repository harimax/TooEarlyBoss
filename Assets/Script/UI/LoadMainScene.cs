using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class LoadMainScene : MonoBehaviour
{
    private const string mainSceneTitle = "DunegonScene";
    //　タイトルからゲームシーンへ移動する
    public void OnClickStartButton()
    {
        SceneManager.LoadScene(mainSceneTitle);
    }
}
