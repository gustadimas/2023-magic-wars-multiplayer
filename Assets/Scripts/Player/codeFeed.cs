using UnityEngine;

public class codeFeed : MonoBehaviour
{
    [SerializeField] float destroyTime = 4f;

    void OnEnable()
    {
        Destroy(gameObject, destroyTime);
    }
}
