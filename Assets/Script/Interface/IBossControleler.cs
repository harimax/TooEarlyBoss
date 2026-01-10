using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// ボスの基本的なインターフェース
/// </summary>
public interface IBossController
{
    void PauseBoss();
    void ResumeBoss();
    void DeadTrigger();
}

