using System;
using UnityEngine;
using UnityEngine.XR;

namespace MOR.Industries
{
    // Represents a local controller.  
    public class MORHand : MonoBehaviour
    {
        
        public enum HandTypes { Gestures, Bones }

       public enum Handedness
        {
            Right,
            Left
        }
       
        [Serializable]
        public struct ControllerAttachmentPoint
        {
            public MultiPlatform.ControllerTypes controllerType;
            public Transform attachmentPoint;
        }

        public bool DeviceFound => deviceFound;
        

        bool deviceFound = false;

        private string controllerName = "";
        public string ControllerName
        {
            get
            {
                return controllerName;
            }
        }

        private const string LEFT_HAND = "Left";
        private const string RIGHT_HAND = "Right";
        private const string VIVE = "vive";
        private const string OPEN_VR = "openvr";
        private const string KNUCKLES = "knuckles";
        private const string COSMOS = "cosmos";
        private const string OCULUS = "oculus";
        private const string TOUCH = "cv1";
        private const string RIFTS = "rift s";
        private const string QUEST = "quest";


        public bool IsTracking { get; private set; } = false;

        public MultiPlatform.ControllerTypes ControllerType { get; private set; } = MultiPlatform.ControllerTypes.Unknown;


        public MORHand otherHand;
        public Handedness handedness = Handedness.Right;
        private InputDevice device; // The device associated with the current hand (used for haptics)

    }
}


