using UnityEngine;
using System.Collections;
using Leap;
using System;

public class PotteryManager : MonoBehaviour {
    public HandController leaphandController;
    [Header("Debug")]
    public LineRenderer lineRenderer;
    public GameObject fingerTipSphere;

    private Spline spline;
    private Frame frame;
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
        spline = new Spline(0.6f, 1.5f, 15);
        lineRenderer.SetVertexCount(spline.getSize());
        for(int i = 0; i <= spline.getSize()-1; i++)
        {
            lineRenderer.SetPosition(i, spline.getVertex(i));
        }
    }
	
	// Update is called once per frame
	void Update () {
        Frame frame = m_leapController.Frame();
        // Debug.Log("PotteryManager:\tFrame:"+frame.Timestamp+","+
        //     "PalmPos:"+ leaphandController.transform.TransformPoint(frame.Hands[0].PalmPosition.ToUnityScaled(false)).ToString());

        //todo https://developer.leapmotion.com/documentation/csharp/devguide/Leap_Coordinate_Mapping.html
        //frame = GetFrame();
        Hand hand = frame.Hand(0);
        PointableList pointables = hand.Pointables;
        FingerList fingers = hand.Fingers;
        Vector point = fingers[1].TipPosition;
        //Debug.Log("point is: "+ point);
        Vector3 fingertip = new Vector3(point.x, point.y, point.z);
        //Debug.Log("fingertip is: " + fingertip);

        fingerTipSphere.transform.position = fingertip;

        //check if hands are found:
        if (frame.Hands.Count > 0) {
            // Check if Hand touches clay
            Vector3 test = frame.Hands[0].PalmPosition.ToUnityScaled(false);
            test = leaphandController.transform.TransformPoint(test);
            float splineDistToPoint = spline.DistanceToPoint(test);
            //float splineDistToPoint = spline.DistanceToPoint(fingers.Frontmost.StabilizedTipPosition.ToUnity(false));
            Debug.Log("PotteryManager:\tDistance of spline to hand: "+ splineDistToPoint);
            if (splineDistToPoint <= 0)
            {
                // get current gesture
                switch (getCurrentGesture(hand))
                {
                    case GESTURE.PUSH:
                        {
                            //spline.PushAtPosition(fingers.Frontmost.StabilizedTipPosition.ToUnity(false), splineDistToPoint);
                            spline.PushAtPosition(test, splineDistToPoint);
                        }
                        break;

                    case GESTURE.PULL:
                        {

                        }
                        break;
                    case GESTURE.SMOOTH:
                        {

                        }
                        break;
                }
                
            
            }
        }
    }

    private GESTURE getCurrentGesture(Hand hand)
    {
        //todo
        // was amcht hand.PinchStrength?
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
