using TMPro;
using UnityEngine;

public class FontFixer : MonoBehaviour
{
    public TMP_FontAsset brokenFont;
    public Material fallbackMaterial;

    void Start()
    {
        if (brokenFont != null && fallbackMaterial != null)
        {
            brokenFont.material = fallbackMaterial;
            Debug.Log("Font material reassigned.");
        }
    }
}