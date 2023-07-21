using UnityEngine;

public class CameraTests : MonoBehaviour
{
    public Transform direction_Transform;
    public Transform parent_Transform;

    public Transform cam;
    public Transform sub;

    public float turnSpeed = 85f;

    public float ray_distance = 1f;
    public float ray_height = 1f;
    public LayerMask ray_LayerMask;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        cam.RotateAround(sub.position, Vector3.up, turnSpeed * Time.deltaTime);

        if (PressInput())
        {
            //Vector3 dir = cam.TransformDirection(GetInputDirection());

            Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);
            Vector3 vp_point = Camera.main.WorldToViewportPoint(direction_Transform.position) + (inputDir * ray_distance);

            Ray ray = Camera.main.ViewportPointToRay(vp_point);

            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, ray_height, ray_LayerMask))
            {
                parent_Transform.rotation = Quaternion.identity;
                direction_Transform.forward = hit.point - direction_Transform.position;
                direction_Transform.LookAt(hit.point, sub.up);
            }
        }


    }
    public bool PressInput()
    {
        return Input.GetAxis("Horizontal") != 0
            || Input.GetAxis("Vertical") != 0;
    }

    public Vector3 GetInputDirection()
    {
        return new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;

        Vector3 inputDir = new Vector3(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"), 0f);

        Vector3 vp_point = Camera.main.WorldToViewportPoint(direction_Transform.position) + (inputDir * ray_distance);

        Ray ray = Camera.main.ViewportPointToRay(vp_point);

        Gizmos.DrawRay(ray);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, ray_height, ray_LayerMask))
        {
            Gizmos.DrawSphere(hit.point, 0.01f);
        }

    }
}
