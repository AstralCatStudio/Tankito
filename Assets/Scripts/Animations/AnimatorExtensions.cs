using UnityEngine;

namespace Tankito.Utils
{
    public static class AnimatorExtensions
    {
        public static void ResetAllTriggers(this Animator animator)
        {
            //Debug.Log("Called Triggers Reset");
            
            foreach (var trigger in animator.parameters)
            {
                if (trigger.type == AnimatorControllerParameterType.Trigger)
                {
                    animator.ResetTrigger(trigger.name);
                }
            }
        }
    }
}