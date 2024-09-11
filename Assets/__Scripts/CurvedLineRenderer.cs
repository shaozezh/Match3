using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CurvedLineRenderer : MonoBehaviour
{
    public Transform startPoint;
    public Transform[] destinations;
    public int curveResolution = 20;
    public float curveHeightMultiplier = .5f;
    public LineRenderer preconfiguredLineRenderer;
    public float moveSpeed = 5.0f;
    public GameObject starPrefab;
    private List<GameObject> stars = new List<GameObject>();
    //private GameObject star;
    public IEnumerator CurvesMove(Vector2 start, GameObject endBlock, bool isBonus)
    {
        Vector2[] curvePoints = DrawCurve(start, endBlock.transform.position);
        GameObject star = Instantiate(starPrefab, curvePoints[0], Quaternion.identity, transform);
        Transform prefabRectTransform = star.GetComponent<Transform>();
        yield return StartCoroutine(MoveUIAlongCurve(prefabRectTransform, curvePoints, endBlock, star, isBonus));
        //stars.Add(instantiatedPrefab);
    }

    public void DestroyStar(GameObject star)
    {
        /*foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }*/
        star.SetActive(false);
    }
    public void DestroyAllStars()
    {
        foreach (GameObject star in stars)
        {
            star.SetActive(false);
        }
    }

    private IEnumerator MoveUIAlongCurve(Transform uiImage, Vector2[] curvePoints, GameObject endBlock, GameObject star, bool isBonus)
    {
        int pointIndex = 0;

        while (pointIndex < curvePoints.Length - 1)
        {
            Vector2 startPosition = curvePoints[pointIndex];
            Vector2 endPosition = curvePoints[pointIndex + 1];
            float journeyLength = Vector2.Distance(startPosition, endPosition);
            float startTime = Time.time;
            //Vector3 direction = (endPosition - startPosition).normalized;
        
        // Calculate the angle required for rotation around the Z-axis
        //float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            while (Vector2.Distance(uiImage.position, endPosition) > 0.01f)
            {
                // Calculate the fraction of journey completed
                float distCovered = (Time.time - startTime) * moveSpeed;
                float fractionOfJourney = distCovered / journeyLength;

                // Smooth interpolation along the curve segment
                uiImage.position = Vector2.Lerp(startPosition, endPosition, fractionOfJourney);
                //uiImage.rotation = Quaternion.Euler(0, 0, angle);

                // Wait for the next frame
                yield return null;
            }

            pointIndex++;
        }

        // Ensure the UI Image reaches the final point
        uiImage.position = curvePoints[curvePoints.Length - 1];
        star.GetComponent<Animator>().SetTrigger("Fade");
        if (isBonus)
        {
            int randomIndex = Random.Range(0, 3);
            if (randomIndex == 0)
            {
                endBlock.GetComponent<Block>().MakeColBomb();
            }
            else if (randomIndex == 1)
            {
                endBlock.GetComponent<Block>().MakeRowBomb();
            }
            else
            {
                endBlock.GetComponent<Block>().MakeSquareBomb();
            }
            DestroyStar(star);
        }
        else
        {
            stars.Add(star);
        }
        

    }

    public Vector2[] DrawCurve(Vector2 start, Vector2 end)
    {
        GameObject lineObj = new GameObject("CurvedLine");
        LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
        //Vector3 tempStart = new Vector3(start.x, start.y, 0);
        //Vector3 tempEnd = new Vector3(end.x, end.y, 0);
        //lineRenderer = preconfiguredLineRenderer;
        //lineRenderer.positionCount = curveResolution + 1;
        lineRenderer.material = preconfiguredLineRenderer.material;
        lineRenderer.widthCurve = preconfiguredLineRenderer.widthCurve;
        lineRenderer.colorGradient = preconfiguredLineRenderer.colorGradient;
        lineRenderer.positionCount = curveResolution + 1;
        float xDifference = end.x - start.x;
        float dynamicCurveHeight = xDifference * curveHeightMultiplier;
        Vector2 controlPoint = start + Vector2.right * dynamicCurveHeight;
        Vector2[] curvePoints = new Vector2[curveResolution + 1];
        for (int i = 0; i <= curveResolution; i++)
        {
            float t = i / (float)curveResolution;
            Vector2 curvePoint = CalculateQuadraticBezierPoint(t, start, controlPoint, end);
            lineRenderer.SetPosition(i, curvePoint);
            curvePoints[i] = curvePoint;
        }

        return curvePoints;
    }

    Vector2 CalculateQuadraticBezierPoint(float t, Vector2 p0, Vector2 p1, Vector2 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;

        Vector2 p = uu * p0; // (1-t)^2 * p0
        p += 2 * u * t * p1; // 2(1-t)t * p1
        p += tt * p2; // t^2 * p2

        return p;
    }
}
