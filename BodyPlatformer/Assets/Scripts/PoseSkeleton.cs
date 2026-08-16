using UnityEngine;

public class PoseSkeleton : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;
    [SerializeField] private Material lineMaterial;
    [SerializeField] private float lineWidth = 0.05f;

    private readonly int[,] connections =
    {
        {11, 12}, // hombros

        {11, 13}, // brazo izquierdo
        {13, 15},

        {12, 14}, // brazo derecho
        {14, 16},

        {11, 23}, // torso izquierdo
        {12, 24}, // torso derecho
        {23, 24}, // cadera

        {23, 25}, // pierna izquierda
        {25, 27},

        {24, 26}, // pierna derecha
        {26, 28}
    };

    private LineRenderer[] lines;

    void Start()
    {
        lines = new LineRenderer[connections.GetLength(0)];

        for (int i = 0; i < lines.Length; i++)
        {
            GameObject lineObject = new GameObject("BodyLine_" + i);
            lineObject.transform.parent = transform;

            LineRenderer line = lineObject.AddComponent<LineRenderer>();

            line.positionCount = 2;
            line.startWidth = lineWidth;
            line.endWidth = lineWidth;

            if (lineMaterial != null)
            {
                line.material = lineMaterial;
            }

            line.sortingOrder = 5;

            lines[i] = line;
        }
    }

    void Update()
    {
        if (poseReceiver == null)
            return;

        GameObject[] points = poseReceiver.GetLandmarkObjects();

        if (points == null || points.Length < 33)
            return;

        for (int i = 0; i < lines.Length; i++)
        {
            int startIndex = connections[i, 0];
            int endIndex = connections[i, 1];

            if (points[startIndex] == null || points[endIndex] == null)
                continue;

            lines[i].SetPosition(
                0,
                points[startIndex].transform.position
            );

            lines[i].SetPosition(
                1,
                points[endIndex].transform.position
            );
        }
    }
}