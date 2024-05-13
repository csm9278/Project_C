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
            int rand = Random.Range(mapNodeCount - 1, mapNodeCount + 1);    //������ �� ī��Ʈ ������ ����
            mapInterval = (maxY + (-minY)) / rand;
            List<MapNode> refNodes = new List<MapNode>();   //���� ������ ������ ����Ʈ

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
                if (i == 0)  //ó�� ����
                    node.SetNode(MapNode.NodeState.Moveable, i);
                else
                    node.SetNode(MapNode.NodeState.Lock, i);

                refNodes.Add(node);
            }

            nodeList.Add(refNodes); //�� ���� ��� ����
        }

        //���� ��� ����
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



        // �Ϲ� �� ������ ����
        bool isDoubleRoot = false;
        for (int i = 0; i < nodeList.Count - 2; i++)
        {
            isDoubleRoot = false;
            if (nodeList[i].Count == nodeList[i + 1].Count) //�������� ��� �� ����
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //�ϴ� ������ ���� 

                    if (!isDoubleRoot)  // ������Ʈ ����
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% Ȯ���� ���� ��忡�� 2���� ������ ������ ��
                        {
                            if (j == 0)  // �� ���� ����� ��� �Ʒ��� ��ĭ
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i].Count) //�� �Ʒ��� ����� ��� ���� ��ĭ 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            } 
                            else    //�� �̿��� ���� �� Ȥ�� �Ʒ��� ��ĭ
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
            else if (nodeList[i].Count > nodeList[i + 1].Count) // ���������� ������ ���� ��
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    if (j < nodeList[i + 1].Count)
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //�ϴ� ������ ���� 
                    }

                    if (j >= nodeList[i + 1].Count) //������ ��忡�� �� ������ �߰�
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                        break;
                    }

                    if (!isDoubleRoot)  // ������Ʈ ����
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% Ȯ���� ���� ��忡�� 2���� ������ ������ ��
                        {
                            if (j == 0)  // �� ���� ����� ��� �Ʒ��� ��ĭ
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i + 1].Count) //�� �Ʒ��� ����� ��� ���� ��ĭ 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            }
                            else    //�� �̿��� ���� �� Ȥ�� �Ʒ��� ��ĭ
                            {
                                Debug.Log("����");
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
            else    //���� �� ���� ������ ���� ��
            {
                for (int j = 0; j < nodeList[i].Count; j++)
                {
                    if (j < nodeList[i + 1].Count)
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j]); //�ϴ� ������ ���� 

                    if (j + 1 >= nodeList[i].Count) //������ ��忡�� �Ʒ� ������ �߰�
                    {
                        nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                        break;
                    }

                    if (!isDoubleRoot)  // ������Ʈ ����
                    {
                        float rand = Random.Range(0.0f, 1.0f);

                        if (rand <= 0.35f) // 35% Ȯ���� ���� ��忡�� 2���� ������ ������ ��
                        {
                            if (j == 0)  // �� ���� ����� ��� �Ʒ��� ��ĭ
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j + 1]);
                            }
                            else if (j + 1 >= nodeList[i].Count) //�� �Ʒ��� ����� ��� ���� ��ĭ 
                            {
                                nodeList[i][j].outMoveNode.Add(nodeList[i + 1][j - 1]);
                            }
                            else    //�� �̿��� ���� �� Ȥ�� �Ʒ��� ��ĭ
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

        // ������ �Ϲ� ��� ������� ����
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