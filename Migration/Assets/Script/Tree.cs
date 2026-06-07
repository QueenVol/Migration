using UnityEngine;

public class Tree : MonoBehaviour
{
    public float age;

    public bool mature;

    private void Start()
    {
        age = 0;
        mature = false;
    }

    private void Update()
    {
        age += Time.deltaTime;

        if (age > 60f)
        {
            mature = true;
        }
    }
}
