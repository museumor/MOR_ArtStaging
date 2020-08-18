    
    
using System.Collections.Generic;
using UnityEngine;

/*
 *
 * Manages the location of the local player
 * 
 */

namespace MOR.Industries
{
    public class MORPlayer : Singleton<MORPlayer>
    {
        public GameObject head;
        public Collider headCollider;
        public Collider face;
        public Collider leftEarCollider;
        public Collider rightEarCollider;
        //public Collider vision;
        //public Transform visionStart;

        public Transform audioListener;
        public MORHand[] morHands; // We're going to want to swap over to these for the teleport stuff... and everything else
        
        
        private Vector3? initialPosition = null;
        public Vector3 InitialPosition => initialPosition ?? transform.position;

        private Quaternion? initialRotation = null;
        private static readonly int PlayerScale = Shader.PropertyToID("_PlayerScale");

        public Quaternion InitialRotation => initialRotation ?? trackingOriginRotator.rotation;

        public Transform trackingOriginTransform { get { return transform; } }

        public Transform trackingOriginRotator;
        public bool IsScaledDown => trackingOriginTransform.localScale.x < 1;

        public float Scale
        {
            get
            {
                return trackingOriginTransform.localScale.x;
            }
        }

        public Vector3 Up { get; set; } = Vector3.up;

        public bool Initialized { get; private set; } = false;

        private void Start()
        {
            if (audioListener)
            {
                audioListener.transform.parent = head.transform;
                audioListener.transform.localPosition = Vector3.zero;
                audioListener.transform.localRotation = Quaternion.identity;
            }

            initialPosition = transform.position;
            initialRotation = trackingOriginRotator.rotation;

            Shader.SetGlobalFloat(PlayerScale, 1f);

            Initialized = true;
        }

        public Vector3 feetPositionGuess
        {
            get
            {
                Transform hmd = head.transform;
                if (hmd)
                {
                    Vector3 headLocal = hmd.localPosition;//position of head localspace.
                    headLocal.y = 0;//move down to playerspace 0 on y avis.
                    return trackingOriginRotator.TransformPoint(headLocal);//convert to world position
                                                                           //return trackingOriginTransform.position + Vector3.ProjectOnPlane(hmd.position - trackingOriginTransform.position, trackingOriginTransform.up);
                }
                return trackingOriginTransform.position;
            }
        }


        //-------------------------------------------------
        public MORHand leftHand
        {
            get
            {
                for (int j = 0; j < morHands.Length; j++)
                {
                    if (!morHands[j].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (morHands[j].handedness == MORHand.Handedness.Right)
                    {
                        continue;
                    }

                    return morHands[j];
                }

                return null;
            }
        }


        //-------------------------------------------------
        public MORHand rightHand
        {
            get
            {
                for (int j = 0; j < morHands.Length; j++)
                {
                    if (!morHands[j].gameObject.activeInHierarchy)
                    {
                        continue;
                    }

                    if (morHands[j].handedness == MORHand.Handedness.Left)
                    {
                        continue;
                    }

                    return morHands[j];
                }

                return null;
            }
        }
        

    }
}
