using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

enum SubDivideType
{
    Four,
    Horizon,
    Vertual,
}

enum _9Slice
{
    LeftUp,
    Up,
    RightUp,
    Left,
    Center,
    Right,
    LeftDown,
    Down,
    RightDown,

    AllUp,
    AllRight,
    AllDown,
    AllLeft,

    Horizon,
    Vertual,

    All,
}

class TempStruct
{
    public Rect rect;
    public _9Slice style;
    public bool subDivide;
}

class SubMeshStyle
{
    public SubMeshStyle(SubDivideType subType, _9Slice[] list)
    {
        this.type = subType;
        this.subStyleList = list;
    }

    public SubDivideType type;
    public _9Slice[] subStyleList;
}

public class GridCreator : MonoBehaviour
{
    const float threeOne = 1 / 3f;
    const float sixOne = 1 / 6f;
    static Vector2[] leftDownUv = new Vector2[]{ new Vector2(0, threeOne), new Vector2(threeOne, threeOne), new Vector2(threeOne, 0), new Vector2(0, 0)};
    static Vector2[] subLeftDownUv = new Vector2[]{ new Vector2(0, sixOne), new Vector2(sixOne, sixOne), new Vector2(sixOne, 0), new Vector2(0, 0)};

    static Vector2[] ChangeUVRect(Vector2[] uv, Vector2 diff)
    {
        Vector2[] newUv = new Vector2[uv.Length];
        for(int i = 0; i < uv.Length; i++)
        {
            newUv[i] = uv[i] + diff;
        }

        return newUv;
    }

    Dictionary<int, SubMeshStyle> allSubMeshStyle = new Dictionary<int, SubMeshStyle>()
    {
        {(int)_9Slice.All, new SubMeshStyle(SubDivideType.Four, new _9Slice[]{_9Slice.LeftUp, _9Slice.RightUp, _9Slice.RightDown, _9Slice.LeftDown})},

        {(int)_9Slice.Vertual, new SubMeshStyle(SubDivideType.Vertual, new _9Slice[]{_9Slice.Left, _9Slice.Right})},
        {(int)_9Slice.Horizon, new SubMeshStyle(SubDivideType.Horizon, new _9Slice[]{_9Slice.Up, _9Slice.Down})},

        {(int)_9Slice.AllUp, new SubMeshStyle(SubDivideType.Four, new _9Slice[]{_9Slice.LeftUp, _9Slice.RightUp, _9Slice.Right, _9Slice.Left})},
        {(int)_9Slice.AllRight, new SubMeshStyle(SubDivideType.Four, new _9Slice[]{_9Slice.Up, _9Slice.RightUp, _9Slice.RightDown, _9Slice.Down})},
        {(int)_9Slice.AllDown, new SubMeshStyle(SubDivideType.Four, new _9Slice[]{_9Slice.Left, _9Slice.Right,  _9Slice.RightDown, _9Slice.LeftDown})},
        {(int)_9Slice.AllLeft, new SubMeshStyle(SubDivideType.Four, new _9Slice[]{_9Slice.LeftUp, _9Slice.Up, _9Slice.Down, _9Slice.LeftDown})},
    } ;
    
    Dictionary<int, Vector2[]> allUvs = new Dictionary<int, Vector2[]>(){
        {(int)_9Slice.LeftUp, ChangeUVRect(leftDownUv, new Vector2(0, 1 - threeOne))},
        {(int)_9Slice.Up, ChangeUVRect(leftDownUv, new Vector2(threeOne, 1 - threeOne))},
        {(int)_9Slice.RightUp, ChangeUVRect(leftDownUv, new Vector2(1 - threeOne, 1 - threeOne))},

        {(int)_9Slice.Left, ChangeUVRect(leftDownUv, new Vector2(0, threeOne))},
        {(int)_9Slice.Center, ChangeUVRect(leftDownUv, new Vector2(threeOne, threeOne))},
        {(int)_9Slice.Right, ChangeUVRect(leftDownUv, new Vector2(1 - threeOne, threeOne))},

        {(int)_9Slice.LeftDown, leftDownUv},
        {(int)_9Slice.Down, ChangeUVRect(leftDownUv, new Vector2(threeOne, 0))},
        {(int)_9Slice.RightDown,  ChangeUVRect(leftDownUv, new Vector2(1 - threeOne, 0))},
    };

