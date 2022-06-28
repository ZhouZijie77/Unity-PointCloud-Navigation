using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGridMap : MonoBehaviour
{
    //根据地图生成可通行区域栅格地图

    public HexGrid hexGrid;
    public int Elevation;
    public int threshold = 100;
    GameObject cloudmap_object;
    GameObject map_3d;
    bool isGridMap = false;
    bool isalreadyCreate = false;
    Vector3[] vertices;
    int count;


    private void Awake()
    {
        cloudmap_object = Resources.Load<GameObject>("cloudmap_object");
        map_3d = Instantiate(cloudmap_object, transform); //动态创建3dmap的游戏对象
        map_3d.SetActive(false);
        map_3d.transform.rotation = Quaternion.Euler(-90.0f, -63.6f, 70.4f);
        map_3d.transform.position = new Vector3(19.6f, 0f, 13.9f);
        StartCoroutine("GetVertices");
        //GetVertices();      
    }


    private void Update()
    {
        if (isGridMap == true && isalreadyCreate == false)
        {
            CreateMap();
            isalreadyCreate = true;
        }
        if (isGridMap == false && isalreadyCreate == true)
        {
            for (int i = 0; i < count; i++)
            {
                Vector3 pos = map_3d.transform.TransformPoint(vertices[i]);
                pos.y = 0;
                HexCell cell = hexGrid.GetCell(pos);
                if (cell == null)
                    continue;
                cell.PointCount = 0;
                cell.Elevation = 0;
                cell.Color = Color.white;
            }
            isalreadyCreate = false;
        }
    }
    void GetVertices()
    {
        Mesh mesh = map_3d.GetComponent<MeshFilter>().sharedMesh;
        if (mesh.isReadable == true)
        {
            count = mesh.vertices.Length;
            vertices = mesh.vertices;
        }
    }
    void CreateMap()
    {
        for (int i = 0; i < count; i++)
        {
            Vector3 pos = map_3d.transform.TransformPoint(vertices[i]);
            pos.y = 0;
            HexCell cell = hexGrid.GetCell(pos);
            if (cell == null)
            {
                //Debug.Log($"pos:{pos}");
                continue;
            }
            cell.PointCount++;
            if (cell.Elevation == 0 && cell.PointCount >= threshold)
            {
                if (cell.coordinates.X == 8 && cell.coordinates.Z == 18)
                {
                    Debug.Log($"posffff:{pos}");
                }
                cell.Elevation = Elevation;
                cell.Color = Color.black;
            }
        }
    }

    public void SetisGridMap()
    {
        isGridMap = !isGridMap;
    }


}
