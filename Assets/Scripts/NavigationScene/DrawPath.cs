using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawPath : MonoBehaviour
{
    /// <summary>
    /// 使用梯度下降方法将轨迹曲线画出来。
    /// </summary>
    /// 
    LineRenderer lineRenderer;
    List<Vector3> positions;
    public HexGrid hexGrid;
    Vector3 startPoint;
    Vector3 endPoint;
    public HexCell targetCell;
    public bool isEnabled = false;

    public float height = 0.1f; //曲线的高度
    float pathStep = 0.05f; //步长
    public Vector3 EndPoint { get => endPoint; set => endPoint = value; }
    public Vector3 StartPoint { get => startPoint; set => startPoint = value; }

    float[] gradX, gradZ; //三个方向的梯度

    private void Awake()
    {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.startWidth = 0.1f; //开始的线宽
        lineRenderer.endWidth = 0.1f; //结束的线宽
        lineRenderer.startColor = Color.green;
        lineRenderer.startColor = Color.green;
        gradX = new float[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ * hexGrid.chunkCountX * hexGrid.chunkCountZ];
        gradZ = new float[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ * hexGrid.chunkCountX * hexGrid.chunkCountZ];
    }
    void Start()
    {
        //positions.Clear();
        //for (int i = 0; i < 100; i++)
        //{
        //    positions.Add(new Vector3(x, 0.1f, z));
        //    x += 0.1f;
        //    z = Mathf.Sin(x);  
        //   // z = 0;

        //    //Debug.Log(positions[i]);
        //}
    }
    void Update()
    {
        lineRenderer.enabled = isEnabled;
    }

    public void AddPositions()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError("起点或终点为空！");
            return;
        }
        // Debug.Log("start:" + startPoint + "  " + "target" + endPoint);
        positions = new List<Vector3>();
        Array.Clear(gradX, 0, gradX.Length);
        Array.Clear(gradZ, 0, gradZ.Length);
        Vector3 currentPos;
        HexCell currentCell = targetCell;
        float dx = endPoint.x - hexGrid.GetCell(endPoint).postion_.x;
        float dz = endPoint.z - hexGrid.GetCell(endPoint).postion_.z;
        int c = 0;

        //Debug.Log("dx:" + dx + " dz:" + dz);
        while (c++ < HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ * hexGrid.chunkCountX * hexGrid.chunkCountZ * 23)
        //while (c++ < 2)
        {
            float nx = currentCell.postion_.x + dx;
            float nz = currentCell.postion_.z + dz;

            //Debug.Log("nx:" + nx + " " + "nz" + nz);
            //在起点附近
            if (Mathf.Abs(nx - startPoint.x) < 0.5f && Mathf.Abs(nz - startPoint.z) < 0.5f)
            {
                currentPos.x = startPoint.x;
                currentPos.y = height;
                currentPos.z = startPoint.z;
                positions.Add(currentPos);
                break;
            }
            currentPos.x = nx;
            currentPos.y = height;
            currentPos.z = nz;
            positions.Add(currentPos);
            //Debug.Log("position:" + currentPos);

            //判断六个方向的邻居是否都已经探索
            bool flag = false;
            for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
            {
                if (currentCell.GetNeighbor(d) == null || currentCell.GetNeighbor(d).gCost == int.MaxValue)
                {
                    flag = true;
                    break;
                }
            }
            if (flag)
            {//六个方向上的邻居至少有一个没有探索或者没有六个邻居，不进行梯度插值

                if (currentCell.parentCell != null)
                {
                    currentCell = currentCell.parentCell;
                    dx = 0;
                    dz = 0;
                }
            }
            else
            { //六个方向都已经被探索，进行梯度插值

                //计算三个方向邻居的梯度
                GradCell(currentCell);
                GradCell(currentCell.GetNeighbor(HexDirection.NW));
                GradCell(currentCell.GetNeighbor(HexDirection.NE));
                GradCell(currentCell.GetNeighbor(HexDirection.E));

                int index = hexGrid.GetCellIndex(currentCell);
                int index0 = hexGrid.GetCellIndex(currentCell.GetNeighbor(HexDirection.NW));
                int index1 = hexGrid.GetCellIndex(currentCell.GetNeighbor(HexDirection.NE));
                int index2 = hexGrid.GetCellIndex(currentCell.GetNeighbor(HexDirection.E));
                Debug.Log($"0:({gradX[index]},{gradZ[index]})");
                Debug.Log("dx:" + dx + " " + "dz" + dz);
                ////将(dx,dz)进行坐标变换
                //Vector3 pos = new Vector3(dx, 0, dz);
                //Quaternion q = Quaternion.Euler(0, 30, 0);
                //Vector3 translation = new Vector3(HexMetrics.innerRadius * 0.5f, 0, HexMetrics.innerRadius * 0.866052f);
                //Matrix4x4 m = Matrix4x4.TRS(translation, q, new Vector3(1, 1, 1));
                //Vector3 pos1 = m.inverse.MultiplyPoint3x4(pos);
                //float dx1 = pos1.x;
                //float dz1 = pos1.z;

                //进行插值
                float x1 = (1 - dx) * gradX[index] + dx * gradX[index2];
                float x2 = (1 - dx) * gradX[index0] + dx * gradX[index1];
                float x = (1 - dz) * x1 + dz * x2;
                float z1 = (1 - dx) * gradZ[index] + dx * gradZ[index2];
                float z2 = (1 - dx) * gradZ[index0] + dx * gradZ[index1];
                float z = (1 - dz) * z1 + dz * z2;
                if (x == 0f && z == 0f)
                {
                    Debug.LogError("梯度为零！");
                    break;
                }
                Vector2 temp = new Vector2(x, z);
                temp = temp.normalized;
                dx += temp.x * pathStep;
                dz += temp.y * pathStep;

                //更新currentCell和(dx,dz)
                Vector3 temp3 = new Vector3(dx, 0, dz) + currentCell.postion_;
                currentCell = hexGrid.GetCell(temp3);
                temp3 = temp3 - currentCell.postion_;
                dx = temp3.x;
                dz = temp3.z;
            }
        }

        lineRenderer.positionCount = positions.Count;
        lineRenderer.SetPositions(positions.ToArray());
        isEnabled = true;
    }
    void GradCell(HexCell cell)
    { //计算当前cell的梯度

        if (gradX[hexGrid.GetCellIndex(cell)] + gradZ[hexGrid.GetCellIndex(cell)] > 0.0f)
        { //当前cell梯度已经计算过了
            return;
        }
        float d1 = 0f;
        float d2 = 0f;
        float d3 = 0f;
        float dx,dz;

        HexCell[] neighbors = cell.getNeighbors;

        //计算三个方向梯度
        if (neighbors[5] != null && neighbors[5].gCost != int.MaxValue)
        {
            d1 += cell.gCost - neighbors[5].gCost;
        }
        if (neighbors[2] != null && neighbors[2].gCost != int.MaxValue)
        {
            d1 += neighbors[2].gCost - cell.gCost;
        }
        if (neighbors[0] != null && neighbors[0].gCost != int.MaxValue)
        {
            d2 += cell.gCost - neighbors[0].gCost;
        }
        if (neighbors[3] != null && neighbors[3].gCost != int.MaxValue)
        {
            d2 += neighbors[3].gCost - cell.gCost;
        }
        if (neighbors[1] != null && neighbors[1].gCost != int.MaxValue)
        {
            d3 += cell.gCost - neighbors[1].gCost;
        }
        if (neighbors[4] != null && neighbors[4].gCost != int.MaxValue)
        {
            d3 += neighbors[4].gCost - cell.gCost;
        }

        Debug.Log("d1=" + d1 + "," + "d2=" + d2 + "," + "d3=" + d3);
        //将三个方向梯度合并为(dx,dz)
        dx = d3 - d1 * 0.5f + d2 * 0.5f;
        dz = (d1 + d2) * 0.866025f;
        Vector2 temp = new Vector2(dx, dz);
        if (temp.magnitude != 0)
            temp = temp.normalized;
        gradX[hexGrid.GetCellIndex(cell)] = temp.x;
        gradZ[hexGrid.GetCellIndex(cell)] = temp.y;

    }



}
