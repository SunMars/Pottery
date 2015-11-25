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

    public ToolModel[] toolModels;

    [Header("Debug")]
    public LineRenderer lineRenderer;
    public GameObject fingerTipSphere;

    private Spline spline;
    private Controller m_leapController;

    
    private TOOL currentTool;

    enum MODUS
    {
        HANDMODUS,
        TOOLMODUS,
        HUDMODUS,
        NONE
    }
    enum TOOL
    {
        PUSHTOOL,
        PUSHTOOL2,
        PULLTOOL,
        PULLTOOL2,
        SMOOTHTOOL,
        SMOOTHTOOL2
    }
    enum GESTURE
    {
        PUSH,
        PULL,
        PULL1,
        SMOOTH
    }

    // Use this for initialization
    void Start()
    {
        //m_leapController = new Controller();
        m_leapController = handController.GetLeapController();
        // initiate the Spline 
        spline = new Spline(ClayRadius, ClayHeight, ClayResolution, ClayVariance);

        // generate initial clay-object
        latheController.init(spline.getSplineList());
        currentTool = TOOL.PUSHTOOL;

    }

    // Update is called once per frame
    void Update()
    {
        getInput();
        Frame frame = m_leapController.Frame();

        // Guess what the user wants to do
        switch (checkIntend(frame))
        {
            case MODUS.HANDMODUS:
                {
                    // Check if Hand touches clay
                    // todo: getclosest finger
                    Vector3 tipPosition = frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
                    tipPosition *= handController.transform.localScale.x; //scale position with Handmovement
                    tipPosition += handController.transform.position; 

                    // [Debug] moves white sphere to tip Position
                    fingerTipSphere.transform.position = tipPosition;

                    float splineDistToPoint = spline.DistanceToPoint(tipPosition);

                    if (splineDistToPoint <= 0)
                    {
                        // get current gesture
                        switch (getCurrentGesture(frame.Hands))
                        {
                            case GESTURE.PUSH:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.1f; };
                                    //v-- benutzt prozentuale menge der vertices
                                    spline.PushAtPosition(tipPosition, splineDistToPoint, effectStrength, affectedArea, currentDeformFunction);
                                }
                                break;

                            case GESTURE.PULL: //pull with open hand - use height
                                {
                                    //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };
                                    
                                    Vector3 indexTipPosition = handController.transform.localScale.x * frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
                                    indexTipPosition += handController.transform.position;
                                    Vector3 thumbTipPosition = handController.transform.localScale.x * frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);
                                    thumbTipPosition += handController.transform.position;

                                    float affectedHeight = Mathf.Abs(indexTipPosition.y - thumbTipPosition.y);

                                    Vector3 center = (indexTipPosition + thumbTipPosition) / 2f;
                                    spline.PullAtPosition(center, effectStrength, affectedHeight, currentDeformFunction, Spline.UseAbsolutegeHeight);
                                }
                                break;
                            case GESTURE.PULL1: //pull with pinch
                                {
                                   //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };
                                    
                                    spline.PullAtPosition(tipPosition, effectStrength, affectedArea, currentDeformFunction);
                                }
                                break;
                            case GESTURE.SMOOTH:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.05f; };
                                    //spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea*0.5f, currentDeformFunction);
                                    spline.SmoothArea(tipPosition, effectStrength, affectedArea * 0.8f, currentDeformFunction);
                                }
                                break;
                            default:
                                break;
                        }
                    }
                }
                break;

            case MODUS.TOOLMODUS:
                {
                    //Get TipPosition of the Tool
                    Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    tipPosition *= handController.transform.localScale.x; //scale position with Handmovement
                    tipPosition += handController.transform.position; 

                    // [Debug] moves white sphere to tip Position
                    fingerTipSphere.transform.position = tipPosition;

                    float splineDistToPoint = spline.DistanceToPoint(tipPosition);

                    if (splineDistToPoint <= 0)
                    {
                        switch (currentTool)
                        {
                            case TOOL.PUSHTOOL:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Sin(input), 2f); };
                                    spline.PushAtPosition(tipPosition, splineDistToPoint, effectStrength, affectedArea*2, currentDeformFunction);
                                }
                                break;
                            case TOOL.PULLTOOL:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Sin(input), 2f); };
                                    spline.PullAtPosition(tipPosition, effectStrength, affectedArea*2, currentDeformFunction);
                                }
                                break;
                            case TOOL.SMOOTHTOOL:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Sin(input); };
                                    //reduced affected area
                                    spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea * 0.25f, currentDeformFunction);
                                }
                                break;
                        }
                    }
                }
                break;
            case MODUS.HUDMODUS:
                //todo
                break;
            default:
                return; // prevents recalc of Spline (Modus.None)
        }

        //get List of deformed Spline
        List<Vector3> currentSpline = spline.getSplineList();

        //generate new Mesh
        latheController.updateMesh(currentSpline);
    }

    private MODUS checkIntend(Frame frame)
    {
        // if palm faces away from Objekt -> Hudmodus
        // if Tool visible -> toolmodus
        if (frame.Tools.Count > 0)
        {
            //Debug.Log("checkIntend has deteced a Tool!");
            return MODUS.TOOLMODUS;
        }
        // if Hand visible -> handmodus
        if (frame.Hands.Count > 0) {
            return MODUS.HANDMODUS;
        } else // User does not interact with Spline
        {
            return MODUS.NONE;
        }
    }


    /// <summary>
    /// Checks current Hand gesture
    /// </summary>
    /// <param name="hand"></param>
    /// <returns>current Gesture: Pull,Push or Smooth</returns>
    private GESTURE getCurrentGesture(HandList hands)
    {
        if(hands.Count > 1)
        {
            return GESTURE.SMOOTH;
        }
        //calculate pinch-angle
        Vector3 indexTipPosition = handController.transform.localScale.x * hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
        Vector3 thumbTipPosition = handController.transform.localScale.x * hands[0].Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);
        Vector3 palmPosition = handController.transform.localScale.x * hands[0].PalmPosition.ToUnityScaled(false);

        Vector3 v1 = palmPosition - indexTipPosition;
        Vector3 v2 = palmPosition - thumbTipPosition;
        v1.Normalize();
        v2.Normalize();
        float dotValue = Vector3.Dot(v1, v2);
        if (dotValue > 1.0f)
            dotValue = 1.0f;
        else if (dotValue < -1.0f)
            dotValue = -1.0f;

        //skalarprodukt
        //angle between thumb, indexfinger and Palm
        float angle = Mathf.Acos(dotValue / (v1.sqrMagnitude * v2.sqrMagnitude));

        //if angle is bigger than 1.1-> hand is pinching
        //1.1 is approximated value, possibly not best value for everyone

        if (angle > 1.1f) {
            return GESTURE.PULL;
        } else {
            if (hands[0].PinchStrength > 0.5f || hands[1].PinchStrength > 0.5f) {
                return GESTURE.PULL1;
            }
            else {
                return GESTURE.PUSH;
            }
        }
        
    }

    
    //Only for testing - switching tools with keys
    private void getInput()
    {
        if (Input.GetKey("1"))
        {
            currentTool = TOOL.PUSHTOOL;
            handController.toolModel = toolModels[0];
            Debug.Log("Push Tool Selected");
        }
        if (Input.GetKey("2"))
        {
            currentTool = TOOL.PULLTOOL;
            handController.toolModel = toolModels[1];
            Debug.Log("Pull Tool Selected");
        }
        if (Input.GetKey("3"))
        {
            currentTool = TOOL.SMOOTHTOOL;
            handController.toolModel = toolModels[2];
            Debug.Log("Smoothing Tool Selected");
        }
    }
}
