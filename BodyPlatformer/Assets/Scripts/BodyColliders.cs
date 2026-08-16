using UnityEngine;

public class BodyColliders : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;

    [Header("Edge Colliders")]
    [SerializeField] private float edgeRadius = 0.03f;

    [Header("Head Collider")]
    [SerializeField] private float headRadiusMultiplier = 1.15f;
    [SerializeField] private float minimumHeadRadius = 0.25f;

    private EdgeCollider2D upperBodyCollider;
    private EdgeCollider2D leftLegCollider;
    private EdgeCollider2D rightLegCollider;

    private CircleCollider2D headCollider;
    private GameObject headColliderObject;

    private void Start()
    {
        int bodyLayer = LayerMask.NameToLayer("Body");

        upperBodyCollider = CreateEdgeCollider(
            "UpperBodyCollider",
            bodyLayer
        );

        leftLegCollider = CreateEdgeCollider(
            "LeftLegCollider",
            bodyLayer
        );

        rightLegCollider = CreateEdgeCollider(
            "RightLegCollider",
            bodyLayer
        );

        CreateHeadCollider(bodyLayer);
    }

    private EdgeCollider2D CreateEdgeCollider(
        string objectName,
        int layer
    )
    {
        GameObject obj = new GameObject(objectName);

        obj.transform.SetParent(transform);
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        obj.layer = layer;

        EdgeCollider2D edge =
            obj.AddComponent<EdgeCollider2D>();

        edge.edgeRadius = edgeRadius;

        return edge;
    }

    private void CreateHeadCollider(int layer)
    {
        headColliderObject =
            new GameObject("HeadCollider");

        headColliderObject.transform.SetParent(transform);

        headColliderObject.transform.localPosition = Vector3.zero;
        headColliderObject.transform.localRotation = Quaternion.identity;
        headColliderObject.transform.localScale = Vector3.one;

        headColliderObject.layer = layer;

        headCollider =
            headColliderObject.AddComponent<CircleCollider2D>();
    }

    private void FixedUpdate()
    {
        if (poseReceiver == null)
            return;

        GameObject[] points =
            poseReceiver.GetLandmarkObjects();

        if (points == null || points.Length < 33)
            return;

        UpdateUpperBody(points);
        UpdateLeftLeg(points);
        UpdateRightLeg(points);
        UpdateHead(points);
    }

    private void UpdateUpperBody(GameObject[] points)
    {
        Vector2[] edgePoints =
        {
            GetLocalPoint(points[15], upperBodyCollider),
            GetLocalPoint(points[13], upperBodyCollider),
            GetLocalPoint(points[11], upperBodyCollider),
            GetLocalPoint(points[12], upperBodyCollider),
            GetLocalPoint(points[14], upperBodyCollider),
            GetLocalPoint(points[16], upperBodyCollider)
        };

        upperBodyCollider.points = edgePoints;
    }

    private void UpdateLeftLeg(GameObject[] points)
    {
        Vector2[] edgePoints =
        {
            GetLocalPoint(points[23], leftLegCollider),
            GetLocalPoint(points[25], leftLegCollider),
            GetLocalPoint(points[27], leftLegCollider)
        };

        leftLegCollider.points = edgePoints;
    }

    private void UpdateRightLeg(GameObject[] points)
    {
        Vector2[] edgePoints =
        {
            GetLocalPoint(points[24], rightLegCollider),
            GetLocalPoint(points[26], rightLegCollider),
            GetLocalPoint(points[28], rightLegCollider)
        };

        rightLegCollider.points = edgePoints;
    }

    private void UpdateHead(GameObject[] points)
    {
        // MediaPipe:
        // 0 = nariz
        // 7 = oreja izquierda
        // 8 = oreja derecha

        Vector3 nose =
            points[0].transform.position;

        Vector3 leftEar =
            points[7].transform.position;

        Vector3 rightEar =
            points[8].transform.position;

        Vector3 center =
            (leftEar + rightEar) / 2f;

        float headWidth =
            Vector2.Distance(leftEar, rightEar);

        float radius =
            Mathf.Max(
                minimumHeadRadius,
                headWidth * 0.5f * headRadiusMultiplier
            );

        headColliderObject.transform.position =
            new Vector3(
                center.x,
                center.y,
                0f
            );

        headCollider.offset = Vector2.zero;
        headCollider.radius = radius;
    }

    private Vector2 GetLocalPoint(
        GameObject landmark,
        EdgeCollider2D collider
    )
    {
        Vector3 worldPosition =
            landmark.transform.position;

        return collider.transform
            .InverseTransformPoint(worldPosition);
    }
}