    Dictionary<int, Vector2[]> subAllUvs = new Dictionary<int, Vector2[]>(){
        {(int)_9Slice.LeftUp, ChangeUVRect(subLeftDownUv, new Vector2(0, 1 - sixOne))},
        {(int)_9Slice.Up, ChangeUVRect(subLeftDownUv, new Vector2(sixOne, 1 - sixOne))},
        {(int)_9Slice.RightUp, ChangeUVRect(subLeftDownUv, new Vector2(1 - sixOne, 1 - sixOne))},

        {(int)_9Slice.Left, ChangeUVRect(subLeftDownUv, new Vector2(0, sixOne))},
        {(int)_9Slice.Center, ChangeUVRect(subLeftDownUv, new Vector2(sixOne, sixOne))},
        {(int)_9Slice.Right, ChangeUVRect(subLeftDownUv, new Vector2(1 - sixOne, sixOne))},

        {(int)_9Slice.LeftDown, subLeftDownUv},
        {(int)_9Slice.Down, ChangeUVRect(subLeftDownUv, new Vector2(sixOne, 0))},
        {(int)_9Slice.RightDown,  ChangeUVRect(subLeftDownUv, new Vector2(1 - sixOne, 0))},
    };


    public Vector2Int[] newVertices = new Vector2Int[]{
        new Vector2Int(0, 0), new Vector2Int(1, 0), new Vector2Int(1, 1), new Vector2Int(0, 1)
    };

    HashSet<Vector2Int> _pointsSet = new HashSet<Vector2Int>();
    List<Vector2Int> _allPoints = new List<Vector2Int>();
    List<_9Slice> _allPointStyles = new List<_9Slice>();

   void Start()
    {
        SetInfo(newVertices.Select(m=>new Vector2Int(Mathf.FloorToInt(m.x), Mathf.FloorToInt(m.y))).ToList());
    }

    List<Vector2> _allPointsGenerate = new List<Vector2>();
    List<Vector2> _allUVGenerate = new List<Vector2>();
    List<int> _allTriangle = new List<int>();

    public void SetInfo(List<Vector2Int> input)
    {
        _allPoints.Clear();
        _pointsSet.Clear();
        _allPointStyles.Clear();

        foreach (var item in input)
        {
            _allPoints.Add(item);
            _pointsSet.Add(item);
            _allPointStyles.Add(default);
        }

        List<TempStruct> allRect = new List<TempStruct>();
        for(int i = 0; i < _allPoints.Count; i++)
        {
            var style = CalStyle(_allPoints[i]);
            Rect rect = new Rect(_allPoints[i], Vector2.one);
            if(style > _9Slice.RightDown)
            {
                // 细分网格
                CreateSubMesh(rect, style, allRect);
            }
            else
            {
                allRect.Add(new TempStruct()
                {
                    style = style, rect = rect
                });
            }
        }

        Mesh mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;

        for(int i = 0; i < allRect.Count; i++)
        {
            var style = allRect[i].style;
            GenerateRect(allRect[i].rect, style, allRect[i].subDivide);
        }

        mesh.vertices = _allPointsGenerate.Select(m=>new Vector3(m.x, 0, m.y)).ToArray();
        mesh.uv = _allUVGenerate.ToArray();
        mesh.triangles = _allTriangle.ToArray();
    }

