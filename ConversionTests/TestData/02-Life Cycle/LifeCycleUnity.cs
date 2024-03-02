using UnityEngine;

public class HelloWorld : MonoBehaviour
{
    void Awake()
    {
        Debug.Log("I'm just been accessed!");
    }

    void Start()
    {
        Debug.Log("I'm ready!");
    }

    void Update()
    {
        Debug.Log("I'm doing work!");
    }

}