using UnityEngine;

public struct Bridge {
    public class Side {
        public Color color = Color.white;
        public bool valid = true;
        public Side end;
        public bool link = false;
        public bool used = false;
        public bool blocked = false;

        public Side(Vector3 left, Vector3 right) {
            this.left = left;
            this.right = right;
        }

        public Vector3 left;
        public Vector3 right;

        public Vector3 mid {
            get {
                return left + (right - left) / 2f;
            }
        }

        public Vector3 dir {
            get {
                return (end.mid - mid).normalized;
            }
        }

        public Vector3 ldir {
            get {
                return end.mid - mid;
            }
        }

        public Vector3 sdir {
            get {
                return (right - left).normalized;
            }
        }

        public Vector3 lsdir {
            get {
                return right - left;
            }
        }
        public void Build() {
            if (end == null || end.used || used)
                return;

            // check if no bridge is in the way
            // with a physics check box
            float checkHeight = 1.5f;
            float pDot = Vector3.Dot(normal, dir.normalized);
            float r = sdir.magnitude * pDot;
            Vector3 check_start = mid + (dir.normalized * ((r / 2f) + (1 - pDot)));
            Vector3 check_end = end.mid - (dir.normalized * ((r / 2f) + (1 - pDot)));
            Vector3 tmpDir = (check_end - check_start);
            Vector3 pos = check_start + tmpDir / 2f;
            float w = sdir.magnitude * pDot;
            //Vector3 scale = new Vector3(w, checkHeight, tmpDir.magnitude);
            Vector3 scale = new Vector3(checkHeight, checkHeight, tmpDir.magnitude);
            Quaternion orientation = Quaternion.LookRotation(dir);
            blocked = Physics.CheckBox(pos, scale / 2f, orientation, LayerMask.GetMask("Bridge"));

            if (blocked) {
                // if blocked, close bridge
                Case.NewRamp(left, right);
                return;
            }


            // set used 
            end.used = true;
            used = true;

            float angle = Vector3.Angle(normal, dir);
            // check ladder angle
            if (angle > GlobalRoomData.Get.angleToLadder) {
                Transform ladder = PoolManager.Instance.RequestObject("ladder");
                float dot = Vector3.Dot(dir, Vector3.up);
                ladder.position = dot > 0f ? mid : end.mid;
                ladder.LookAt(dot > 0f ? end.mid : mid);
                ladder.Rotate(Vector3.right * 90f);
                ladder.GetComponent<LadderControl>().height = ldir.magnitude;
                return;
            } else {
                Transform bridge_Tr = PoolManager.Instance.RequestObject("bridge");
                float balconyHeight = GlobalRoomData.Get.balconyHeight;

                // mesh
                MeshFilter meshFilter = bridge_Tr.GetComponentInChildren<MeshFilter>();
                Vector3[] vertices = new Vector3[8]
                {
                left - Vector3.up * balconyHeight,
                right- Vector3.up * balconyHeight,
                left,
                right,
                end.right,
                end.left,
                end.right - Vector3.up * balconyHeight,
                end.left - Vector3.up * balconyHeight,
                };
                MeshControl.Update(meshFilter, vertices);
                bridge_Tr.GetComponentInChildren<MeshCollider>().sharedMesh = meshFilter.mesh;

                Case.NewRamp(left, end.right);
                Case.NewRamp(right, end.left);

                if (angle > GlobalRoomData.Get.angleToStairs) {
                    float stairWidth = GlobalRoomData.Get.stairWidth;
                    float dis = (end.mid - mid).magnitude;
                    int stairCount = (int)(dis / stairWidth);
                    for (int i = 0; i < (stairCount + 1); i++) {
                        Vector3 stair_Left = Vector3.Lerp(left, end.right, (float)i / stairCount);
                        Vector3 stair_Right = Vector3.Lerp(right, end.left, (float)i / stairCount);
                        Transform stairStep = PoolManager.Instance.RequestObject("stair step");
                        stairStep.position = stair_Left + (stair_Right - stair_Left) / 2f;
                        stairStep.right = (stair_Left - stair_Right).normalized;
                        stairStep.localScale = new Vector3((stair_Left - stair_Right).magnitude, stairWidth, stairWidth);
                    }
                }
            }
        }

        public void Draw() {
            //Gizmos.color = used ? Color.green: Color.yellow;
            Gizmos.color = color;
            Gizmos.DrawLine(left, right);
            Gizmos.DrawSphere(left, 0.1f);
            if (end != null) {
                // debug check box 
                float checkHeight = 1.5f;
                float dot = Vector3.Dot(normal, dir.normalized);
                float r = sdir.magnitude * dot;
                Vector3 check_start = mid + (dir.normalized * ((r / 2f) + (1 - dot)));
                Vector3 check_end = end.mid - (dir.normalized * ((r / 2f) + (1 - dot)));
                Vector3 tmpDir = (check_end - check_start);
                Vector3 pos = check_start + tmpDir / 2f;
                float w = sdir.magnitude * dot;
                Vector3 scale = new Vector3(w, checkHeight, tmpDir.magnitude);
                Quaternion orientation = Quaternion.LookRotation(dir);
                bool _blocked = Physics.CheckBox(pos, scale / 2f, orientation, LayerMask.GetMask("Bridge"));
                Gizmos.color = blocked ? Color.red : Color.green;
                Gizmos.matrix = Matrix4x4.TRS(pos, orientation, Vector3.one);
                Gizmos.DrawWireCube(Vector3.zero, scale);
                Gizmos.matrix = Matrix4x4.identity;

            }
            return;
            if (end != null) {
                Gizmos.color = Color.green;
                Gizmos.DrawLine(left, right);
                Gizmos.DrawSphere(left, 0.1f);


            } else {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(left - Vector3.up * 0.1f, right - Vector3.up * 0.1f);
                Gizmos.DrawSphere(left - Vector3.up * 0.1f, 0.1f);
            }

        }

        public Vector3 normal {
            get { return Vector3.Cross(sdir, Vector3.up); }
        }


    }
}