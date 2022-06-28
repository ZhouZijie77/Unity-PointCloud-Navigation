using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;



public class HexMapEditor : MonoBehaviour
{
    public Color[] colors; //在编辑器中选择颜色

    public HexGrid hexGrid; 

    private Color activeColor;

    HexDirection dragDirection; //drag的方向


    int brushSize;
    int activeElevation; //选中时的高度
    bool applyColor; //选中时是否赋颜色
    bool applyElevation = true;
    bool isEditing = false;
 
    void Awake()
    {
        SelectColor(0);
    }

    void Update()
    {
        //if (Input.GetButtonDown("Fire1") && !EventSystem.current.IsPointerOverGameObject())
        //if (Input.GetMouseButtonDown(0) && isEditing)
        if (Input.GetMouseButton(0) && isEditing&&Input.touchCount!=2 && !EventSystem.current.IsPointerOverGameObject()) 
        {
            //if (!EventSystem.current.IsPointerOverGameObject())
            //{
            //    HandleInput();
            //}
            //else
            //{
            //    GameObject _button = EventSystem.current.currentSelectedGameObject;
            //    Debug.Log(_button);
            //}
            HandleInput();

        }
        
    }


    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(inputRay, out hit))
        {
            HexCell currentCell = hexGrid.GetCell(hit.point);           
            EditCells(currentCell);
            
        }
    }
    void EditCells(HexCell center)
    { //同时编辑多个cell，以传入的center为中心
        int centerX = center.coordinates.X;
        int centerZ = center.coordinates.Z;

        //r为遍历时的半径，遍历周围的cell
        for (int r = 0, z = centerZ - brushSize; z <= centerZ; z++, r++)
        {
            for (int x = centerX - r; x <= centerX + brushSize; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }
        for (int r = 0, z = centerZ + brushSize; z > centerZ; z--, r++)
        {
            for (int x = centerX - brushSize; x <= centerX + r; x++)
            {
                EditCell(hexGrid.GetCell(new HexCoordinates(x, z)));
            }
        }

    }
    void EditCell(HexCell cell)
    { //处理对cell的编辑操作
        if (cell!=null)
        {
            if (applyColor)
            {
                cell.Color = activeColor;
            }
            if (applyElevation)
            {
                cell.Elevation = activeElevation;
            }
            
        }

    }

    public void SelectColor(int index)
    {
        applyColor = index >= 0;
        if (applyColor)
        {
            activeColor = colors[index];
        }
    }
    public void SetElevation(Slider slider)
    { //将传入的slider值转成整型
        activeElevation = (int)slider.value;
        //Debug.Log(activeElevation);
    }
    //public void SetApplyElevation(Toggle toggle)
    //{
    //    applyElevation = toggle.isOn;
    //}
    public void SetBrushSize(Slider slider)
    {
        brushSize = (int)slider.value;
    }
    public void SetisEditing(Toggle toggle)
    {
        isEditing = toggle.isOn;
    }

    public void ShowUI(Toggle toggle)
    { //是否显示坐标UI
        hexGrid.ShowUI(toggle.isOn);
    }    

}
