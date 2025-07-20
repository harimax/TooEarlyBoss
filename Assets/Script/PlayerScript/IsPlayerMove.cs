using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsPlayerMove : MonoBehaviour
{
    //プレイヤーを動かしてよいかを判断するスクリプト
    private static IsPlayerMove _instance;
    public bool CanMove { get; set; } = false;  // 読み取り専用にしておくと安全
    public bool DebugCanmove;
    // 起動時に自身を取得
    void Awake()
    {
        CanMove = DebugCanmove;
        _instance = this;
    }
    //
    public static IsPlayerMove GetInstance()
    {
        return _instance;
    }
}
