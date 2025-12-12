using UnityEngine;

public class Bullet : MonoBehaviour
{
    [Header("References")]
    public GameObject parent; // parent only checks if the bullet doesn't hit the tank that shot the bullet, weird but works for now

    [SerializeField] private GameObject explosionEffect;
    [SerializeField] private GameObject impactEffect;
    [SerializeField] private GameObject smokeEffect;

    [Header("Audio")]
    public AudioSource _bulletWhistle;
    public AudioSource _tankHitAudioSource;
    public AudioSource _enviormentHitAudioSource;

    [Header("Settings")]
    [SerializeField] private float damage = 1f;

    private void Start()
    {
        _bulletWhistle = GetComponentInChildren<AudioSource>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.root != parent)
        {
            IDamagable iDamage = other.GetComponent<IDamagable>();
            if (iDamage != null) iDamage.Damage(damage);


            if (other.gameObject.layer == 9)
            {
                Debug.Log("Player!: " + other.transform.root);

                //TODO Change instantiate and destroy logic to play and stop
                var vxf = Instantiate(explosionEffect, this.transform.position, this.transform.rotation);
                Destroy(vxf, 3);


                Debug.Log("Andere tank geraakt");
                _bulletWhistle.Stop();

                //TODO Change instantiate and destroy logic to play and stop
                var hitAudio = Instantiate(_tankHitAudioSource, this.transform.position, this.transform.rotation);
                Destroy(hitAudio.gameObject, 4);

                Destroy(gameObject);
            }
            else
            {
                if (other.gameObject.layer == 2) return;

                Debug.Log("Geen andere tank gehit");
                //TODO Change instantiate and destroy logic to play and stop
                var vxf = Instantiate(impactEffect, this.transform.position, this.transform.rotation);
                Destroy(vxf, 3);

                //TODO Change instantiate and destroy logic to play and stop
                var hitAudio = Instantiate(_enviormentHitAudioSource, this.transform.position, this.transform.rotation);
                Destroy(hitAudio.gameObject, 4);

                Rigidbody rb = GetComponent<Rigidbody>(); // refine this so the bullet actually goes on the ground
                Collider col = GetComponent<Collider>();
                col.isTrigger = false;
                rb.useGravity = true;
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
                _bulletWhistle.Stop();

                Debug.Log("Nothing: " + other.name);

                Destroy(this);
            }
        }
    }
}
