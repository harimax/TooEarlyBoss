using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace Invector.vCharacterController
{
    public class MeshTrailTut : MonoBehaviour
    {
        public float activetime = 2.0f;
        [Header("Mesh Related")]
        public float meshRefreshRate = 0.1f;
        private float meshDestroyDelay = 0.4f;
        public Transform position;
        [Header("shader Related")]
        public Material[] mat;
        private vThirdPersonMotor vThirdPersonMotor;

        private bool isTrailActive;
        private SkinnedMeshRenderer[] skinnedMeshRenderers;
        // Start is called before the first frame update
        void Start()
        {

            SkinnedMeshRenderer meshRenderer = GetComponent<SkinnedMeshRenderer>();
            vThirdPersonMotor = GetComponentInParent<vThirdPersonMotor>();
            mat = meshRenderer.materials;
        }

        // Update is called once per frame
        void Update()
        {
            if (!isTrailActive && vThirdPersonMotor.inputMagnitude >= 0.6f)
            {
                isTrailActive = true;
                StartCoroutine(ActivateTrail(activetime));
            }
        }
        IEnumerator ActivateTrail(float timeActive)
        {
            timeActive -= meshRefreshRate;
            if (skinnedMeshRenderers == null)
                skinnedMeshRenderers = GetComponentsInChildren<SkinnedMeshRenderer>();

            // Debug.Log(skinnedMeshRenderers.Length);  skinnedMeshRenderers.Length
            for (int i = 0; i < skinnedMeshRenderers.Length; i++)
            {
                GameObject gboj = new GameObject();
                gboj.transform.SetPositionAndRotation(position.position, position.rotation);
                MeshRenderer mr = gboj.AddComponent<MeshRenderer>();
                MeshFilter mf = gboj.AddComponent<MeshFilter>();
                Mesh mesh = new Mesh();
                skinnedMeshRenderers[i].BakeMesh(mesh);
                mf.mesh = mesh;
                mr.materials = mat;
                // StartCoroutine(AnimateMaterialFloat(mr.material,0,shaderVarRate,shaderVarRefreshRate));
                Destroy(gboj, meshDestroyDelay);
            }
            yield return new WaitForSeconds(meshRefreshRate);
            isTrailActive = false;
        }

    }
}
