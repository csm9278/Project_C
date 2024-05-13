using System.Collections.Generic;
using UnityEngine;

public class MapNode : MonoBehaviour
{
    public enum  NodeState
    {
        Lock,
        Visited,
        Moveable,
    }

    public enum NodeType
    {
        NormalBattle,
        Rest,
        Chest,
        Shop,
        Elite,
        Boss
    }

    [Header("--- SizeControll ---")]
    public float maxSize = 2.0f;
    public float changeSpeed = 2.0f;
    Vector3 curSize = Vector3.zero;
    float minSize = 0.0f;
    SpriteRenderer _spRenderer;
    bool isUp = false;

    //Node State
    public NodeState nodeState = NodeState.Lock;
    public NodeType nodeType = NodeType.NormalBattle;
    public Sprite[] nodeImage;
    public int nodeFloor = -1;

    //Circle Image
    public GameObject circleImage;

    //Moveable Node Color
    public List<MapNode> outMoveNode = new List<MapNode>();
    LineRenderer lineRenderer;

    //Color Change
    [Header("--- Color Change ---")]
    bool increaseColor = false;
    public float colorChangeSpeed = 8.0f;
    public Color changeColor;
    Color _curColor;

    [HideInInspector] public MapCreater mapCreater;

    private void Start() => StartFunc();

    private void StartFunc()
    {
        if(_spRenderer == null)
        _spRenderer = GetComponentInChildren<SpriteRenderer>();
        lineRenderer = GetComponent<LineRenderer>();

        curSize = _spRenderer.gameObject.transform.localScale;
        minSize = curSize.x;
    }

    private void Update() => UpdateFunc();

    private void UpdateFunc()
    {
        if (GameManager.inst.pause)
            return;

        SizeControll(isUp);

        ChangeMoveableColor();
    }

    private void OnMouseEnter()
    {
        isUp = true;
    }

    private void OnMouseExit()
    {
        isUp = false;
    }

    private void OnMouseUp()
    {
        if (GameManager.inst.pause)
            return;

        if (nodeState == NodeState.Lock || nodeState == NodeState.Visited)
            return;

        if (!isUp || GameManager.inst.changing)
            return;

        circleImage.gameObject.SetActive(true);
        mapCreater.SetLockMap(nodeFloor);
        _spRenderer.color = Color.green;
        nodeState = NodeState.Visited;

        NodeEnter();

        foreach(MapNode node in outMoveNode)
        {
            node.nodeState = NodeState.Moveable;
        }
    }

    void NodeEnter()
    {
        switch(nodeType)
        {
            case NodeType.NormalBattle:
                GameManager.inst.StartChangeObject(ObjectType.Battle);
                break;

            case NodeType.Shop:
                GameManager.inst.StartChangeObject(ObjectType.Shop);
                break;

            case NodeType.Rest:
                GameManager.inst.StartChangeObject(ObjectType.Rest);
                break;

            case NodeType.Elite:
                GameManager.inst.StartChangeObject(ObjectType.EliteBattle);
                break;

            case NodeType.Chest:
                GameManager.inst.StartChangeObject(ObjectType.Chest);
                break;

            case NodeType.Boss:
                break;
        }
    }

    public void NodeLock()
    {
        _spRenderer.color = Color.white;

        if(nodeState != NodeState.Visited)
            nodeState = NodeState.Lock;
    }

    public void SetNode(NodeState state, int floor)
    {
        nodeState = state;
        nodeFloor = floor;
        SetNodeType(floor);

    }
    
    void SetNodeType(int floor)
    {
        if (floor == 0)
            nodeType = NodeType.NormalBattle;
        else if (floor == mapCreater.mapMaxSize * 0.5f)
        {
            nodeType = NodeType.Chest;
        }
        else if(floor == mapCreater.mapMaxSize - 1)
        {
            nodeType = NodeType.Rest;
        }
        else if (floor == mapCreater.mapMaxSize)
        {
            nodeType = NodeType.Boss;
        }
        else
        {
            float rand = Random.Range(0.0f, 1.0f);
            if(rand <= 0.6f) //60% 일반적으로 생성 그리고 4층 이상부터 30% 확률로 엘리트 생성
            {
                rand = Random.Range(0.0f, 1.0f);
                if (rand <= 0.2f && floor >= 4)
                {
                    nodeType = NodeType.Elite;
                }
                else
                {
                    nodeType = NodeType.NormalBattle;
                }
            }
            else if(0.6f < rand && rand <= 0.8f)
            {
                nodeType = NodeType.Shop;
            }
            else
            {
                nodeType = NodeType.Rest;
            }
        }

        if (_spRenderer == null)
            _spRenderer = GetComponentInChildren<SpriteRenderer>();
        _spRenderer.sprite = nodeImage[(int)nodeType];
    }

    void SizeControll(bool isUp)
    {
        if(isUp)
        {
            if(maxSize > curSize.x)
            {
                curSize.x += Time.deltaTime * changeSpeed;
                curSize.y += Time.deltaTime * changeSpeed;
                _spRenderer.transform.localScale = curSize;
            }
        }
        else
        {
            if (minSize < curSize.x)
            {
                curSize.x -= Time.deltaTime * changeSpeed;
                curSize.y -= Time.deltaTime * changeSpeed;
                _spRenderer.transform.localScale = curSize;
            }
        }
    }

    void ChangeMoveableColor()
    {
        if(nodeState == NodeState.Moveable)
        {
            _curColor = _spRenderer.color;
            if(!increaseColor)
            {
                _curColor = Color.Lerp(_curColor, changeColor, colorChangeSpeed * Time.deltaTime);

                //_curColor.a -= Time.deltaTime * colorChangeSpeed;
                _spRenderer.color = _curColor;

                if (_curColor.r  <= changeColor.r + .01f)
                    increaseColor = true;

                //if (_curColor.a <= 0.0f)
                //    increaseColor = true;
            }
            else
            {
                _curColor = Color.Lerp(_curColor, Color.white, colorChangeSpeed * Time.deltaTime);

                //_curColor.a += Time.deltaTime * colorChangeSpeed;
                _spRenderer.color = _curColor;

                if (_curColor.r + .01f >= 1.0f)
                    increaseColor = false;
                //if (_curColor.a >= 1.0f)
                //    increaseColor = false;
            }
        }
    }
}