    private void CreateSubMesh(Rect rect, _9Slice style, List<TempStruct> allRect)
    {
        var subDivide = allSubMeshStyle[(int)style];
        switch (subDivide.type)
        {
            case SubDivideType.Four:
                var size = rect.size / 2;
                allRect.Add(new TempStruct(){rect = new Rect(rect.min + new Vector2(0, size.y), size), style = subDivide.subStyleList[0], subDivide = true});
                allRect.Add(new TempStruct(){rect = new Rect(rect.min + size, size), style = subDivide.subStyleList[1], subDivide = true});
                allRect.Add(new TempStruct(){rect = new Rect(rect.min + new Vector2(size.x, 0), size), style = subDivide.subStyleList[2], subDivide = true});
                allRect.Add(new TempStruct(){rect = new Rect(rect.min, size), style = subDivide.subStyleList[3], subDivide = true});
                break;
            case SubDivideType.Horizon:
                size = new Vector2(rect.size.x, rect.size.y / 2);
                allRect.Add(new TempStruct(){rect = new Rect(rect.min + new Vector2(0, size.y), size), style = subDivide.subStyleList[0], subDivide = true});
                allRect.Add(new TempStruct(){rect = new Rect(rect.min, size), style = subDivide.subStyleList[1], subDivide = true});
                break;
            case SubDivideType.Vertual:
                size = new Vector2(rect.size.x / 2, rect.size.y);
                allRect.Add(new TempStruct(){rect = new Rect(rect.min, size), style = subDivide.subStyleList[0], subDivide = true});
                allRect.Add(new TempStruct(){rect = new Rect(rect.min + new Vector2(size.x, 0), size), style = subDivide.subStyleList[1], subDivide = true});
                break;
            default:throw new Exception("cannot here");
        }
    }

    private void GenerateRect(Rect rect, _9Slice style, bool subDivide)
    {
        int fromIndex = _allPointsGenerate.Count;

        // points
        _allPointsGenerate.Add(rect.min + new Vector2(0, rect.size.y));
        _allPointsGenerate.Add(rect.min + new Vector2(rect.size.x, rect.size.y));
        _allPointsGenerate.Add(rect.min + new Vector2(rect.size.x, 0));
        _allPointsGenerate.Add(rect.min + new Vector2(0, 0));

        // uv
        if(subDivide)
        {
            _allUVGenerate.AddRange(subAllUvs[(int)style]);
        }
        else
        {
            _allUVGenerate.AddRange(allUvs[(int)style]);
        }

        // triangle
        _allTriangle.Add(fromIndex);
        _allTriangle.Add(fromIndex + 1);
        _allTriangle.Add(fromIndex + 2);

        _allTriangle.Add(fromIndex);
        _allTriangle.Add(fromIndex + 2);
        _allTriangle.Add(fromIndex + 3);
    }

    bool[] roundEmpty = new bool[4];
    Vector2Int[] dirs = new Vector2Int[]
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left,
    };

    private _9Slice CalStyle(Vector2Int vector2Int)
    {
        int count = 0; 
        for(int i = 0; i < dirs.Length; i++)
        {
            roundEmpty[i] = !_pointsSet.Contains(vector2Int + dirs[i]);
            count += (roundEmpty[i] ? 1 : 0);
        }

        switch (count)
        {
            case 0: return _9Slice.Center;
            case 1: return GetDir(roundEmpty);
            case 2: return GetCornor(roundEmpty);
            case 3: return GetAllDir(roundEmpty);
            case 4: return _9Slice.All;
            default:
                break;
        }

        throw new Exception("Can not here");
    }

    private _9Slice GetAllDir(bool[] roundEmpty)
    {
        if(!roundEmpty[0]) return _9Slice.AllDown;
        if(!roundEmpty[1]) return _9Slice.AllLeft;
        if(!roundEmpty[2]) return _9Slice.AllUp;
        if(!roundEmpty[3]) return _9Slice.AllRight;
        throw new Exception("Can not here");
    }

    private _9Slice GetCornor(bool[] roundEmpty)
    {
        if(roundEmpty[0] && roundEmpty[1]) return _9Slice.RightUp;
        if(roundEmpty[1] && roundEmpty[2]) return _9Slice.RightDown;
        if(roundEmpty[2] && roundEmpty[3]) return _9Slice.LeftDown;
        if(roundEmpty[3] && roundEmpty[0]) return _9Slice.LeftUp;
        if(roundEmpty[0] && roundEmpty[2]) return _9Slice.Horizon;
        if(roundEmpty[1] && roundEmpty[3]) return _9Slice.Vertual;

        throw new Exception("Can not here");
    }

    private _9Slice GetDir(bool[] roundEmpty)
    {
        if(roundEmpty[0]) return _9Slice.Up;
        if(roundEmpty[1]) return _9Slice.Right;
        if(roundEmpty[2]) return _9Slice.Down;
        if(roundEmpty[3]) return _9Slice.Left;
        throw new Exception("Can not here");
    }
}
