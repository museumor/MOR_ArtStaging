
using System;
#if UNITY_EDITOR
using UnityEngine;
#endif

namespace MOR.Industries
{
    [SelectionBase]
    public class Artwork : MonoBehaviour {
        public string ArtworkNameTag;

        public Vector3[] visibilityPoints;

        //don't load/unload only enable/disable. Good for things that hickup on load/unload
        public bool onlyEnableDisable = false;
        public bool drawVisibilityPoints = true;
        [HideInInspector] [NonSerialized] public int numCountedVertices = -1;

        public string ArtworkAssetBundle;
        public bool Addressable;
        public Transform dropZone;

        [Tooltip("If the artwork is visible in the main dimension AND in another dimension, add that room here")]
        public int isAlsoInRoom;

        public bool lowPriorityLoad = false;
        public bool free = false;
        public bool featured = false;
        [Header("Android")] public float disableByDistance = -1;
        private bool? enabledOverride = null;


        public virtual bool activeState {
            get { return gameObject.activeSelf; }
        }
        

        protected Bounds GetBounds(){
            Bounds bounds = new Bounds();
            bool inited = false;
            Renderer[] renderers = GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers) {
                Bounds innerBounds = renderer.bounds;
                if (innerBounds.extents == Vector3.zero) {
                    continue;
                }

                if (!inited) {
                    bounds = innerBounds;
                    inited = true;
                }
                else {
                    bounds.Encapsulate(innerBounds);
                }
            }

            return bounds;
        }

    }



}
