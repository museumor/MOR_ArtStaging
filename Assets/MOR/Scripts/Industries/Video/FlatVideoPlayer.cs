using UnityEngine;

namespace MOR.Industries
{
    public class FlatVideoPlayer : VideoPlayerBase
    {
        [Tooltip("Object Enabled when not playing video. Disables on play")]       
        public GameObject notPlaying;
        public bool playInFrontOnly = true;


        protected  void Update() {
            //show the videoscreen if playing
            //if (screen != null && screen.gameObject.activeSelf != IsPlaying) {
            //    screen.gameObject.SetActive(IsPlaying);
            //}
            //hide the 'off' screen
            //if (notPlaying != null && notPlaying.activeSelf == IsPlaying) {
             //   notPlaying.gameObject.SetActive(!IsPlaying);
            //}
        
            
            // If we're in front of the player and close enough start it.
            // note, we don't just use isOneVisitorClosestToThisPlayer and closestVisitorDistSqr because we want only
            // one visitor to start the player. The networking will make the players play for all the other players
            if (!WantsToPlay && isOneVisitorClosestToThisPlayer && closestVisitorDistSqr < onDistance * onDistance) {
                Vector3 vecToPlayer = transform.position - Globals.playerHead.position;
                float distToVisitorSqr = vecToPlayer.sqrMagnitude;
                bool isClose = distToVisitorSqr < onDistance * onDistance;
                bool isInFront = !playInFrontOnly || Vector3.Dot(vecToPlayer, transform.forward) > 0;
                bool isUnder = vecToPlayer.y > 3.5f;
                if (isClose && isInFront && isUnder == false) {
                    //StartPlayback();
                }
            }
        }

        protected  bool ShouldStopPlayingBecauseOfVisitorPosition() {
            //if (base.ShouldStopPlayingBecauseOfVisitorPosition()) {
            //    return true;
            //}
            // we DO just use isOneVisitorClosestToThisPlayer and closestVisitorDistSqr for turning it off because
            // the networking doesn't turn videos _off_. It only turns them on.
            return WantsToPlay && (!isOneVisitorClosestToThisPlayer || distToVisitorClosestToThisPlayerSqr > offDistance * offDistance);
        }

        //simpler turn off logic if we have disabled the multi-player for this video player.
        protected  bool ShouldStopPlayingBecauseOfVisitorPositionNoMultiPlayer() {
            //if (base.ShouldStopPlayingBecauseOfVisitorPosition()) {
            //    return true;
            //}
            float sqrDist = (transform.position - Globals.playerHead.position).sqrMagnitude;
            return WantsToPlay && sqrDist > offDistance * offDistance;
        }

        public  void StopPlayback()
        {
            
            if(notPlaying != null){
                notPlaying.gameObject.SetActive(true);
            }
            //base.StopPlayback();
        }

        private void OnDrawGizmosSelected() {
            Gizmos.color = Color.cyan;
            DrawGizmoCircle(transform.position, onDistance);
            Gizmos.color = Color.blue;
            DrawGizmoCircle(transform.position, offDistance);
        }
        
        public void DrawGizmoCircle(Vector3 position, float radius)
        {
            float theta = 0;
            float x = radius * Mathf.Cos(theta);
            float y = radius * Mathf.Sin(theta);
            Vector3 pos = position + transform.rotation * (new Vector3(x, 0, y));
            Vector3 newPos = pos;
            Vector3 lastPos = pos;
            float endAngle = playInFrontOnly? Mathf.PI :  Mathf.PI * 2;
            for (theta = 0.0f; theta < endAngle; theta += 0.1f)
            {
                x = radius * Mathf.Cos(theta);
                y = radius * Mathf.Sin(theta);
                newPos = position + transform.rotation *(new Vector3(x, 0, y));
                Gizmos.DrawLine(pos, newPos);
                pos = newPos;
            }
            Gizmos.DrawLine(pos, lastPos);
        }
    }
}