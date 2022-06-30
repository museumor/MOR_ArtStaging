using System.Collections.Generic;
using System.Net;
using UnityEngine;


namespace MOR.Industries
{

    public abstract class VideoPlayerBase : MonoBehaviour
    {

        [Tooltip("Relative to Streaming Assets or Streaming URL. If this is not set then the settings on the player will be used")]
        public string videoPathOrUrl;
        public string audioPathOrUrl;
        public bool loop = false;
        public Transform screen;
        public Transform[] extraScreens;

        [SerializeField]
        public AudioSource audioSource;
        public float onDistance = 1;
        public float offDistance = 3;
      
        //public float screenFadeTime = 4;
        //public AnimationCurve screenFadeCurve = AnimationCurve.Linear(0,0,1,1);
        //public float visitorProximityShutoffDelay = .2f;
        public bool disableMultiPlayer = false;
        //public bool manualStart = false;
        
        private Ray centreRay;
        private float volume = 1;
        private bool mute = false;
        protected float closestVisitorDistSqr;
        protected bool isOneVisitorClosestToThisPlayer;
        protected float distToVisitorClosestToThisPlayerSqr;
        //we can't currently support pause and sync video over the network
        //protected bool isPaused = false;
        //private float pauseTime;

       
        //Do we want to play, do we think we are playing
        public bool WantsToPlay { get; private set; } = false;
        public float StartTime { get; private set; }
        public AudioSource TargetAudioSource => audioSource;
        protected float PlayerDistance => DistanceToLine(centreRay, Globals.playerHead.position);
        private float shouldStopPlayingBecauseOfVisitorPositionCount = 0;
        //will probably return zero if the video isn't loaded already
        private Coroutine startPlaybackCo;
        private HttpWebRequest request;
        
        public static float DistanceToLine(Ray ray, Vector3 point)
        {
            return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
        }
        public double VideoLength {
            get {
              
                return -1;
            }
        }
        
        public double CurrentVideoTime {
            get {
              
                return 0;
            }
        }
        public float Volume {
            get => volume;
            set {
                volume = value;
            }
        }

        public bool Mute {
            get => mute;
            set {
                mute = value;
            }
        }

        public virtual void Start() {
            centreRay = new Ray(transform.position, Vector3.up);

        }

        protected virtual void Awake() {
            if (videoPathOrUrl.StartsWith("https://player.vimeo.com")) {
                SetVimeoVideoPath(videoPathOrUrl);
            }else {
                SetVideoPath(videoPathOrUrl);
            }
        }

        protected void OnEnable() {
           // VideoManager.instance.AddVideoPlayer(this);
        }

        public void SetVideoPath(string path, string audioPath = "") {
            videoPathOrUrl = path;
            audioPathOrUrl = audioPath;
            //useMediaReference = false;
        }

        private void SetVimeoVideoPath(string url) {

            request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.AllowAutoRedirect = false;
            request.Credentials = CredentialCache.DefaultCredentials;

            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            var redirectURL = response.GetResponseHeader("Location");
            Debug.Log(redirectURL);
            response.Close();
            SetVideoPath(redirectURL);
        }
        
        
    }
}