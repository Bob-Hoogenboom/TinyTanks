/// <summary>
/// Apply to any script to apply health and easy acces to a damage class
/// </summary>
public interface IDamagable
{
    float HitPoints { get; }
    void Damage(float damage);
}
