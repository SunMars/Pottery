using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using Leap;
using System;

public class PotteryManager : MonoBehaviour
{
    public HandController leaphandController;
    public Lathe latheController;
    public HandController handController;
    public int ClayResolution;
    public float ClayHeight, ClayRadius, ClayVariance;
    public float effectStrength, affectedArea;
    public UIManager uiManager;
    public ToolModel[] toolModels;

    [Header("Debug")]
    public LineRenderer lineRenderer;
    public GameObject fingerTipSphere;
    public GameObject thumbTipSphere;

    private Spline spline;
    private Controller m_leapController;
    private Vector3 tipPosition;

    private TOOL currentTool;

    public bool DebugdrawLine;

    enum MODE
    {
        HANDMODE,
        TOOLMODE,
        HUDMODE,
        NONE
    }
    enum TOOL
    {
        PUSHTOOL1,
        PUSHTOOL2,
        PULLTOOL1,
        PULLTOOL2,
        SMOOTHTOOL1,
        SMOOTHTOOL2
    }
    enum GESTURE
    {
        PUSH1,
        PULL1,
        PULL2,
        SMOOTH1,
        NONE
    }
    enum LEAPHAND
    {
        INDEX,
        MIDDLE,
        PINKY,
        RING,
        THUMB,
        PALM
    }

    // Use this for initialization
    void Start()
    {
        DebugdrawLine = false;
        //m_leapController = new Controller();
        m_leapController = handController.GetLeapController();
        // initiate the Spline 
        spline = new Spline(ClayRadius, ClayHeight, ClayResolution, ClayVariance);

        // generate initial clay-object
        latheController.init(spline.getSplineList());
        currentTool = TOOL.PUSHTOOL1;

    }

