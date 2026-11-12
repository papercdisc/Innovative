using UnityEngine;

public class VFXPlayer : MonoBehaviour
{
    public GameObject vfxPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject vfx = Instantiate(vfxPrefab, transform.position, transform.rotation, this.gameObject.transform);

        Destroy(gameObject, 5f); // destroy this object after 5 seconds
    }
}
