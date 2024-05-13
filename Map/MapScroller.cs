using UnityEngine;

public class MapScroller : MonoBehaviour
{
    private Camera mainCam;
    private Vector3 pickPos;

    public float minX;
    public float maxX;

    public bool widthMode = true;

    bool drag = false;

    private void Awake()
    {
        mainCam = Camera.main;
    }

    //private void Start() => StartFunc();

    //private void StartFunc()
    //{
         
    //}

    private void Update() => UpdateFunc();

    private void UpdateFunc()
    {
        if (GameManager.inst.pause)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            pickPos = -transform.position + ScreenToMousePos();
            drag = true;
        }

        if (Input.GetMouseButtonUp(0))
            drag = false;

        scrollMap();
        BackOrigin();
    }

    public void scrollMap()
    {
        if (!drag)
            return;

        Vector3 nowPos = ScreenToMousePos();

        this.transform.position = new Vector3(nowPos.x - pickPos.x,
                                              this.transform.position.y,
                                              this.transform.position.z);
    }

    public Vector3 ScreenToMousePos()
    {
        Vector3 pos = mainCam.ScreenToWorldPoint(Input.mousePosition);
        pos.z = 0;

        return pos;
    }

    public void BackOrigin()
    {
        if (drag)
            return;

        if (transform.localPosition.x < maxX && transform.localPosition.x > minX)
            return;

        if (transform.localPosition.x > maxX)
            this.transform.position = Vector3.Lerp(this.transform.position, new Vector3(maxX, this.transform.position.y, this.transform.position.z), 0.05f);
        else if(transform.localPosition.x < minX)
            this.transform.localPosition = Vector3.Lerp(this.transform.position, new Vector3(minX, this.transform.position.y, this.transform.position.z), 0.05f);

    }
}