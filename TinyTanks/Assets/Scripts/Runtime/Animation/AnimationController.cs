using UnityEngine;

namespace Animation
{
    /// <summary>
    /// Simple animation controller script to test animations
    /// </summary>
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private Animator anim;
        [Tooltip("This is a multiplier. it does the default animator speed and multiplies that by animSpeed. \n(animator.speed * animSpeed)")]
        [SerializeField] private float animSpeed = 1f;

        private int _leftTrackAnim = Animator.StringToHash("LeftTrack");
        private int _rightTrackAnim = Animator.StringToHash("RightTrack");


        private void Update()
        {
            //-1 makes the track go backwards
            //1 makes it go forwards 
            //0 is idle

            anim.speed = animSpeed;

            if (Input.GetKey(KeyCode.W))        // ^
            {
                anim.SetFloat(_leftTrackAnim, 1);
                anim.SetFloat(_rightTrackAnim, 1);
            }
            else if (Input.GetKey(KeyCode.S))   // v
            {
                anim.SetFloat(_leftTrackAnim, -1);
                anim.SetFloat(_rightTrackAnim, -1);
            }
            else if (Input.GetKey(KeyCode.A))   // < 
            {
                anim.SetFloat(_leftTrackAnim, -1);
                anim.SetFloat(_rightTrackAnim, 1);
            }
            else if (Input.GetKey(KeyCode.D))   // >
            {
                anim.SetFloat(_leftTrackAnim, 1);
                anim.SetFloat(_rightTrackAnim, -1);
            }
            else
            {
                anim.SetFloat(_leftTrackAnim, 0);
                anim.SetFloat(_rightTrackAnim, 0);
            }
        }
    }
}