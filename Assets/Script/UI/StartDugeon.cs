using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Invector;
namespace Invector.vCharacterController
{
public class StartDugeon : MonoBehaviour
{
    [SerializeField] private GameObject MainCamera;
    [SerializeField] private GameObject trainingUI;
    [SerializeField] private GameObject GameUI;


    private void Start() {
    }
    public void StartDungeonMode()
    {
        MainCamera.SetActive(true);
        trainingUI.SetActive(false);
        GameUI.SetActive(true);
    }
    public void StartMission()
    {
        
    }
}
}
