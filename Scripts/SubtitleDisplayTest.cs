using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum SubtitleOptions { AttachToController, DelayedFollowUp, ScenePlacement }
public class SubtitleDisplayTest : MonoBehaviour
{
    public Transform centerEyeAnchor;
    public Transform rightHandAnchor;
    public GameObject subtitleBoxMarkers;
    private Transform[] markers;
    private float attachDis = 5;
    private bool subtitleEnabled;
    private Canvas canvas;
    public float sensitiveness = 0.92f;
    public SubtitleOptions displayOption;


    // Start is called before the first frame update
    void Start()
    {

        int children = subtitleBoxMarkers.transform.childCount;
        markers = new Transform[children];
        for (int i = 0; i < children; ++i)
        {
            markers[i] = subtitleBoxMarkers.transform.GetChild(i);
        }
        Debug.Log(markers.Length);
        subtitleEnabled = true;
        canvas = GetComponentInChildren<Canvas>();
        canvas.transform.SetPositionAndRotation(centerEyeAnchor.transform.position + centerEyeAnchor.transform.forward * attachDis, centerEyeAnchor.transform.rotation);
    }

    // Update is called once per frame
    void Update()
    {
        switch (displayOption)
        {
            case SubtitleOptions.AttachToController:
                if (subtitleEnabled)
                {
                    if (OVRInput.GetDown(OVRInput.Button.One))
                    {
                        subtitleEnabled = false;
                        canvas.gameObject.SetActive(false);

                    }
                    canvas.transform.SetPositionAndRotation(rightHandAnchor.transform.position + rightHandAnchor.transform.forward * attachDis, centerEyeAnchor.transform.rotation);
                }
                else
                {
                    if (OVRInput.GetDown(OVRInput.Button.One))
                    {
                        subtitleEnabled = true;
                        canvas.gameObject.SetActive(true);
                    }
                }

                break;
            case SubtitleOptions.DelayedFollowUp:
                //Debug.Log(Mathf.Abs(Quaternion.Dot(centerEyeAnchor.transform.rotation, canvas.transform.rotation)));
                if (Mathf.Abs(Quaternion.Dot(centerEyeAnchor.transform.rotation, canvas.transform.rotation)) < sensitiveness)
                {
                    // do something
                    canvas.transform.SetPositionAndRotation(centerEyeAnchor.transform.position + centerEyeAnchor.transform.forward * attachDis, centerEyeAnchor.transform.rotation);
                }
                break;
            case SubtitleOptions.ScenePlacement:
                float min = Mathf.Abs(Quaternion.Dot(centerEyeAnchor.transform.rotation, markers[0].rotation));
                int index = 0;
                for (int i = 1; i < markers.Length; ++i)
                {
                    float dotProduct = Mathf.Abs(Quaternion.Dot(centerEyeAnchor.transform.rotation, markers[i].transform.rotation));
                    if (dotProduct < min)
                    {
                        min = dotProduct;
                        index = i;
                    }
                }
                canvas.transform.SetParent(markers[index]);
                break;
            default:
                break;
        }


        //if (fixedHeadPos)
        //{
        //    canvas.transform.SetPositionAndRotation(headPos.transform.position + headPos.transform.forward * attachDis, headPos.transform.rotation);
        //} else if (delayedFollowUp)
        //{
        //    //Debug.Log("HEAD: "+headPos.transform.rotation);
        //    //Debug.Log("CANVAS: "+canvas.transform.rotation);
        //    Debug.Log(Mathf.Abs(Quaternion.Dot(headPos.transform.rotation, canvas.transform.rotation)));
        //    if (Mathf.Abs(Quaternion.Dot(headPos.transform.rotation, canvas.transform.rotation)) < delayFactor)
        //    {
        //        // do something
        //        canvas.transform.SetPositionAndRotation(headPos.transform.position + headPos.transform.forward * attachDis, headPos.transform.rotation);
        //    }
        //}

    }
}
