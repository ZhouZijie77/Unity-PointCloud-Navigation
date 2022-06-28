using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMesh : MonoBehaviour
{
    //处理上色部分
    Mesh hexMesh;
    MeshCollider meshCollider;

    //设置成为静态可以节省内存空间
    static List<Vector3> vertices = new List<Vector3>();
    static List<int> triangles = new List<int>();
    static List<Color> colors = new List<Color>();
    void Awake()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        meshCollider = gameObject.AddComponent<MeshCollider>();
        hexMesh.name = "Hex Mesh";
        
    }

    public void Triangulate(HexCell[] cells)
    {
        //清除旧的数据
        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        colors.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.colors = colors.ToArray();
        hexMesh.RecalculateNormals();

        meshCollider.sharedMesh = hexMesh;
    }
    void Triangulate(HexCell cell)
    {//依据cell的邻居进行三角化
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            if(cell!=null)
            {
                
                Triangulate(d, cell);
            }
            else
                Debug.Log("Cell is null!!");
        }
    }
    void Triangulate(HexDirection direction, HexCell cell)
    {//给传入的cell添加6个三角形
        Vector3 center = cell.postion_; //该cell的中心位置

        Vector3 v1 = center + HexMetrics.GetFirstSolidCorner(direction); //第一个纯色顶点
        Vector3 v2 = center + HexMetrics.GetSecondSolidCorner(direction); //第二个纯色顶点
        //添加纯色区域的定点和三角形并上色
        AddTriangle(center, v1, v2);
        AddTriangleColor(cell.Color);

        //在前三个方向上给混色区域上色
        if (direction <= HexDirection.SE)
        {
            if(cell==null)
            {
                Debug.Log("!CELL is null");
            }
            else
                TriangulateConnection(direction, cell, v1, v2);
        }
    }

    void TriangulateConnection(//用混色的矩形连接相邻的六边形，再将三角形空隙填上
        HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2
    )
    {
        HexCell neighbor = cell.GetNeighbor(direction);
        if (neighbor == null)
        { //如果该方向上不存在邻居，则返回
            return;
        }

        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;
        //将v3和v4的高度升上去
        v3.y = v4.y = neighbor.Elevation * HexMetrics.elevationStep;

        //三角化边缘平台
        if (cell.GetEdgeType(direction) == HexEdgeType.Slope)
        { //如果边类型是slope，则分成好几个平台
            TriangulateEdgeTerraces(v1, v2, cell, v3, v4, neighbor);
        }
        else
        {//对于flat和cliff，直接三角化
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(cell.Color, neighbor.Color);
        }


        //再将三角形空隙填上
        HexCell nextNeighbor = cell.GetNeighbor(direction.Next());
        if (direction <= HexDirection.E && nextNeighbor != null)
        {
            Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
            v5.y = nextNeighbor.Elevation * HexMetrics.elevationStep;

            //按照正确的顺序给TriangulateCorner方法传入参数:bottom,left,right
            if (cell.Elevation <= neighbor.Elevation)
            { //如果该cell比邻居cell高度低
                if (cell.Elevation <= nextNeighbor.Elevation)
                {
                    //如果该cell高度比下一个邻居cell高度低，说明该cell是最低的
                    TriangulateCorner(v2, cell, v4, neighbor, v5, nextNeighbor);
                }
                else
                { //说明下一个邻居cell是最低的
                    TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
                }
            }
            else if (neighbor.Elevation <= nextNeighbor.Elevation)
            {
                TriangulateCorner(v4, neighbor, v5, nextNeighbor, v2, cell);
            }
            else
            {
                TriangulateCorner(v5, nextNeighbor, v2, cell, v4, neighbor);
            }

        }
    }



    void TriangulateEdgeTerraces(
        Vector3 beginLeft, Vector3 beginRight, HexCell beginCell,
        Vector3 endLeft, Vector3 endRight, HexCell endCell
    ) //三角化边缘平台
    {
        Vector3 v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(beginRight, endRight, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, 1);

        AddQuad(beginLeft, beginRight, v3, v4);
        AddQuadColor(beginCell.Color, c2);
        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c2;
            v3 = HexMetrics.TerraceLerp(beginLeft, endLeft, i);
            v4 = HexMetrics.TerraceLerp(beginRight, endRight, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, endCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2);
        }
        AddQuad(v3, v4, endLeft, endRight);
        AddQuadColor(c2, endCell.Color);
    }

    void TriangulateCorner(
        Vector3 bottom, HexCell bottomCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) //将顶点处的三角形区域
    {
        //分别得到bottomcell和leftcell以及rightcell之间的边类型
        HexEdgeType leftEdgeType = bottomCell.GetEdgeType(leftCell);
        HexEdgeType rightEdgeType = bottomCell.GetEdgeType(rightCell);

        //按照类型分情况三角化
        if (leftEdgeType == HexEdgeType.Slope)
        {
            if (rightEdgeType == HexEdgeType.Slope)
            { //和左右两个cell的边都是slope，SSF
                TriangulateCornerTerraces(
                    bottom, bottomCell, left, leftCell, right, rightCell
                );

            }
            else if (rightEdgeType == HexEdgeType.Flat)
            { //左边cell的边为slope，右cell边为flat ，SFS
                TriangulateCornerTerraces(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );

            }
            //左边cell的边为slope，右cell的边为cliff
            else TriangulateCornerTerracesCliff(
                bottom, bottomCell, left, leftCell, right, rightCell
            );

        }
        else if (rightEdgeType == HexEdgeType.Slope)
        {
            if (leftEdgeType == HexEdgeType.Flat)
            {//右边cell为slope，左边cell是flat
                TriangulateCornerTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );

            }
            else TriangulateCornerCliffTerraces(
                bottom, bottomCell, left, leftCell, right, rightCell
            );

        }
        else if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            if (leftCell.Elevation < rightCell.Elevation)
            {
                TriangulateCornerCliffTerraces(
                    right, rightCell, bottom, bottomCell, left, leftCell
                );
            }
            else
            {
                TriangulateCornerTerracesCliff(
                    left, leftCell, right, rightCell, bottom, bottomCell
                );
            }
        }
        else
        {
            AddTriangle(bottom, left, right);
            AddTriangleColor(bottomCell.Color, leftCell.Color, rightCell.Color);
        }

    }

    void TriangulateCornerTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        Vector3 v3 = HexMetrics.TerraceLerp(begin, left, 1);
        Vector3 v4 = HexMetrics.TerraceLerp(begin, right, 1);
        Color c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);
        Color c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, 1);
        AddTriangle(begin, v3, v4);
        AddTriangleColor(beginCell.Color, c3, c4);


        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v3;
            Vector3 v2 = v4;
            Color c1 = c3;
            Color c2 = c4;
            v3 = HexMetrics.TerraceLerp(begin, left, i);
            v4 = HexMetrics.TerraceLerp(begin, right, i);
            c3 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            c4 = HexMetrics.TerraceLerp(beginCell.Color, rightCell.Color, i);
            AddQuad(v1, v2, v3, v4);
            AddQuadColor(c1, c2, c3, c4);
        }

        AddQuad(v3, v4, left, right);
        AddQuadColor(c3, c4, leftCell.Color, rightCell.Color);
    }
    void AddTriangleColor(Color color)
    {//添加三角形三个顶点的颜色
        colors.Add(color);
        colors.Add(color);
        colors.Add(color);
    }
    void AddTriangleColor(Color c1, Color c2, Color c3)
    {
        //给三个顶点赋颜色
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
    }
    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)//传入参数为三个顶点
    {
        //添加边以及三角形
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {//填充混合颜色区域的梯形
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }

    void AddQuadColor(Color c1, Color c2, Color c3, Color c4)
    {//填充混合颜色区域的梯形的颜色
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c3);
        colors.Add(c4);
    }
    void AddQuadColor(Color c1, Color c2)
    {
        colors.Add(c1);
        colors.Add(c1);
        colors.Add(c2);
        colors.Add(c2);
    }

    void TriangulateCornerTerracesCliff(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    ) //处理左边是slope，右边是cliff的情况
    {
        float b = 1f / (rightCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(begin, right, b);
        Color boundaryColor = Color.Lerp(beginCell.Color, rightCell.Color, b);

        TriangulateBoundaryTriangle(
            begin, beginCell, left, leftCell, boundary, boundaryColor
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor
            );
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    void TriangulateCornerCliffTerraces(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 right, HexCell rightCell
    )
    {
        float b = 1f / (leftCell.Elevation - beginCell.Elevation);
        if (b < 0)
        {
            b = -b;
        }
        Vector3 boundary = Vector3.Lerp(begin, left, b);
        Color boundaryColor = Color.Lerp(beginCell.Color, leftCell.Color, b);

        TriangulateBoundaryTriangle(
            right, rightCell, begin, beginCell, boundary, boundaryColor
        );

        if (leftCell.GetEdgeType(rightCell) == HexEdgeType.Slope)
        {
            TriangulateBoundaryTriangle(
                left, leftCell, right, rightCell, boundary, boundaryColor
            );
        }
        else
        {
            AddTriangle(left, right, boundary);
            AddTriangleColor(leftCell.Color, rightCell.Color, boundaryColor);
        }
    }

    void TriangulateBoundaryTriangle(
        Vector3 begin, HexCell beginCell,
        Vector3 left, HexCell leftCell,
        Vector3 boundary, Color boundaryColor
    )
    {
        Vector3 v2 = HexMetrics.TerraceLerp(begin, left, 1);
        Color c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, 1);

        AddTriangle(begin, v2, boundary);
        AddTriangleColor(beginCell.Color, c2, boundaryColor);

        for (int i = 2; i < HexMetrics.terraceSteps; i++)
        {
            Vector3 v1 = v2;
            Color c1 = c2;
            v2 = HexMetrics.TerraceLerp(begin, left, i);
            c2 = HexMetrics.TerraceLerp(beginCell.Color, leftCell.Color, i);
            AddTriangle(v1, v2, boundary);
            AddTriangleColor(c1, c2, boundaryColor);
        }

        AddTriangle(v2, left, boundary);
        AddTriangleColor(c2, leftCell.Color, boundaryColor);
    }

}
