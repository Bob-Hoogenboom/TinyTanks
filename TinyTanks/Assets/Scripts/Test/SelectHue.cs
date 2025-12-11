using UnityEngine;

public class SelectHue : MonoBehaviour
{
    [Range(0, 1)]
    public float hueValue;

    private MaterialPropertyBlock _mpb;

    private void Start()
    {
        ApplyHue();
    }

    public void ApplyHue()
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Hue", hueValue);
            r.SetPropertyBlock(_mpb);
        }
    }

    public void RandomHue()
    {
        if (_mpb == null)
            _mpb = new MaterialPropertyBlock();

        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        float random = UnityEngine.Random.Range(0f, 1f);
        hueValue = random;

        foreach (Renderer r in renderers)
        {
            r.GetPropertyBlock(_mpb);
            _mpb.SetFloat("_Hue", hueValue);
            r.SetPropertyBlock(_mpb);
        }
    }
}