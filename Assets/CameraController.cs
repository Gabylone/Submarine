using UnityEngine;
using Cinemachine;

public class CameraController : MonoBehaviour {
    private Camera cam;
    private CinemachineVirtualCamera virtualCam;
    private CinemachineBrain brain;
    public CinemachineTransposer transposer;
    public CinemachineComposer composer;

    public float minY = 0f;
    public float maxY = 0f;
    public float maxX = 0f;
    public float maxZ = 0f;
    public float decal;

    public Transform composerTarget;
    public Transform playerTarget;

    // Start is called before the first frame update
    void Start() {
        virtualCam = GetComponent<CinemachineVirtualCamera>();
        brain = GetComponent<CinemachineBrain>();
        transposer = virtualCam.GetCinemachineComponent<CinemachineTransposer>();
        composer = virtualCam.GetCinemachineComponent<CinemachineComposer>();
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.T))
            randomizeTransposer();

        if (Input.GetKeyDown(KeyCode.C))
            randomizeComposer();

        composer.m_TrackedObjectOffset = composerTarget.position;
        var d = (composerTarget.position - playerTarget.position).normalized;
        d.y = 0F;
        var p = -d * decal;
        Vector3 offset = transform.TransformDirection(Vector3.right * maxX);
        transposer.m_FollowOffset = p + offset + Vector3.up * maxY;
    }

    private void randomizeComposer() {
    }

    private void randomizeTransposer() {
        float x = Random.Range(-maxX, maxX);
        float y = Random.Range(minY, maxY);
        float z = Random.Range(-maxZ, maxZ);
        Vector3 offset = new Vector3(x, y, z);
        transposer.m_FollowOffset = offset;
    }
}
