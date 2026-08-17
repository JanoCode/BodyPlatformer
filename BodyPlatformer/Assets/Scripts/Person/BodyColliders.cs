using UnityEngine;

public class BodyColliders : MonoBehaviour
{
    [SerializeField] private PoseReceiver poseReceiver;

    [Header("Edge Colliders")]
    [SerializeField] private float edgeRadius = 0.03f;

    [Header("Head")]
    [SerializeField] private float headHeightMultiplier = 0.55f;
    [SerializeField] private float headWidthMultiplier = 1.15f;

    [Header("Hands")]
    [SerializeField] private float handWidthMultiplier = 1.8f;
    [SerializeField] private float minimumHandWidth = 0.3f;

    private EdgeCollider2D leftArmCollider;
    private EdgeCollider2D shouldersCollider;
    private EdgeCollider2D rightArmCollider;

    private EdgeCollider2D leftLegCollider;
    private EdgeCollider2D rightLegCollider;

    private EdgeCollider2D headCollider;

    private EdgeCollider2D leftHandCollider;
    private EdgeCollider2D rightHandCollider;

    private void Start()
    {
        int bodyLayer = LayerMask.NameToLayer("Body");

        leftArmCollider =
            CreateEdgeCollider("LeftArmCollider", bodyLayer);

        shouldersCollider =
            CreateEdgeCollider("ShouldersCollider", bodyLayer);

        rightArmCollider =
            CreateEdgeCollider("RightArmCollider", bodyLayer);

        leftLegCollider =
            CreateEdgeCollider("LeftLegCollider", bodyLayer);

        rightLegCollider =
            CreateEdgeCollider("RightLegCollider", bodyLayer);

        headCollider =
            CreateEdgeCollider("HeadCollider", bodyLayer);

        leftHandCollider =
            CreateEdgeCollider("LeftHandCollider", bodyLayer);

        rightHandCollider =
            CreateEdgeCollider("RightHandCollider", bodyLayer);
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

    private void FixedUpdate()
    {
        if (poseReceiver == null)
            return;

        if (!poseReceiver.IsPersonDetected())
        {
            DisableAllColliders();
            return;
        }

        GameObject[] points =
            poseReceiver.GetLandmarkObjects();

        if (points == null || points.Length < 33)
        {
            DisableAllColliders();
            return;
        }

        UpdateLeftArm(points);
        UpdateShoulders(points);
        UpdateRightArm(points);

        UpdateLeftLeg(points);
        UpdateRightLeg(points);

        UpdateHead(points);

        UpdateLeftHand(points);
        UpdateRightHand(points);
    }

    private void DisableAllColliders()
    {
        if (leftArmCollider != null)
            leftArmCollider.enabled = false;

        if (shouldersCollider != null)
            shouldersCollider.enabled = false;

        if (rightArmCollider != null)
            rightArmCollider.enabled = false;

        if (leftLegCollider != null)
            leftLegCollider.enabled = false;

        if (rightLegCollider != null)
            rightLegCollider.enabled = false;

        if (headCollider != null)
            headCollider.enabled = false;

        if (leftHandCollider != null)
            leftHandCollider.enabled = false;

        if (rightHandCollider != null)
            rightHandCollider.enabled = false;
    }

    private bool AreVisible(params int[] indexes)
    {
        foreach (int index in indexes)
        {
            if (!poseReceiver.IsLandmarkVisible(index))
                return false;
        }

        return true;
    }

    private void UpdateLeftArm(GameObject[] points)
    {
        bool visible = AreVisible(15, 13, 11);

        leftArmCollider.enabled = visible;

        if (!visible)
            return;

        leftArmCollider.points = new Vector2[]
        {
            GetLocalPoint(points[15], leftArmCollider),
            GetLocalPoint(points[13], leftArmCollider),
            GetLocalPoint(points[11], leftArmCollider)
        };
    }

    private void UpdateShoulders(GameObject[] points)
    {
        bool visible = AreVisible(11, 12);

        shouldersCollider.enabled = visible;

        if (!visible)
            return;

        shouldersCollider.points = new Vector2[]
        {
            GetLocalPoint(points[11], shouldersCollider),
            GetLocalPoint(points[12], shouldersCollider)
        };
    }

    private void UpdateRightArm(GameObject[] points)
    {
        bool visible = AreVisible(12, 14, 16);

        rightArmCollider.enabled = visible;

        if (!visible)
            return;

        rightArmCollider.points = new Vector2[]
        {
            GetLocalPoint(points[12], rightArmCollider),
            GetLocalPoint(points[14], rightArmCollider),
            GetLocalPoint(points[16], rightArmCollider)
        };
    }

    private void UpdateLeftLeg(GameObject[] points)
    {
        bool visible = AreVisible(23, 25, 27);

        leftLegCollider.enabled = visible;

        if (!visible)
            return;

        leftLegCollider.points = new Vector2[]
        {
            GetLocalPoint(points[23], leftLegCollider),
            GetLocalPoint(points[25], leftLegCollider),
            GetLocalPoint(points[27], leftLegCollider)
        };
    }

    private void UpdateRightLeg(GameObject[] points)
    {
        bool visible = AreVisible(24, 26, 28);

        rightLegCollider.enabled = visible;

        if (!visible)
            return;

        rightLegCollider.points = new Vector2[]
        {
            GetLocalPoint(points[24], rightLegCollider),
            GetLocalPoint(points[26], rightLegCollider),
            GetLocalPoint(points[28], rightLegCollider)
        };
    }

    private void UpdateHead(GameObject[] points)
    {
        bool visible = AreVisible(0, 7, 8);

        headCollider.enabled = visible;

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

        Vector2[] edgePoints =
            new Vector2[segments + 1];

        for (int i = 0; i <= segments; i++)
        {
            float t = (float)i / segments;

            float angle =
                Mathf.PI * (1f - t);

            Vector3 worldPoint =
                new Vector3(
                    center.x + Mathf.Cos(angle) * radiusX,
                    center.y + Mathf.Sin(angle) * radiusY,
                    0f
                );

            edgePoints[i] =
                headCollider.transform
                    .InverseTransformPoint(worldPoint);
        }

        headCollider.points = edgePoints;
    }

    private void UpdateLeftHand(GameObject[] points)
    {
        bool visible = AreVisible(15, 17, 19);

        leftHandCollider.enabled = visible;

        if (!visible)
            return;

        UpdateHandCollider(
            points[15],
            points[17],
            points[19],
            leftHandCollider
        );
    }

    private void UpdateRightHand(GameObject[] points)
    {
        bool visible = AreVisible(16, 18, 20);

        rightHandCollider.enabled = visible;

        if (!visible)
            return;

        UpdateHandCollider(
            points[16],
            points[18],
            points[20],
            rightHandCollider
        );
    }

    private void UpdateHandCollider(
        GameObject wristObject,
        GameObject pinkyObject,
        GameObject indexObject,
        EdgeCollider2D handCollider
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

        // El Transform sigue la mano para funcionar
        // correctamente como plataforma móvil.
        handCollider.transform.position = center;

        Vector2 left =
            -sideDirection * width * 0.5f;

        Vector2 right =
            sideDirection * width * 0.5f;

        handCollider.points = new Vector2[]
        {
            left,
            right
        };
    }

    private Vector2 GetLocalPoint(
        GameObject landmark,
        EdgeCollider2D collider
    )
    {
        return collider.transform
            .InverseTransformPoint(
                landmark.transform.position
            );
    }
}