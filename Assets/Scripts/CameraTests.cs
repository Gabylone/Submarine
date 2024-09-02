using JetBrains.Annotations;
using System.Threading;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;
using UnityEngine.Windows.WebCam;

public class CameraTests : MonoBehaviour {

    public Transform anchor;

    public Transform target;

    public Vector3 offset;

    public Transform pivot_LookAt;
    public Transform follow_LookAt;

    public float position_speed = 1f;
    public float rotation_speed = 1f;

    public float dunno;

    Vector3 _nearestPoint;

    public enum CamType {
        Pivot,
        Follow,
    }

    public CamType type;
    public Transform parent;

    public SplineContainer spline;
    public float splineRatio = 0f;

    public float distanceToPlayer = 3f;

    // Start is called before the first frame update
    void Start() {

        var trs = parent.GetComponentsInChildren<Transform>();

        spline.Spline.Clear();
        for (int i = 1; i < trs.Length; i++) {
            var p = trs[i].position;
            var knot = new BezierKnot(new Unity.Mathematics.float3(p.x, p.y, p.z));
            spline.Spline.Add(knot);
        }
    }

    // Update is called once per frame
    void Update() {
        if ( type == CamType.Pivot ) {
            var direction = Player.Instance.GetTransform.position - anchor.position;
            anchor.rotation = Quaternion.LookRotation(direction.normalized, RotationRef.Instance.GetUpDirection());

            var targetPos = anchor.position - anchor.TransformDirection(offset);
            transform.position = Vector3.Lerp(transform.position, targetPos, position_speed * Time.deltaTime);
            transform.forward = Vector3.Lerp(transform.forward, Player.Instance.transform.position - transform.position, rotation_speed * Time.deltaTime);
        } else {

            var playerPosition = Player.Instance.GetTransform.position;
            var p = new float3 (playerPosition.x, playerPosition.y, playerPosition.z);
            var dir = Player.Instance.GetTransform.position - transform.position;


            float3 nearestPoint;
            SplineUtility.GetNearestPoint(spline.Spline, p, out nearestPoint, out splineRatio);

            _nearestPoint = new Vector3(nearestPoint.x, nearestPoint.y, nearestPoint.z);

            var splinePos = spline.Spline.EvaluatePosition(splineRatio);
            var pos = new Vector3(splinePos.x, splinePos.y, splinePos.z);

            transform.position = Vector3.Lerp(transform.position, pos, position_speed * Time.deltaTime);
            transform.forward = Vector3.Lerp(transform.forward, follow_LookAt.position - transform.position, rotation_speed * Time.deltaTime);
        }
    }

    private void OnDrawGizmos() {

        var trs = parent.GetComponentsInChildren<Transform>();
        Gizmos.DrawSphere(_nearestPoint, 0.5f);
        for (int i = 1; i < trs.Length; i++) {
            Gizmos.DrawSphere(trs[i].position, 0.1f);
            if ( i < trs.Length - 1 ) {
                Gizmos.DrawLine(trs[i].position, trs[(i + 1) % trs.Length].position);
            }
        }

    }
}
