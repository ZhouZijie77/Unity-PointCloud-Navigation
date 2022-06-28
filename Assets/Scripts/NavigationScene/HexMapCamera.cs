using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HexMapCamera : MonoBehaviour
{
    //包含了视角控制、手势控制的方法
    Transform swivel, stick;
    float zoom = 1f; // zoom level

    // 确定放大缩小的极限位置
    public float stickMinZoom = -250;
    public float stickMaxZoom = -4;

    public float swivelMinZoom, swivelMaxZoom;
    public float moveSpeed;
    public float Mobile_moveSpeed;
    public float moveSpeedMinZoom, moveSpeedMaxZoom;
    public float rotationSpeed;
    public float rotationSensitivityY = 0.01f;//绕Y轴旋转的灵敏度
    float rotationAngle;
    bool isRotating = false;
    public float zoomRate = 5.0f;
    //old touch position
    Vector2 oTouchPos1;
    Vector2 oTouchPos2;
    public AStar aStar;




    //缩放延迟
    int timerScalingDelay = 3; //延迟计数器，前几帧不缩放

    public HexGrid grid;

    public bool IsRotating
    {
        get
        {
            return isRotating;
        }
        set
        {
            isRotating = value;
            if(isRotating)
            {
                //如果旋转被设为true，则同时禁止地图编辑和路径规划
                aStar.IsEditing = false;
                aStar.IsPathPlanning = false;
            }         
        }
    }

    void Awake()
    {
        swivel = transform.GetChild(0);
        stick = swivel.GetChild(0);
    }

    void Update()
    {
        float zoomDelta = Input.GetAxis("Mouse ScrollWheel");
        if (zoomDelta != 0f)
        {
            AdjustZoom(zoomDelta);
        }
        float rotationDelta = Input.GetAxis("Rotation");
        if (rotationDelta != 0f)
        {
            AdjustRotation(rotationDelta);
        }

        float xDelta = Input.GetAxis("Horizontal");
        float zDelta = Input.GetAxis("Vertical");
        if (xDelta != 0f || zDelta != 0f)
        {
            AdjustPosition(xDelta, zDelta);
        }

        if (Input.touchCount == 2 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            //aStar.IsPathPlanning = false;
            var deltaposition = Input.GetTouch(0).deltaPosition;
            AdjustPosition(-deltaposition.x * Mobile_moveSpeed, -deltaposition.y * Mobile_moveSpeed);
            MobileZoom();

        }
        if (Input.touchCount != 2)
        {
            timerScalingDelay = 2; //无缩放操作后，计数器重置
        }
        if (IsRotating && Input.touchCount == 1)
        {//旋转视角
            MobileRotateY();
        }

    }

    void AdjustZoom(float delta)
    {
        //Debug.Log($"zoom{delta}");
        zoom = Mathf.Clamp01(zoom + delta);
        float distance = Mathf.Lerp(stickMinZoom, stickMaxZoom, zoom);
        stick.localPosition = new Vector3(0f, 0f, distance);
        float angle = Mathf.Lerp(swivelMinZoom, swivelMaxZoom, zoom);
        //swivel.localRotation = Quaternion.Euler(angle, 0f, 0f);
    }
    void AdjustPosition(float xDelta, float zDelta)
    {
        Vector3 direction =
            transform.localRotation *
            new Vector3(xDelta, 0f, zDelta).normalized;
        float damping = Mathf.Max(Mathf.Abs(xDelta), Mathf.Abs(zDelta));
        float distance =
            Mathf.Lerp(moveSpeedMinZoom, moveSpeedMaxZoom, zoom) *
            damping * Time.deltaTime;

        Vector3 position = transform.localPosition;
        position += direction * distance;
        transform.localPosition = position;
        transform.localPosition = ClampPosition(position);
    }


    Vector3 ClampPosition(Vector3 position)
    {
        float xMax =
            (grid.chunkCountX * HexMetrics.chunkSizeX - 0.5f) *
            (2f * HexMetrics.innerRadius);
        position.x = Mathf.Clamp(position.x, 0f, xMax);

        float zMax =
            (grid.chunkCountZ * HexMetrics.chunkSizeZ - 1) *
            (1.5f * HexMetrics.outerRadius);
        position.z = Mathf.Clamp(position.z, 0f, zMax);

        return position;
    }

    void AdjustRotation(float delta)
    {
        rotationAngle += delta * rotationSpeed * Time.deltaTime;
        if (rotationAngle < 0f)
        {
            rotationAngle += 360f;
        }
        else if (rotationAngle >= 360f)
        {
            rotationAngle -= 360f;
        }
        transform.localRotation = Quaternion.Euler(0f, rotationAngle, 0f);
    }
    public void SetIsRotating(Toggle toggle)
    {
        IsRotating = toggle.isOn;
        
        
    }
    public void MobileZoom()
    {//进行放大缩小操作


        //new touch position
        Vector2 nTouchPos1 = Input.GetTouch(0).position;
        Vector2 nTouchPos2 = Input.GetTouch(1).position;
        //new and old distance
        float nDis = Vector2.Distance(nTouchPos1, nTouchPos2);
        float oDis = Vector2.Distance(oTouchPos1, oTouchPos2);
        if (timerScalingDelay < 0)
        {
            //Debug.Log("Zoom");
            if (nDis > oDis)
            {//放大

                Camera.main.fieldOfView -= (nDis - oDis) * zoomRate * Time.deltaTime;
            }
            else
                Camera.main.fieldOfView += (oDis - nDis) * zoomRate * Time.deltaTime;
        }
        else
        {
            timerScalingDelay -= 1;
        }
        oTouchPos1 = nTouchPos1;
        oTouchPos2 = nTouchPos2;
    }
    void MobileRotateY()
    {//绕Y轴旋转

        //获取地图原点的屏幕坐标位置
        Vector3 gridMapPos = Camera.main.WorldToScreenPoint(grid.transform.position);
        gridMapPos.y = 0;

        //手指滑动指向以及手指坐标
        Vector3 fingerPos = new Vector3(Input.GetTouch(0).position.x, 0f, Input.GetTouch(0).position.y);
        Vector3 fingerDirection = new(Input.GetTouch(0).deltaPosition.x, 0f, Input.GetTouch(0).deltaPosition.y);
        Vector3 relativeDirection = fingerPos - gridMapPos;
        Vector3 cross = Vector3.Cross(relativeDirection, fingerDirection);
        //判断顺时针或者逆时针
        float rotateY = -cross.y * rotationSensitivityY * Time.deltaTime;
        Debug.Log($"({cross.z})");
        transform.Rotate(0, rotateY, 0);
    }
}
