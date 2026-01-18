using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace Invector.vCharacterController
{
public class MeshTrailTut : MonoBehaviour
{
    private Animator animator;
    public float activetime=2.0f;
    [Header("Mesh Related")]
    public float meshRefreshRate=0.1f;
    private float meshDestroyDelay=0.4f;
    public Transform position;
    [Header("shader Related")]
    public Material[] mat;
    public string shaderVarRef;
    public float shaderVarRate=0.05f;
    public float shaderVarRefreshRate=0.05f;
    private vThirdPersonMotor vThirdPersonMotor;

    private bool isTrailActive;
    private SkinnedMeshRenderer[] skinnedMeshRenderers;
    // Start is called before the first frame update
    void Start()
    {
        // MeshRenderer meshRenderer=GetComponent<MeshRenderer>();
        // vThirdPersonMotor=GetComponentInParent<vThirdPersonMotor>();
        // mat=meshRenderer.materials;

        SkinnedMeshRenderer meshRenderer=GetComponent<SkinnedMeshRenderer>();
        vThirdPersonMotor=GetComponentInParent<vThirdPersonMotor>();
        mat=meshRenderer.materials;
    }

    // Update is called once per frame
    void Update()
    {
        if( !isTrailActive && vThirdPersonMotor.inputMagnitude >= 0.6f)
        {
            isTrailActive=true;
            StartCoroutine(ActivateTrail(activetime));
            // StartCoroutine(ActivateTrail(activetime-0.5f));

        }
    }
    IEnumerator ActivateTrail(float timeActive)
    {
            timeActive-=meshRefreshRate;
            if(skinnedMeshRenderers==null)
                skinnedMeshRenderers=GetComponentsInChildren<SkinnedMeshRenderer>();
            
            // Debug.Log(skinnedMeshRenderers.Length);  skinnedMeshRenderers.Length
            for(int i=0; i<skinnedMeshRenderers.Length; i++)
            {
                GameObject gboj=new GameObject();
                gboj.transform.SetPositionAndRotation(position.position,position.rotation);
                MeshRenderer mr= gboj.AddComponent<MeshRenderer>();
                MeshFilter mf=gboj.AddComponent<MeshFilter>();
                Mesh mesh= new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);
                mf.mesh =mesh;
                mr.materials=mat;
                // StartCoroutine(AnimateMaterialFloat(mr.material,0,shaderVarRate,shaderVarRefreshRate));
                Destroy(gboj,meshDestroyDelay);
            }
        yield return new WaitForSeconds(meshRefreshRate);
        isTrailActive=false;
    }
    // IEnumerator AnimateMaterialFloat(Material mat,float goal, float rate, float refreshRate)
    // {
    //     float valueToAnime=mat.GetFloat(shaderVarRef);
    //     Debug.Log(valueToAnime);

    //     while(valueToAnime >goal)
    //     {
    //         valueToAnime-=rate;
    //         mat.SetFloat(shaderVarRef,valueToAnime);
    //         yield return new  WaitForSeconds(refreshRate);
    //     }
    // }
}
}
