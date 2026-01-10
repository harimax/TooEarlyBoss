using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComeGameScene : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] Fade fade;
    void Start()
    {
        if (fade != null || fade.enabled == true)
        {
            fade.FadeOut(1.0f);
        }
    }
}
