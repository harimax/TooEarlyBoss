using UnityEngine;

namespace Invector
{
    public class vHealthItem : MonoBehaviour
    {
        [Tooltip("How much health will be recovery")]
        public float value;
        public string tagFilter = "Player";

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag(tagFilter))
            {
                // access the basic character information
                var healthController = other.GetComponent<vHealthController>();
                if (healthController != null)
                {
                        // limit healing to the max health
                        healthController.AddHealth((int)value);
                        Destroy(gameObject);              
                }
            }
        }
    }
}