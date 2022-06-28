using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexCell 
{

    public HexCoordinates coordinates ; //六边形坐标
    Color color; //颜色
    //int elevation = int.MinValue;//高度
    float elevation = float.MinValue;
    public RectTransform uiRect; //该cell对应的UI标签

    public HexGridChunk chunk; //该cell属于的chunk
    public Vector3 postion_ = new(0, 0, 0);

    int pointCount = 0;

    public int Elevation
    {
        get
        {
            return (int)(elevation*10);
        }
        set
        { //设置高度,value为1-6整型
            if (Mathf.Approximately(elevation, value / 10f))
            {
                return;
            }
            elevation = value/10f;
            Vector3 position = postion_;
            position.y = value * HexMetrics.elevationStep;
            postion_ = position;

            //设置UI标签高度
            Vector3 uiPosition = uiRect.localPosition;
            uiPosition.z = value * -HexMetrics.elevationStep;
            uiRect.localPosition = uiPosition;
            
            Refresh();
        }
    }



    public Color Color
    {
        get
        {
            return color;
        }
        set
        {
            if (color == value)
            {
                return;
            }
            color = value;
            Refresh();
        }
    }

    //A*算法用到的启发值
    public int hCost;
    public int gCost;
    public int fCost
    {
        get
        {
            return gCost + hCost;
        }
    }
    public HexCell parentCell;


    //[SerializeField]
    HexCell[] neighbors = new HexCell[6];
    public HexCell[] getNeighbors
    {
        get
        {
            return neighbors;
        }
    }

    public int PointCount { get => pointCount; set => pointCount = value; }

    public HexCell GetNeighbor(HexDirection direction)
    {//获取邻居
        return neighbors[(int)direction];
    }

    public void SetNeighbor(HexDirection direction, HexCell cell)
    {//设置邻居
        neighbors[(int)direction] = cell;
        cell.neighbors[(int)direction.Opposite()] = this;
    }
    public HexEdgeType GetEdgeType(HexDirection direction)
    {//根据方向得到与该方向邻居的边的类型
        return HexMetrics.GetEdgeType(
            elevation, neighbors[(int)direction].elevation
        );
    }
    public HexEdgeType GetEdgeType(HexCell otherCell)
    { //确定两个本cell和othercell之间的边的类型
        return HexMetrics.GetEdgeType(
            elevation, otherCell.elevation
        );
    }
    void Refresh()
    { //当该cell要刷新时，仅仅刷新所属的chunk和其邻居所在的chunk
        if (chunk)
        {
            chunk.Refresh();
            for (int i = 0; i < neighbors.Length; i++)
            {
                HexCell neighbor = neighbors[i];
                if (neighbor != null && neighbor.chunk != chunk)
                {
                    neighbor.chunk.Refresh();
                }
            }
        }
    }

    void RefreshSelfOnly()
    { //仅仅刷新该cell所属的chunk
        chunk.Refresh();
    }
   
}
