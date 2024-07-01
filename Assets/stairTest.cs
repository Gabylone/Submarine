using UnityEngine;

public class stairTest : MonoBehaviour {
    public Transform start_left;
    public Transform start_right;
    public Transform end_left;
    public Transform end_right;
    public float height = 1.0f;
    private void OnDrawGizmos() {
        /*Vector3 start_sdir = start_right.position - start_left.position;
        Vector3 end_mid = end_left.position + (end_right.position - end_left.position)/ 2f;
        Vector3 start_mid = start_left.position + (start_sdir) / 2f;
        Vector3 dir = end_mid - start_mid;
        Vector3 normal = Vector3.Cross(start_sdir, Vector3.up);
        float dot = Vector3.Dot(normal.normalized, dir.normalized);
        float r = start_sdir.magnitude * dot;
        Vector3 check_start = start_mid + (dir.normalized * ((r / 2f) + (1 - dot)));
        Vector3 check_end = end_mid - (dir.normalized * ((r / 2f) + (1 - dot)));
        Vector3 tmpDir = (check_end - check_start);
        Vector3 pos = check_start + tmpDir/2f;
        float w = start_sdir.magnitude * dot;
        Vector3 scale = new Vector3(w, height, tmpDir.magnitude);
        Quaternion orientation = Quaternion.LookRotation(dir);

        bool _blocked = Physics.CheckBox(pos, scale / 2f, orientation, LayerMask.GetMask("Bridge"));
        Gizmos.color = _blocked ? Color.red : Color.green;
        Gizmos.matrix = Matrix4x4.TRS(pos, orientation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, scale);*/
    }
}
