using UnityEngine;

public class PlayerRideTrigger : MonoBehaviour
{
    //プレイヤーが乗れば子にする
    void OnTriggerStay(Collider other)
    {
        if (other.transform.parent != transform && other.transform.CompareTag("Player") && other.GetComponent<Invector.vCharacterController.vCharacter>() != null)
        {
            other.transform.parent = transform;
        }
    }
    //プレイヤーが降りると子を外す
    void OnTriggerExit(Collider other)
    {
        if (other.transform.parent == transform && other.transform.CompareTag("Player"))
        {
            other.transform.parent = null;
            other.transform.eulerAngles = new Vector3(0, other.transform.eulerAngles.y, 0);
        }
    }
}
