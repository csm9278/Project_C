using System.Collections.Generic;
using UnityEngine;

public class MapCreater : MonoBehaviour
{
    public GameObject mapNode;
    public int mapMaxSize = 2;
    public float mapInterval;
    public int mapNodeCount = 4;
    MapScroller mapScroller;

    //HighObject
    GameObject nodeParent;

    //NodeLine
    List<List<MapNode>> nodeList = new List<List<MapNode>>();
    public GameObject lineObject;

    public float maxY;
    public float minY;

    private void Start() => StartFunc();

    private void StartFunc()
    {
        mapScroller = GetComponent<MapScroller>();
        CreateNewMap();

        SetLine();
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.DownArrow))
        {
            nodeList.Clear();

            Transform tr = this.transform;

            foreach (Transform child in tr)
                Destroy(child.gameObject);

            CreateNewMap();
            SetLine();
        }
    }

    void CreateNewMap()
    {
        nodeParent = new GameObject();
        nodeParent.name = "nodeParent";
        this.gameObject.transform.position = Vector3.zero;
        nodeParent.transform.parent = this.gameObject.transform;
        nodeParent.transform.position = Vector2.zero;

        for (int i = 0; i < mapMaxSize; i++)
        {
            float firstY = maxY;
            int rand = Random.Range(mapNodeCount - 1, mapNodeCount + 1);    //랜덤한 맵 카운트 개수를 위해
            mapInterval = (maxY + (-minY)) / rand;
            List<MapNode> refNodes = new List<MapNode>();   //층의 노드들을 저장할 리스트

            for (int j = 0; j < rand; j++)
            {
                GameObject obj = Instantiate(mapNode);
                MapNode node = obj.GetComponent<MapNode>();
                node.mapCreater = this;

                //obj Setting
                obj.transform.parent = nodeParent.transform;
                obj.name = i + "Floor " + j + "th Node";
                float randomY = Random.Range(firstY + 0.5f, firstY - 0.5f);
                obj.transform.position = new Vector2(i + (2 * i), randomY);
                firstY -= mapInterval;

                //Node Setting
                if (i == 0)  //처음 노드들
                    node.SetNode(MapNode.NodeState.Moveable, i);
                else
                    node.SetNode(MapNode.NodeState.Lock, i);

                refNodes.Add(node);
            }

            nodeList.Add(refNodes); //각 층별 노드 저장
        }

        //보스 노드 생성
        List<MapNode> bossNodes = new List<MapNode>();
        GameObject bossobj = Instantiate(mapNode);
        MapNode bossNode = bossobj.GetComponent<MapNode>();
        bossNode.mapCreater = this;

        //obj Setting
        bossobj.transform.parent = nodeParent.transform;
        bossobj.name = nodeList.Count + " Boss Node";
        Vector3 lastNodePos = nodeList[mapMaxSize - 1][0].transform.position;
        lastNodePos.y = 0.5f;
        lastNodePos.x += 5.0f;
        bossobj.transform.position = lastNodePos;
        bossobj.transform.localScale = new Vector2(2.0f, 2.0f);

        bossNode.SetNode(MapNode.NodeState.Lock, mapMaxSize);

        bossNodes.Add(bossNode);
        nodeList.Add(bossNodes);



        // 일반 맵 선택지 세팅
        bool isDoubleRoot = false;
        for (int i = 0; i < nodeList.Count - 2; i++)
        {
            isDoubleRoot = false;
            if (nodeList[i].Count == nodeList[i + 1].Count) //다음층과 노드 수 동일
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //일단 일직선 연결 

                    if (!isDoubleRoot)  // 랜덤루트 생성
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% 확률과 전에 노드에서 2번의 제작을 안했을 시
                        {
                            if (j == 0)  // 맨 위쪽 노드인 경우 아래로 한칸
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i].Count) //맨 아래쪽 노드인 경우 위로 한칸 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            } 
                            else    //그 이외의 경우는 위 혹은 아래로 한칸
                            {
                                int upDown = Random.Range(0, 2);
                                if (upDown == 0)
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                                else
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);

                            }
                            isDoubleRoot = true;
                        }
                    }
                    else
                        isDoubleRoot = false;
                }
                
            }
            else if (nodeList[i].Count > nodeList[i + 1].Count) // 다음층보다 노드수가 많을 때
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    if (j < nodeList[i + 1].Count)
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //일단 일직선 연결 
                    }

                    if (j >= nodeList[i + 1].Count) //마지막 노드에서 위 선택지 추가
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                        break;
                    }

                    if (!isDoubleRoot)  // 랜덤루트 생성
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% 확률과 전에 노드에서 2번의 제작을 안했을 시
                        {
                            if (j == 0)  // 맨 위쪽 노드인 경우 아래로 한칸
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i + 1].Count) //맨 아래쪽 노드인 경우 위로 한칸 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            }
                            else    //그 이외의 경우는 위 혹은 아래로 한칸
                            {
                                Debug.Log("랜덤");
                                int upDown = Random.Range(0, 2);
                                if (upDown == 0)
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                                else
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);

                            }
                            isDoubleRoot = true;
                        }
                    }
                    else
                        isDoubleRoot = false;


                }
            }
            else    //다음 층 보다 노드수가 적을 때
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    if (j < nodeList[i + 1].Count)
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //일단 일직선 연결 

                    if (j + 1 >= nodeList[i].Count) //마지막 노드에서 아래 선택지 추가
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                        break;
                    }

                    if (!isDoubleRoot)  // 랜덤루트 생성
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% 확률과 전에 노드에서 2번의 제작을 안했을 시
                        {
                            if (j == 0)  // 맨 위쪽 노드인 경우 아래로 한칸
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i].Count) //맨 아래쪽 노드인 경우 위로 한칸 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            }
                            else    //그 이외의 경우는 위 혹은 아래로 한칸
                            {
                                int upDown = Random.Range(0, 2);
                                if (upDown == 0)
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                                else
                                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);

                            }
                            isDoubleRoot = true;
                        }
                    }
                    else
                        isDoubleRoot = false;
                }
            }
        }

        // 마지막 일반 노드 보스노드 연결
        for (int i = 0; i < nodeList[mapMaxSize - 1].Count; i++)
        {
            nodeList[mapMaxSize - 1][i].outMoveNode.Add(nodeList[mapMaxSize][0]);
        }

        mapScroller.minX = -(bossobj.transform.position.x - 5.0f);
        mapScroller.maxX = nodeList[0][0].transform.position.x - 5.0f;

    }

    public void SetLine()
    {
        for (int i = 0; i < nodeList.Count - 1; i++)
        {
            for (int j = 0; j < nodeList[i].Count; j++)
            {
                foreach(var outgoing in nodeList[i][j].GetComponent<MapNode>().outMoveNode)
                {
                    GameObject lineObj = Instantiate(lineObject);
                    lineObj.name = "TestLine";
                    lineObj.transform.parent = nodeParent.transform;
                    lineObj.transform.position = nodeList[i][j].transform.position;

                    if (lineObj.TryGetComponent(out LineObject line))
                        line.SetLine(nodeList[i][j].transform.localPosition, outgoing.transform.localPosition);
                }
            }
        }

    }

    public void SetLockMap(int floor)
    {
        for(int i = 0; i < nodeList[floor].Count; i++)
        {
            nodeList[floor][i].NodeLock();
        }
    }
}