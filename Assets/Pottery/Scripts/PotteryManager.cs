using UnityEngine;
using System.Collections;
using Leap;


public class PotteryManager : MonoBehaviour {

    [Header("Debug")]
    public LineRenderer lineRenderer;
    public GameObject fingerTipSphere;

    private Spline spline;
    private Frame frame;
    private Controller leapController;
    private LeapRecorder recorder;
    private bool enableRecordPlayback = false;

    /** Creates a new Leap Controller object. */
    void Awake()
    {
        leapController = new Controller();
        recorder = new LeapRecorder();
    }

    // Use this for initialization
    void Start () {

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
        frame = GetFrame();
        Hand hand = frame.Hand(0);
        PointableList pointables = hand.Pointables;
        FingerList fingers = hand.Fingers;
        Vector point = fingers[1].TipPosition;
        Debug.Log("point is: "+ point);
        Vector3 fingertip = new Vector3(point.x, point.y, point.z);
        Debug.Log("fingertip is: " + fingertip);

        fingerTipSphere.transform.position = fingertip;
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
