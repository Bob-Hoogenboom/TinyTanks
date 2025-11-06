using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// Sprite button
/// Needs a sprite and collider to work
/// </summary>

[RequireComponent(typeof(Collider))]
public class RoleButton : MonoBehaviour
{
    public bool interactable = true;

    [Space]
    public SpriteRenderer targetGraphic;
    public Color normalColor = Color.white;
    public Color pressedColor = Color.gray;

    [Space]
    public KeyCode roleKey = KeyCode.None;
    public UnityEvent onClick;

    private bool _isPressed;

    private void Reset()
    {
        // Auto-assign if missing
        targetGraphic = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        if (targetGraphic != null)
            targetGraphic.color = normalColor;
    }

    private void OnMouseDown()
    {
        if (!interactable) return;
        PressButton();
    }

    private void OnMouseUp()
    {
        if (!interactable) return;
        ReleaseButton();
    }

    private void Update()
    {
        if (!interactable) return;

        if (Input.GetKeyDown(roleKey))
            PressButton();

        if (Input.GetKeyUp(roleKey))
            ReleaseButton();
    }

    private void PressButton()
    {
        if (_isPressed) return;
        _isPressed = true;

        if (targetGraphic != null)
            targetGraphic.color = pressedColor;

        onClick.Invoke();
    }

    private void ReleaseButton()
    {
        _isPressed = false;

        if (targetGraphic != null)
            targetGraphic.color = normalColor;
    }
}