using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class Target : MonoBehaviour, IDamagable
{
    [SerializeField] private float hitPoints = 1f;
    public float HitPoints { get { return hitPoints; } set { hitPoints = value; } }

    [Space]
    public UnityEvent OnHit;
    public UnityEvent OnDefeat;

    private bool _isDefeated = false;

    public void Damage(float damage)
    {
        hitPoints -= damage;

        OnHit.Invoke();

        _isDefeated = hitPoints <= 0 ? true : false;

        if (!_isDefeated) return;


        OnDefeat.Invoke();
    }
}
