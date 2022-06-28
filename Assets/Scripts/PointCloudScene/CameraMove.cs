using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMove : MonoBehaviour
{

    public float sensitivityMousewheel = 10f;
    public float translatesensitivityX = 2f;
    public float translatesensitivityY = 2f;
    // Update is called once per frame
    void Update()
    {
#if UNITY_STANDALONE_WIN
        if (Input.GetAxis("Mouse ScrollWheel") != 0)
        { //滚轮缩放
            GetComponent<Camera>().fieldOfView -= Input.GetAxis("Mouse ScrollWheel") * sensitivityMousewheel;
        }



        if (Input.GetMouseButton(2))
        {
            float translateX = Input.GetAxis("Mouse X") * translatesensitivityX;
            float translateY = Input.GetAxis("Mouse Y") * translatesensitivityY;

            transform.Translate(-translateX, -translateY, 0);
            //transform.Translate(10, 0, 0);
            //Debug.Log(Input.GetAxis("Mouse Y"));
        }
#endif

#if UNITY_ANDROID
        //if (Application.platform == RuntimePlatform.Android)
        //{
        //    Debug.Log("Do something special here!");
        //}
        if (Input.touchCount <= 0)
        {
            return;
        }

        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {//两根手指一起触摸进行平移
            var deltaposition = Input.GetTouch(0).deltaPosition;
            transform.Translate(-deltaposition.x *translatesensitivityX, -deltaposition.y*translatesensitivityY / 100, 0);
            //Debug.Log(Input.GetTouch(0).deltaPosition.x);
        }
#endif
    }
}
