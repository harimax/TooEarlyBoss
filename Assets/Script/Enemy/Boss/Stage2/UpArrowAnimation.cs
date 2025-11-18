using UnityEngine;

public class UpArrowAnimation : MonoBehaviour
{
    private Vector3 initpos;
    private float time;
    private float duration = 1.0f;
    void Start()
    {
        initpos = transform.position;
    }
    void Update()
    {
        if(time<duration)
        {
            time += Time.deltaTime;
            transform.Translate(0f, 10f * Time.deltaTime, 0f);
        }
        else
        {
            transform.position = initpos;
            time = 0f;
        }
    }
}
