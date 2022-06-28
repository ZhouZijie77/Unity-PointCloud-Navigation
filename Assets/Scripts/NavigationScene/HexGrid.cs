using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;



public class HexGrid : MonoBehaviour
{
    public int chunkCountX = 30, chunkCountZ = 10; //块的数量
    int cellCountX;
    int cellCountZ;

    public HexCell cellPrefab; //cell预制件

    public TextMeshProUGUI cellLabelPrefab; //cell标签预制件
    public HexGridChunk chunkPrefab; //块的预制件

    public Color defaultColor = Color.white;
    public Color touchedColor = Color.magenta;

    HexCell[] cells; //所有的cells

    HexGridChunk[] chunks;



    public HexCell[] Cells
    {
        get
        {
            return cells;
        }
    }

    void Awake()
    {

        cellCountX = chunkCountX * HexMetrics.chunkSizeX;
        cellCountZ = chunkCountZ * HexMetrics.chunkSizeZ;
        CreateChunks(); //创建块
        CreateCells(); //创建cells

    }

    void CreateChunks()
    {
        chunks = new HexGridChunk[chunkCountX * chunkCountZ];

        for (int z = 0, i = 0; z < chunkCountZ; z++)
        {
            for (int x = 0; x < chunkCountX; x++)
            {
                HexGridChunk chunk = chunks[i++] = Instantiate(chunkPrefab, transform);
                
                chunk.transform.localPosition = new Vector3(0f, 0f, 0f);
            }
        }
    }
    
    void CreateCells()
    {
        cells = new HexCell[cellCountZ * cellCountX];

        for (int z = 0, i = 0; z < cellCountZ; z++)
        {
            for (int x = 0; x < cellCountX; x++)
            {
                CreateCell(x, z, i++);
            }
        }
#if UNITY_EDITOR
        TimeCount.t2 = System.DateTime.Now;
        if (TimeCount.GetSubSeconds(TimeCount.t1, TimeCount.t2) < 3.0)
        {
            Debug.Log($"t1={TimeCount.t1.ToString("yyyyMMddHH:mm:ss fff")}");
            Debug.Log($"t2={TimeCount.t2.ToString("yyyyMMddHH:mm:ss fff")}");
            Debug.Log($"加载时间：{TimeCount.GetSubSeconds(TimeCount.t1, TimeCount.t2)}");
        }
            
#endif
    }

    /// <summary>
    /// 根据position返回对应cell
    /// </summary>
    public HexCell GetCell(Vector3 position)
    {//返回位置position对应的cell

        position = transform.InverseTransformPoint(position);//世界坐标系到局部坐标系

        HexCoordinates coordinates = HexCoordinates.FromPosition(position);//得到六边形坐标系
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;//得到cell索引
        //Debug.Log($"{coordinates.X},{coordinates.Y},{coordinates.Z})" );
        //Debug.Log("index: "+index);
        if (index < cellCountZ * cellCountX && index>=0)
            return cells[index];
        else return null;
    }
    public int GetCellIndex(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);//世界坐标系到局部坐标系

        HexCoordinates coordinates = HexCoordinates.FromPosition(position);//得到六边形坐标系
        int index = coordinates.X + coordinates.Z * cellCountX + coordinates.Z / 2;//得到cell索引
        //Debug.Log($"{coordinates.X},{coordinates.Y},{coordinates.Z})" );
        //Debug.Log("index: "+index);
        if (index < cellCountZ * cellCountX && index >= 0)
            return index;
        else return -1;
    }

    /// <summary>
    /// 通过cell得到其索引index
    /// </summary>
    public int GetCellIndex(HexCell cell)
    {
        int index = cell.coordinates.X + cell.coordinates.Z * cellCountX + cell.coordinates.Z / 2;
        return index; 
    }
    public HexCell GetCell(int index)
    {
        return cells[index];
    }
    public HexCell GetCell(HexCoordinates coordinates)
    { //由坐标获取cell
        int z = coordinates.Z;
        if (z < 0 || z >= cellCountZ)
        { 
            return null;
        }
        int x = coordinates.X + z / 2;
        if (x < 0 || x >= cellCountX)
        {
            return null;
        }
        return cells[x + z * cellCountX];
    }

    void CreateCell(int x, int z, int i)
    { //创建cell

        //计算出cell的局部坐标(x,y,z)
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        //position.x = (x + z * 0.5f ) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * (HexMetrics.outerRadius * 1.5f);

        
        HexCell cell = cells[i] = new HexCell();
        cell.postion_ = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z); //得到cell的六边形坐标
        cell.Color = defaultColor;
        cell.gCost = int.MaxValue;
        //if (x == 0 && z == 2)
        //{
            
        //}
        //Debug.Log($"test:{cells[0].postion_}");
        //HexCell cell1 = new HexCell();

        //cell.gameObject.SetActive(false);
        if (x > 0)
        { //如果x > 0 ，则设置西邻居为前一号cell
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {//如果z>0并且是偶数，则设置东南邻居
                cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX - 1]);
                }
            }
            else
            { //z>0并且为奇数，
                cell.SetNeighbor(HexDirection.SW, cells[i - cellCountX]);
                if (x < cellCountX - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - cellCountX + 1]);
                }
            }
        }

        //坐标显示
        TextMeshProUGUI label = Instantiate<TextMeshProUGUI>(cellLabelPrefab);
        label.rectTransform.anchoredPosition =
            new Vector2(position.x, position.z);
        label.text = cell.coordinates.ToStringOnSeparateLines();
        cell.uiRect = label.rectTransform; //将UI坐标与cell绑定
        cell.Elevation = 0;
        AddCellToChunk(x, z, cell);
    }

    void AddCellToChunk(int x, int z, HexCell cell)
    { //将该cell添加到对应的chunk当中
        int chunkX = x / HexMetrics.chunkSizeX;
        int chunkZ = z / HexMetrics.chunkSizeZ;
        HexGridChunk chunk = chunks[chunkX + chunkZ * chunkCountX];
        //该cell在chunk中的局部坐标
        int localX = x - chunkX * HexMetrics.chunkSizeX;
        int localZ = z - chunkZ * HexMetrics.chunkSizeZ;
        chunk.AddCell(localX + localZ * HexMetrics.chunkSizeX, cell);
    }

    public void ShowUI(bool visible)
    { //是否显示坐标UI
        for (int i = 0; i < chunks.Length; i++)
        {
            chunks[i].ShowUI(visible);
        }
    }


}
