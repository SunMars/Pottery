using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Leap;
using System;

public class PotteryManager : MonoBehaviour {
    public HandController leaphandController;
    public Lathe latheController;
    [Header("Debug")]
    public LineRenderer lineRenderer;
    public GameObject fingerTipSphere;
    public int ClayResolution;
    public float ClayHeight, ClayRadius, ClayVariance;

    private Spline spline;
    private Controller leapController;
    private LeapRecorder recorder;
    private bool enableRecordPlayback = false;
    private Controller m_leapController;

    enum GESTURE
    {
        PUSH,
        PULL,
        SMOOTH
    }

    /** Creates a new Leap Controller object. */
    void Awake()
    {
        leapController = new Controller();
        recorder = new LeapRecorder();
    }

    // Use this for initialization
    void Start () {
        m_leapController = new Controller();
        // initiate the Debug Spline 
        spline = new Spline(ClayRadius, ClayHeight, ClayResolution, ClayVariance);
        //latheController = new Lathe(spline.getSplineList(), true);
        latheController.init(spline.getSplineList());
       
    }
	
	// Update is called once per frame
	void Update () {
        Frame frame = m_leapController.Frame();
        Hand hand = frame.Hand(0);

        // Debug.Log("PotteryManager:\tFrame:"+frame.Timestamp+","+
        //     "PalmPos:"+ leaphandController.transform.TransformPoint(frame.Hands[0].PalmPosition.ToUnityScaled(false)).ToString());

        //todo https://developer.leapmotion.com/documentation/csharp/devguide/Leap_Coordinate_Mapping.html

        //check if hands are found:
        if (frame.Hands.Count > 0) {
            // Check if Hand touches clay
            Vector3 test = frame.Hands[0].PalmPosition.ToUnityScaled(false);
           // test = leaphandController.transform.TransformPoint(hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false));
            Vector3 tipPosition = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
            tipPosition = test;
            float splineDistToPoint = spline.DistanceToPoint(tipPosition);
            Debug.Log("PotteryManager:\t tipPosition: " + tipPosition.ToString());
            Debug.Log("PotteryManager:\tDistance of spline to hand: "+ splineDistToPoint);
            if (splineDistToPoint <= 0)
            {
                // get current gesture
                switch (getCurrentGesture(hand))
                {
                    case GESTURE.PUSH:
                        {
                            //spline.PushAtPosition(fingers.Frontmost.StabilizedTipPosition.ToUnity(false), splineDistToPoint);
                            spline.PushAtPosition(tipPosition, splineDistToPoint);
                        }
                        break;

                    case GESTURE.PULL:
                        {
                            spline.PullAtPosition(tipPosition);
                        }
                        break;
                    case GESTURE.SMOOTH:
                        {
                            spline.SmoothAtPosition(tipPosition);
                        }
                        break;
                    default:
                        break;
                }

                List<Vector3> currentSpline = spline.getSplineList();
                //todo spline neu rendern
                latheController.updateMesh(currentSpline);

            }
        }
    }

    private GESTURE getCurrentGesture(Hand hand)
    {
        //todo
        // was macht hand.PinchStrength?
        return GESTURE.PUSH;
    }

    /**
  * Returns the latest frame object.
  *
  * If the recorder object is playing a recording, then the frame is taken from the recording.
  * Otherwise, the frame comes from the Leap Motion Controller itself.
  */
    public virtual Frame GetFrame()
    {
        if (enableRecordPlayback && (recorder.state == RecorderState.Playing || recorder.state == RecorderState.Paused))
            return recorder.GetCurrentFrame();

        return leapController.Frame();
    }
}
