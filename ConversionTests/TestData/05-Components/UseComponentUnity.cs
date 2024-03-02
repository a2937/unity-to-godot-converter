using UnityEngine;

public class UseComponent : MonoBehaviour
{
    private SpriteRenderer sr;
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        sr.enabled = true;
    }
}