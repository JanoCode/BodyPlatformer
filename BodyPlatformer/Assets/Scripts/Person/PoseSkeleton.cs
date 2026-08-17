using UnityEngine;

public class PoseSkeleton : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;

    [Header("Head")]
    [SerializeField] private float headHeightMultiplier = 0.55f;
    [SerializeField] private float headWidthMultiplier = 1.15f;

    [Header("Hands")]
    [SerializeField] private float handWidthMultiplier = 1.8f;
    [SerializeField] private float minimumHandWidth = 0.3f;

    private readonly int[,] bodyConnections =
    {
        {11, 12}, // hombros

        {11, 13}, // brazo izquierdo
        {13, 15},

        {12, 14}, // brazo derecho
        {14, 16},

        {23, 25}, // pierna izquierda
        {25, 27},

        {24, 26}, // pierna derecha
        {26, 28}
    };

    private LineRenderer[] bodyLines;

    private LineRenderer headLine;
    private LineRenderer leftHandLine;
    private LineRenderer rightHandLine;

    private void Start()
    {
        CreateBodyLines();

        headLine = CreateLine("HeadLine", 21);
        leftHandLine = CreateLine("LeftHandLine", 2);
        rightHandLine = CreateLine("RightHandLine", 2);
    }

    private void CreateBodyLines()
    {
        bodyLines =
            new LineRenderer[bodyConnections.GetLength(0)];

        for (int i = 0; i < bodyLines.Length; i++)
        {
            bodyLines[i] =
                CreateLine("BodyLine_" + i, 2);
        }
    }

    private LineRenderer CreateLine(
        string objectName,
        int positionCount
    )
    {
        GameObject lineObject =
            new GameObject(objectName);

        lineObject.transform.SetParent(transform);

        LineRenderer line =
            lineObject.AddComponent<LineRenderer>();

        line.positionCount = positionCount;
        line.startWidth = lineWidth;
        line.endWidth = lineWidth;

        line.useWorldSpace = true;

        if (lineMaterial != null)
        {
            line.material = lineMaterial;
        }

        line.sortingOrder = 5;

        return line;
    }

    private void Update()
    {
        if (poseReceiver == null)
            return;

        GameObject[] points =
            poseReceiver.GetLandmarkObjects();

        if (points == null ||
            points.Length < 33)
            return;

        UpdateBodyLines(points);
        UpdateHead(points);
        UpdateLeftHand(points);
        UpdateRightHand(points);
    }

    private void UpdateBodyLines(GameObject[] points)
    {
        for (int i = 0; i < bodyLines.Length; i++)
        {
            int startIndex =
                bodyConnections[i, 0];

            int endIndex =
                bodyConnections[i, 1];

            bool visible =
                poseReceiver.IsLandmarkVisible(startIndex) &&
                poseReceiver.IsLandmarkVisible(endIndex);

            bodyLines[i].enabled = visible;

            if (!visible)
                continue;

            bodyLines[i].SetPosition(
                0,
                points[startIndex].transform.position
            );

            bodyLines[i].SetPosition(
                1,
                points[endIndex].transform.position
            );
        }
    }

    private void UpdateHead(GameObject[] points)
    {
        bool visible =
            poseReceiver.IsLandmarkVisible(0) &&
            poseReceiver.IsLandmarkVisible(7) &&
            poseReceiver.IsLandmarkVisible(8);

        headLine.enabled = visible;

        if (!visible)
            return;

        Vector3 leftEar = points[7].transform.position;
        Vector3 rightEar = points[8].transform.position;

        Vector3 center =
            (leftEar + rightEar) * 0.5f;

        float width =
            Vector2.Distance(leftEar, rightEar) *
            headWidthMultiplier;

        float height =
            width * headHeightMultiplier;

        float radiusX = width * 0.5f;
        float radiusY = height;

        int segments = 20;

        headLine.positionCount = segments + 1;

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;

            // Va desde 180° hasta 0°
            float angle = Mathf.PI * (1f - t);

            float x =
                center.x + Mathf.Cos(angle) * radiusX;

            float y =
                center.y + Mathf.Sin(angle) * radiusY;

            headLine.SetPosition(
                i,
                new Vector3(x, y, 0f)
            );
        }
    }

    private void UpdateLeftHand(GameObject[] points)
    {
        bool visible =
            poseReceiver.IsLandmarkVisible(15) &&
            poseReceiver.IsLandmarkVisible(17) &&
            poseReceiver.IsLandmarkVisible(19);

        leftHandLine.enabled = visible;

        if (!visible)
            return;

        UpdateHandLine(
            points[15],
            points[17],
            points[19],
            leftHandLine
        );
    }

    private void UpdateRightHand(GameObject[] points)
    {
        bool visible =
            poseReceiver.IsLandmarkVisible(16) &&
            poseReceiver.IsLandmarkVisible(18) &&
            poseReceiver.IsLandmarkVisible(20);

        rightHandLine.enabled = visible;

        if (!visible)
            return;

        UpdateHandLine(
            points[16],
            points[18],
            points[20],
            rightHandLine
        );
    }

    private void UpdateHandLine(
        GameObject wristObject,
        GameObject pinkyObject,
        GameObject indexObject,
        LineRenderer line
    )
    {
        Vector3 wrist =
            wristObject.transform.position;

        Vector3 pinky =
            pinkyObject.transform.position;

        Vector3 index =
            indexObject.transform.position;

        Vector3 fingerCenter =
            (pinky + index) * 0.5f;

        Vector2 handDirection =
            fingerCenter - wrist;

        if (handDirection.sqrMagnitude < 0.0001f)
        {
            handDirection = Vector2.up;
        }

        handDirection.Normalize();

        Vector2 sideDirection =
            new Vector2(
                -handDirection.y,
                handDirection.x
            );

        float detectedWidth =
            Vector2.Distance(pinky, index);

        float width =
            Mathf.Max(
                minimumHandWidth,
                detectedWidth * handWidthMultiplier
            );

        Vector3 center =
            Vector3.Lerp(
                wrist,
                fingerCenter,
                0.55f
            );

        Vector3 left =
            center -
            (Vector3)(sideDirection * width * 0.5f);

        Vector3 right =
            center +
            (Vector3)(sideDirection * width * 0.5f);

        line.SetPosition(0, left);
        line.SetPosition(1, right);
    }
}