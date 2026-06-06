using UnityEngine;

public class NetPositionInterpolator : MonoBehaviour
{
    private Vector3 targetPos;
    private const float SPEED = 20f;

    void Awake()
    {
        targetPos = transform.position;
    }

    public void SetTarget(Vector3 pos)
    {
        targetPos = pos;
    }

    void Update()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, SPEED * Time.deltaTime);
    }
}
