using UnityEngine;

public class RainScroll : MonoBehaviour
{
    [SerializeField] private float speed = 2f;//雨下落速度
    [SerializeField] private float cycleHeight = 32f; //雨下落一个周期的高度

    private float startY;

    void Start()
    {
        startY = transform.position.y;
    }

    void Update()
    {
        transform.position += Vector3.down * speed * Time.deltaTime;

        // 下落满一个周期就跳回原位
        if (transform.position.y < startY - cycleHeight)
        {
            transform.position = new Vector3(transform.position.x, startY, transform.position.z);
        }
    }
}
