using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//定义了正六边形相关的量
public static class HexMetrics
{


    public const float innerRadius = 0.5f;//内半径

    public const float outerRadius = innerRadius * 1.1547f; //外半径

    //public const float outerRadius = 10f; //外半径
    //public const float innerRadius = outerRadius * 0.8660254f;//内半径



    public const float solidFactor = 0.75f; //内部纯色的比例

    public const float blendFactor = 1f - solidFactor; //边缘混合颜色的比例

    public const float elevationStep = 0.1f; //高度step

    public const int terracesPerSlope = 2; //每个斜坡上平台的数量

    public const int terraceSteps = terracesPerSlope * 2 + 1; // 平台的step，即曲折边的数量

    public const int chunkSizeX = 5, chunkSizeZ = 5; //块的大小

    //水平方向平台步长
    public const float horizontalTerraceStepSize = 1f / terraceSteps;

    //垂直方向上平台步长
    public const float verticalTerraceStepSize = 1f / (terracesPerSlope + 1);

    static Vector3[] corners = { //六个顶点位置
        new Vector3(0f, 0f, outerRadius),
        new Vector3(innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(0f, 0f, -outerRadius),
        new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
        new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
        new Vector3(0f, 0f, outerRadius)
    };


    public static Vector3 GetFirstCorner(HexDirection direction)
    { //根据方向选取顶点
        return corners[(int)direction];
    }

    public static Vector3 GetSecondCorner(HexDirection direction)
    {//返回对应方向上的下一个顶点
        return corners[(int)direction + 1];
    }

    public static Vector3 GetFirstSolidCorner(HexDirection direction)
    { //返回对应方向上的纯色顶点
        return corners[(int)direction] * solidFactor;
    }

    public static Vector3 GetSecondSolidCorner(HexDirection direction)
    {//返回对应方向上的下一个纯色顶点
        return corners[(int)direction + 1] * solidFactor;
    }
    public static HexDirection Previous(this HexDirection direction)
    {//返回 传入direction的前一个方向
        return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
    }

    public static HexDirection Next(this HexDirection direction)
    {//返回 传入direction的后一个方向
        return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
    }
    public static Vector3 GetBridge(HexDirection direction)
    {
        return (corners[(int)direction] + corners[(int)direction + 1]) * blendFactor;
    }

    public static Vector3 TerraceLerp(Vector3 a, Vector3 b, int step)
    { //对斜坡变平台进行线性插值
        float h = step * HexMetrics.horizontalTerraceStepSize;
        a.x += (b.x - a.x) * h;
        a.z += (b.z - a.z) * h;

        //(step+1)/2是因为仅仅只需要调整奇数步的y值
        float v = ((step + 1) / 2) * HexMetrics.verticalTerraceStepSize;
        a.y += (b.y - a.y) * v;
        return a;
    }

    public static Color TerraceLerp(Color a, Color b, int step)
    { //对平台颜色插值
        float h = step * HexMetrics.horizontalTerraceStepSize;
        return Color.Lerp(a, b, h);
    }
    public static HexEdgeType GetEdgeType(float elevation1, float elevation2)
    { //根据两个高度判断边的类型
        if (elevation1 == elevation2)
        {
            return HexEdgeType.Flat;
        }
        float delta = elevation2 - elevation1;
        //if (delta == 0.1f || delta == -0.1f)
        if (delta <= 0.1f && delta >= -0.1f)
        {
            return HexEdgeType.Slope;
        }
        return HexEdgeType.Cliff;
    }
}