    // Update is called once per frame
    void Update()
    {
        Frame frame = m_leapController.Frame();

        // Guess what the user wants to do
        switch (checkIntend(frame))
        {
            case MODE.HANDMODE:
                {
                    /*
                    // [Debug] moves white sphere to tip Position
                    fingerTipSphere.transform.position = tipPosition;
                    */

                    // get current gesture

                    var currentGesture = getCurrentGesture(frame.Hands);

                    switch (currentGesture)
                    {
                        case GESTURE.PUSH1:
                            {
                                Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                // Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.1f; };
                                // v-- uses the percentage number of the vertices
                                spline.PushAtPosition(tipPosition, spline.DistanceToMesh(tipPosition), effectStrength, affectedArea, currentDeformFunction);
                            }
                            break;

                        case GESTURE.PULL1: //pull with open hand - use height
                            {
                                // Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };
                                
                                Vector3 indexTipPosition = getScaledPosition(frame.Hands[0], LEAPHAND.INDEX); 
                                Vector3 thumbTipPosition = getScaledPosition(frame.Hands[0], LEAPHAND.THUMB);

                                float affectedHeight = Mathf.Abs(indexTipPosition.y - thumbTipPosition.y);

                                Vector3 center = (indexTipPosition + thumbTipPosition) / 2f;
                                spline.PullAtPosition(center, effectStrength * 2f, affectedHeight, currentDeformFunction, Spline.UseAbsolutegeHeight);
                            }
                            break;
                        case GESTURE.PULL2: // pull with pinch
                            {
                                // Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };

                                spline.PullAtPosition(tipPosition, effectStrength * 2f, affectedArea, currentDeformFunction);
                            }
                            break;
                        case GESTURE.SMOOTH1:
                            {
                                Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 1f; };
                                // spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea*0.5f, currentDeformFunction);
                                spline.SmoothArea(tipPosition, 0.2f, affectedArea * 0.3f, 8f, currentDeformFunction);
                            }
                            break;
                        case GESTURE.NONE:
                            break;
                        default:
                            {
                                Debug.Log("no gesture defined for " + currentGesture);
                            }
                            break;
                    }
                    //}
                }
                break;

            case MODE.TOOLMODE:
                {
                    //Get TipPosition of the Tool
                    Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    tipPosition *= handController.transform.localScale.x; //scale position with hand movement
                    tipPosition += handController.transform.position;

                    // [Debug] moves white sphere to tip Position
                    //fingerTipSphere.transform.position = tipPosition;
                    //thumbTipSphere.transform.position = new Vector3(0f,0f,0f);

                    float splineDistToPoint = spline.DistanceToMesh(tipPosition);

                    if (splineDistToPoint <= 0)
                    {
                        switch (currentTool)
                        {
                            case TOOL.PUSHTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 0.8f); };
                                    spline.PushAtPosition(tipPosition, splineDistToPoint*0.75f, effectStrength, affectedArea / 2, currentDeformFunction, true);
                                }
                                break;
                            case TOOL.PULLTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 1f); };
                                    spline.PullAtPosition(tipPosition, effectStrength*1f, affectedArea, currentDeformFunction, true);
                                }
                                break;
                            case TOOL.SMOOTHTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input); };
                                    //reduced affected area
                                    //spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea * 0.2f, currentDeformFunction);
                                    spline.SmoothArea(tipPosition, 0.5f, affectedArea * 0.3f, 4f, currentDeformFunction);

                                }
                                break;
                            default:
                                {
                                    Debug.Log("undefined tool: " + currentTool);
                                }
                                break;
                        }
                    }
                }
                break;
            case MODE.HUDMODE:
                //TODO
                break;
            default:

                //Debug Spheres
                //fingerTipSphere.transform.position = new Vector3(0f,0f,0f);
                //thumbTipSphere.transform.position = new Vector3(0f, 0f, 0f);
                return; // prevents recalculation of Spline (Modus.None)
        }

        //get List of deformed Spline
        List<Vector3> currentSpline = spline.getSplineList();

        //generate new Mesh
        latheController.updateMesh(currentSpline);

        if (DebugdrawLine)
        { //redraw debug line
            Vector3[] tmp = spline.getSpline();

            for (int i = 0; i < tmp.Length; i++)
            {

                lineRenderer.SetPosition(i, tmp[i]);
            }

            DebugdrawLine = false;
        }
    }

    private MODE checkIntend(Frame frame)
    {
        // if palm faces away from Object -> HudMode
        // if Tool visible -> ToolMode
        if (frame.Tools.Count > 0)
        {
            //Debug.Log("checkIntend has detected a Tool!");
            return MODE.TOOLMODE; // TODO English please
        }
        // if Hand visible -> HandMode
        if (frame.Hands.Count > 0)
        {
            return MODE.HANDMODE;
        }
        else // User does not interact with Spline
        {
            return MODE.NONE;
        }
    }


    /// <summary>
    /// Checks current Hand gesture
    /// </summary>
    /// <param name="hand"></param>
    /// <returns>current Gesture: Pull,Push or Smooth</returns>
    private GESTURE getCurrentGesture(HandList hands)
    {
        if (hands[0].Confidence < 0.2f) {
            Debug.Log("Confidence" + hands[0].Confidence);
            //TODO ignore gesture
        }
            

        // get closest finger
        Vector3 closestFinger = getNearestFinger(hands[0]);
        Vector3 thumbPosition = getScaledPosition(hands[0], LEAPHAND.THUMB);
        Vector3 palmPosition = getScaledPosition(hands[0], LEAPHAND.PALM);

        // is palm in obj & finger-palm-center angle ~90 degree?=>smooth
        Vector3 v1 = palmPosition - closestFinger;
        Vector3 v2 = palmPosition - new Vector3(0, palmPosition.y, 0);
        float dotValue = Vector3.Dot(v1.normalized, v2.normalized);
        if (dotValue > 1.0f)
            dotValue = 1.0f;
        else if (dotValue < -1.0f)
            dotValue = -1.0f;

        GESTURE recognizedGesture;

        if (dotValue < 0.3f && dotValue > -0.3f && spline.DistanceToMesh(palmPosition) <= 0.05f)
        {
            tipPosition = palmPosition;
            Debug.Log("dotValue:\t" + dotValue + "\tsmoothing");
            recognizedGesture = GESTURE.SMOOTH1;
        }
        else if ((spline.DistanceToMesh(thumbPosition) <= 0f) && (spline.DistanceToMesh(getNearestFinger(hands[0], true)) <= 0f))
        {         // is thumb in obj? & closest in obj => pull
            tipPosition = getNearestFinger(hands[0], true);
            Debug.Log(thumbPosition + " | " + getNearestFinger(hands[0], true));
            recognizedGesture = GESTURE.PULL1;
        }
        else if (spline.DistanceToMesh(closestFinger) < 0f)
        {        // if closest in obj=> push
            tipPosition = getNearestFinger(hands[0]);
            Debug.Log("push: "+ spline.DistanceToMesh(closestFinger));
            recognizedGesture = GESTURE.PUSH1;
        }
        else
        {
            recognizedGesture = GESTURE.NONE;
        }

        //Debug.Log(recognizedGesture);
        return recognizedGesture;
        
    }

    private Vector3 getNearestFinger(Hand hand, bool ignoreThumb = false)
    {
        int index = 0;
        Vector3 closestFinger = Vector3.zero;

        for (int i = 0; i < hand.Fingers.Count; i++)
        {
            
            Vector3 tmp = handController.transform.localScale.x * hand.Fingers[i].TipPosition.ToUnityScaled(false);
            tmp += handController.transform.position;
            
            if (ignoreThumb)
            {
                // if thumb => jump to next finger
                if (hand.Fingers[i].Type == Finger.FingerType.TYPE_THUMB)
                    continue; // next Finger
            }

            if (closestFinger == Vector3.zero) 
            {
                index = i;
                closestFinger = tmp;
            }
            else if (spline.DistanceToMesh(closestFinger) > spline.DistanceToMesh(tmp))
            {
                index = i;
                closestFinger = tmp;
            }
        }
        //Debug.Log("closest finger: " + hand.Fingers[index].Type);
        return closestFinger;
    }

    public void setPushTool()
    {
        currentTool = TOOL.PUSHTOOL1;
        handController.toolModel = toolModels[0];
        Debug.Log("Push Tool Selected");
        handController.destroyCurrentTools();
    }

    public void setPullTool()
    {
        currentTool = TOOL.PULLTOOL1;
        handController.toolModel = toolModels[1];
        Debug.Log("Pull Tool Selected");
        handController.destroyCurrentTools();
    }

    public void setSmoothingTool()
    {
        currentTool = TOOL.SMOOTHTOOL1;
        handController.toolModel = toolModels[2];
        Debug.Log("Smoothing Tool Selected");
        handController.destroyCurrentTools();
    }

    public void setDebugBool()
    {
        this.DebugdrawLine = true;
    }

    internal void resetAll()
    {
        // initiate the spline 
        spline = new Spline(ClayRadius, ClayHeight, ClayResolution, ClayVariance);

        // generate initial clay-object
        latheController.updateMesh(spline.getSplineList());
    }

    internal Spline getSpline()
    {
        return spline;
    }

    private Vector3 getScaledPosition(Hand hand, LEAPHAND type)
    {
        Vector3 retVal;
        // 3 steps:
        // 1 get Leap-Coords
        switch (type)
        {
            case LEAPHAND.INDEX:
                retVal = hand.Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
                break;
            case LEAPHAND.MIDDLE:
                retVal = hand.Fingers.FingerType(Finger.FingerType.TYPE_MIDDLE)[0].TipPosition.ToUnityScaled(false);

                break;
            case LEAPHAND.RING:
                retVal = hand.Fingers.FingerType(Finger.FingerType.TYPE_RING)[0].TipPosition.ToUnityScaled(false);

                break;
            case LEAPHAND.PINKY:
                retVal = hand.Fingers.FingerType(Finger.FingerType.TYPE_PINKY)[0].TipPosition.ToUnityScaled(false);

                break;
            case LEAPHAND.THUMB:
                retVal = hand.Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);

                break;
            case LEAPHAND.PALM:
                retVal = hand.PalmPosition.ToUnityScaled(false);
                break;
            default:
                retVal = hand.Fingers.Frontmost.TipPosition.ToUnityScaled(false);
                break;
        }

        // 2 scale with handControler
        retVal *= handController.transform.localScale.x;
        
        // 3 offset with handcontroller
        retVal += handController.transform.position;

        return retVal;
    }
}
