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

    private Spline spline;
    private Controller m_leapController;
    private Vector3 tipPosition;

    private TOOL currentTool;

    public bool DebugdrawLine;

    enum MODUS
    {
        HANDMODUS,
        TOOLMODUS,
        HUDMODUS,
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
            case MODUS.HANDMODUS:
                {
                    /*
                    // Check if Hand touches clay
                    // todo: getclosest finger
                    Vector3 tipPosition = frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
                    tipPosition *= handController.transform.localScale.x; //scale position with Handmovement
                    tipPosition += handController.transform.position; 

                    // [Debug] moves white sphere to tip Position
                    fingerTipSphere.transform.position = tipPosition;

                    float splineDistToPoint = spline.DistanceToPoint(tipPosition);

                    if (splineDistToPoint <= 0)
                    {*/
                        // get current gesture
                        switch (getCurrentGesture(frame.Hands))
                        {
                            case GESTURE.PUSH1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.1f; };
                                    //v-- benutzt prozentuale menge der vertices
                                    spline.PushAtPosition(tipPosition, spline.DistanceToPoint(tipPosition), effectStrength, affectedArea, currentDeformFunction);
                                }
                                break;

                            case GESTURE.PULL1: //pull with open hand - use height
                                {
                                    //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };
                                    
                                    Vector3 indexTipPosition = handController.transform.localScale.x * frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
                                    indexTipPosition += handController.transform.position;
                                    Vector3 thumbTipPosition = handController.transform.localScale.x * frame.Hands[0].Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);
                                    thumbTipPosition += handController.transform.position;

                                    float affectedHeight = Mathf.Abs(indexTipPosition.y - thumbTipPosition.y);

                                    Vector3 center = (indexTipPosition + thumbTipPosition) / 2f;
                                    spline.PullAtPosition(center, effectStrength*2f, affectedHeight, currentDeformFunction, Spline.UseAbsolutegeHeight);
                                }
                                break;
                            case GESTURE.PULL2: //pull with pinch
                                {
                                   //Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 2f); };
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.5f; };
                                    
                                    spline.PullAtPosition(tipPosition, effectStrength * 2f, affectedArea, currentDeformFunction);
                                }
                                break;
                            case GESTURE.SMOOTH1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input) * 0.05f; };
                                    //spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea*0.5f, currentDeformFunction);
                                    spline.SmoothArea(tipPosition, 0.5f, affectedArea * 0.4f, 8f,  currentDeformFunction);
                                }
                                break;
                            case GESTURE.NONE:
                                break;
                            default:
                                break;
                        }
                    //}
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
                            case TOOL.PUSHTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 0.8f); };
                                    spline.PushAtPosition(tipPosition, splineDistToPoint, effectStrength, affectedArea/2, currentDeformFunction, true);
                                }
                                break;
                            case TOOL.PULLTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Pow(Mathf.Cos(input), 1f); };
                                    spline.PullAtPosition(tipPosition, effectStrength*2, affectedArea, currentDeformFunction, true);
                                }
                                break;
                            case TOOL.SMOOTHTOOL1:
                                {
                                    Func<float, float> currentDeformFunction = delegate (float input) { return Mathf.Cos(input); };
                                    //reduced affected area
                                    //spline.SmoothAtPosition(tipPosition, effectStrength, affectedArea * 0.2f, currentDeformFunction);
                                    spline.SmoothArea(tipPosition, 0.5f, affectedArea * 0.5f, 8f, currentDeformFunction);

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

        if (DebugdrawLine)
        { //redraw debug line
            Vector3[] tmp = spline.getSpline();
            
            for (int i=0;i < tmp.Length; i++)
            {
                
                lineRenderer.SetPosition(i, tmp[i]);
            }
            DebugdrawLine = false;
        }


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
        if(hands[0].Confidence < 0.2f)
            Debug.Log("Confidence" + hands[0].Confidence);

        // get closest finger
        Vector3 closestFinger = getNearestFinger(hands[0]);
        
        Vector3 thumbPosition = handController.transform.localScale.x * hands[0].Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);
        thumbPosition += handController.transform.position;

        Vector3 palmPosition = handController.transform.localScale.x * hands[0].PalmPosition.ToUnityScaled(false);
        palmPosition += handController.transform.position;

        // is palm in obj & finger-palm-center angle ~90 degree?=>smooth
        Vector3 v1 = palmPosition - closestFinger;
        Vector3 v2 = palmPosition - new Vector3(0, palmPosition.y, 0);
        float dotValue = Vector3.Dot(v1.normalized, v2.normalized);
        if (dotValue > 1.0f)
            dotValue = 1.0f;
        else if (dotValue < -1.0f)
            dotValue = -1.0f;
        GESTURE tmp;
        if (dotValue < 0.2f && dotValue > -0.2f && spline.DistanceToPoint(palmPosition) <= 0f)
        {
            //Debug.Log("dotValue:\t" + dotValue + "\tsmoothing");
            tmp = GESTURE.SMOOTH1;
        }
        else if ((spline.DistanceToPoint(thumbPosition) <= 0f) && (spline.DistanceToPoint(getNearestFinger(hands[0], true)) <= 0f))
        {         // is thumb in obj? & closest in obj => pull
            Debug.Log(thumbPosition + " | " + getNearestFinger(hands[0], true));
            tmp = GESTURE.PULL1;
        }
        else if (spline.DistanceToPoint(closestFinger) < 0f)
        {        // if closest in obj=> push
            //Debug.Log("push: "+ spline.DistanceToPoint(closestFinger));
            tmp = GESTURE.PUSH1;
        }
        else {
            tmp = GESTURE.NONE;
        }
        //Debug.Log(tmp);
        return tmp;




        /*
        tipPosition = hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
        tipPosition *= handController.transform.localScale.x; //scale position with Handmovement
        tipPosition += handController.transform.position;
        if (hands.Count > 1)
        {
            return GESTURE.SMOOTH1;
        }
        //calculate pinch-angle
        Vector3 indexTipPosition = handController.transform.localScale.x * hands[0].Fingers.FingerType(Finger.FingerType.TYPE_INDEX)[0].TipPosition.ToUnityScaled(false);
        Vector3 thumbTipPosition = handController.transform.localScale.x * hands[0].Fingers.FingerType(Finger.FingerType.TYPE_THUMB)[0].TipPosition.ToUnityScaled(false);
        //Vector3 palmPosition = handController.transform.localScale.x * hands[0].PalmPosition.ToUnityScaled(false);

         v1 = palmPosition - indexTipPosition;
         v2 = palmPosition - thumbTipPosition;
        v1.Normalize();
        v2.Normalize();
        dotValue = Vector3.Dot(v1, v2);
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
            return GESTURE.PULL1;
        } else {
            if (hands[0].PinchStrength > 0.5f || hands[1].PinchStrength > 0.5f) {
                return GESTURE.PULL1;
            }
            else {
                return GESTURE.PUSH1;
            }
        }
        */
    }

    private Vector3 getNearestFinger(Hand hand, bool ignoreThumb = false)
    {
        //Vector3 closestFinger = hand.Fingers[0];
        int index = 0;
        Vector3 closestFinger = Vector3.zero;// = handController.transform.localScale.x * hand.Fingers[0].TipPosition.ToUnityScaled(false)
        for (int i = 0; i< hand.Fingers.Count;i++)
        {
            Vector3 tmp = handController.transform.localScale.x * hand.Fingers[i].TipPosition.ToUnityScaled(false);
            tmp += handController.transform.position;
            if ((hand.Fingers[i].Type == Finger.FingerType.TYPE_THUMB) && !ignoreThumb)
            {
                continue;      
            }
            if (closestFinger == Vector3.zero)
            {
                index = i;
                closestFinger = tmp;
            }
            else if (spline.DistanceToPoint(closestFinger) > spline.DistanceToPoint(tmp))
            {
                index = i;
                closestFinger = tmp;
            }
        }
        Debug.Log("closest finger: " + hand.Fingers[index].Type);
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
        // initiate the Spline 
        spline = new Spline(ClayRadius, ClayHeight, ClayResolution, ClayVariance);

        // generate initial clay-object
        latheController.updateMesh(spline.getSplineList());
    }

    internal Spline getSpline()
    {
        return spline;
    }
}
