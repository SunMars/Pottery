using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class UIManager : MonoBehaviour {

    public MeshFilter exportSTLObject;
    public List<Sprite> targetImages;
    public UnityEngine.UI.Image targetCanvas;
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
        targetstep = -1;
        targetCanvas.transform.parent.gameObject.SetActive(false);

        startTime = Time.time;
        initLists();
        manager = this.transform.parent.gameObject.GetComponent<PotteryManager>();
        targetSplines = new List<Spline>();
        importSplines();

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
        //Press E to export the current spline 
        if (Input.GetKeyUp("space") && targetstep >= 0)
        {
            targetstep += 1;
            if (targetstep == targetCount) {
                targetstep = 0;
                Debug.Log("finished!");
                compareSpline();
                return;
            }
            if(targetstep == 0)
            {
                userTimes.Add(Time.time - startTime);
            }
            else
            {
                userTimes.Add(Time.time - getAllTimes());
            }
            userSplines.Add(manager.getSpline());

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
        }
        if (Input.GetKeyUp("f"))
        {
            if(targetstep == -1)
            {
                initLists();
                targetCanvas.transform.parent.gameObject.SetActive(true);
                manager.resetAll();   
            } else {
                targetstep = -1;
                targetCanvas.transform.parent.gameObject.SetActive(false);
                manager.resetAll();
            }
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
        SplineComparison.compare(targetSplines, userSplines, userTimes, "Thereza");
        
        //todo reset all
        initLists();
        manager.resetAll();
    }
}
