using UnityEngine;

[SelectionBase]
public class Springy : MonoBehaviour
{
    public Transform springTarget;
    public Transform springObj;

    [Space(12)]
    public float drag = 2.5f;//drag
    public float springForce = 80.0f;//Spring
    
	[Space(12)]
    public Transform GeoParent;

    Rigidbody SpringRB;
    private Vector3 LocalDistance;//Distance between the two points
    private Vector3 LocalVelocity;//Velocity converted to local space

    void Start()
    {
        SpringRB = springObj.GetComponent<Rigidbody>();//Find the RigidBody component
        springObj.transform.parent = null;//Take the spring out of the hierarchy
    }

    void FixedUpdate()
    {
        //Sync the rotation 
        SpringRB.transform.rotation = this.transform.rotation;

        //Calculate the distance between the two points
        LocalDistance = springTarget.InverseTransformDirection(springTarget.position - springObj.position);
        SpringRB.AddRelativeForce((LocalDistance) * springForce);//Apply Spring

        //Calculate the local velocity of the springObj point
        LocalVelocity = (springObj.InverseTransformDirection(SpringRB.velocity));
        SpringRB.AddRelativeForce((-LocalVelocity) * drag);//Apply drag

        //Aim the visible geo at the spring target
        GeoParent.transform.LookAt(springObj.position, new Vector3(0, 0, 1));
    }

    private void OnDestroy()
    {
        //exit playmode gave a weird error with Destroying the runtime OBJ
        if (!Application.isPlaying) return;

        if (springObj != null)
        {
            Destroy(springObj.gameObject);
        }
    }
}
