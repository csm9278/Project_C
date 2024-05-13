using UnityEngine;

public class LineObject : MonoBehaviour
{
    LineRenderer lineRenderer;

    public void SetLine(Vector3 firstPos, Vector3 secondPos)
    {
        lineRenderer = GetComponent<LineRenderer>();

        for (var i = 0; i < lineRenderer.positionCount; i++)
        {
            lineRenderer.SetPosition(i,
                Vector3.Lerp(Vector3.zero, secondPos - firstPos, (float)i / (lineRenderer.positionCount - 1)));
        }

        //lineRenderer.SetPosition(0, firstPos);
        //lineRenderer.SetPosition(1, new Vector3(secondPos.x * 0.5f, secondPos.y * 0.5f, 0));
        //lineRenderer.SetPosition(2, secondPos);
    }
}