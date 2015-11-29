using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour {

    public MeshFilter exportSTLObject;
    public List<Sprite> targetImages;
    public UnityEngine.UI.Image targetCanvas;
    public UnityEngine.UI.Text infoText;
    public int targetCount = 4;

    enum MODE
    {
        FREEFORM,
        TARGET
    }

    private float startTime;
    private PotteryManager manager;
    private List<Spline> targetSplines;
    private List<Spline> userSplines;
    private List<float> userTimes;
    private int targetstep;

	// Use this for initialization
	void Start () {
        
        targetCanvas.transform.parent.gameObject.SetActive(false);
        infoText.enabled = false;
        startTime = Time.time;
        initLists();
        manager = this.transform.parent.gameObject.GetComponent<PotteryManager>();
        targetSplines = new List<Spline>();
        importSplines();
        targetstep = -1;

    }

    // Update is called once per frame
    void Update () {
        getInput();
    }

    private void initLists()
    {
        targetstep = 0;
        userSplines = new List<Spline>();
        userTimes = new List<float>();
    }

    private void getInput()
    {
            //Press space to get to the next step 
        if (Input.GetKeyUp("space") && targetstep != -1)
        {
            manager.resetAll();
            if (targetstep == 0)
            {
                userTimes.Add(Time.time - startTime);
            }
            else
            {
                userTimes.Add(Time.time - getAllTimes());
            }
            userSplines.Add(manager.getSpline());

            targetstep += 1;
            if (targetstep == targetCount) {
                targetstep = 0;
                targetCanvas.sprite = targetImages[targetstep];
                //targetCanvas.transform.parent.gameObject.SetActive(true);
                Debug.Log("finished!");
                compareSpline();
                
                return;
            }

            Debug.Log("next step!");
            //next image
            targetCanvas.sprite = targetImages[targetstep];
        }

        // reset
        if (Input.GetKeyUp("r"))
        {
            targetstep = 0;
            targetCanvas.sprite = targetImages[targetstep];

            manager.resetAll();
            StartCoroutine(showInfoText("Object reseted"));
        }
        if (Input.GetKeyUp("f"))
        {
            if(targetstep == -1)
            {
                initLists();
                targetCanvas.sprite = targetImages[0];
                targetCanvas.transform.parent.gameObject.SetActive(true);
                manager.resetAll();
                StartCoroutine(showInfoText("Target Modus"));
            } else {
                targetstep = -1;
                targetCanvas.sprite = targetImages[0];
                targetCanvas.transform.parent.gameObject.SetActive(false);
                manager.resetAll();
                StartCoroutine(showInfoText("Freeform Modus"));
            }
        }
        if (Input.GetKeyUp("s"))
        {
            String objectName = "stlExport" + Random.Range(0,100).ToString();
            Export.exportSTL(exportSTLObject.mesh, objectName);
            StartCoroutine(showInfoText("Object exported to Documents/Pottery as " + objectName + ".stl"));
        }
    }

    private float getAllTimes()
    {
        float sum = 0;
        for(int i = 0; i < userTimes.Count; i++)
        {
            sum += userTimes[i];
        }
        return sum;
    }

    internal void addUserSpline(Spline spline)
    {
        userSplines.Add(spline);
    }

    private void importSplines()
    {
        for(int i=0; i< targetCount; i++)
        {
            targetSplines.Add(new Spline(Export.importSpline("targetSpline_" + i.ToString())));
        }
        
    }

    public void export()
    {
        Export.exportSTL(exportSTLObject.mesh, "sphere3");
    }

    public void compareSpline()
    {
        String userID = "result" + Random.Range(0, 100).ToString();
        SplineComparison.compare(targetSplines, userSplines, userTimes, userID);
        StartCoroutine(showInfoText("Target Modus finished. Result is saved in Documents as: " + userID + ".csv"));
        //todo reset all
        initLists();
        manager.resetAll();
    }


    /// <summary>
    /// Coroutine to display Information Text
    /// </summary>
    /// <param name="text">the text you want to show</param>
    /// <returns></returns>
    public IEnumerator showInfoText(String text)
    {
        infoText.enabled = true;
        infoText.text = text;
        yield return new WaitForSeconds(3f);
        infoText.enabled = false;
    }
}
