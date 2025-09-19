using UnityEngine;

/// <summary>
/// Simple script used to scroll a texture to make the illusion of the texture rolling
/// This Script NEEDS a meshrenderer or the effect will not work
/// </summary>
public class ScrollingTexture : MonoBehaviour
{
    [SerializeField] private float scrollX;
    [SerializeField] private float scrollY;
    private MeshRenderer _meshRenderer;

    private void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
        if(_meshRenderer == null)
        {
            Debug.LogWarning("The object this script is attached to does not contain a meshRenderer");
        }
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        _meshRenderer.material.mainTextureOffset = new Vector2(Time.realtimeSinceStartup * scrollX, Time.realtimeSinceStartup * scrollY);
    }
}
