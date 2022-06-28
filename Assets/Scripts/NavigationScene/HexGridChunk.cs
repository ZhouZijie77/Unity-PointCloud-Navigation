using UnityEngine;

public class HexGridChunk : MonoBehaviour
{
	//小格子的集合 chunk
	HexCell[] cells; 

	HexMesh hexMesh;
	Canvas gridCanvas;

	void Awake()
	{
		gridCanvas = GetComponentInChildren<Canvas>();
		hexMesh = GetComponentInChildren<HexMesh>();

		cells = new HexCell[HexMetrics.chunkSizeX * HexMetrics.chunkSizeZ];
		//ShowUI(false); //默认不显示坐标UI
	}

	

	public void AddCell(int index, HexCell cell)
	{ //向chunk中添加cell
		cells[index] = cell;
		//cell.transform.SetParent(transform, false);
		cell.chunk = this;
		cell.uiRect.SetParent(gridCanvas.transform, false);
	}
	public void Refresh()
	{ //重新三角化
		enabled = true;
	}
	void LateUpdate()
	{ //在Update()方法调用之后执行
		hexMesh.Triangulate(cells);
		enabled = false;
	}

	public void ShowUI(bool visible)
	{
		gridCanvas.gameObject.SetActive(visible);
	}
}
