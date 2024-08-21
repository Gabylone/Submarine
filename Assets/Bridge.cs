using UnityEngine;

[System.Serializable]
public struct Bridge {
    public class Side {
        public Color color = Color.white;
        public bool valid = true;
        public Side end;
        public bool link = false;
        public bool used = false;
        public bool blocked = false;
        public Transform parent;

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

        public bool Blocked(Bridge.Side targetSide, bool debug = false) {
            float sideBuffer =GlobalRoomData.Get.bridgeSideBuffer;
            float lenghtBuffer =GlobalRoomData.Get.bridgeLenghtBuffer;
            float upDecal =GlobalRoomData.Get.bridgeUpDecal;

            float dot = Vector3.Dot(targetSide.normal, normal);
            if (dot > -0.25f) {
                if (debug) {
                    Gizmos.color = Color.magenta;
                    Gizmos.DrawRay(mid, normal);
                    Gizmos.DrawRay(targetSide.mid, targetSide.normal);
                    Gizmos.color = Color.white;
                    Gizmos.DrawLine(mid, targetSide.mid);
                }
                return true;
            }

            
            Vector3[] start = new Vector3[4] {
                left - sdir * sideBuffer,
                right + sdir * sideBuffer,
                left - sdir * sideBuffer + sdir * upDecal,
                right + sdir * sideBuffer - sdir * upDecal,
            };

            Vector3[] end = new Vector3[4] {
                targetSide.right + targetSide.sdir * sideBuffer,
                targetSide.left - targetSide.sdir * sideBuffer,
                targetSide.right + targetSide.sdir * sideBuffer+ sdir * upDecal,
                targetSide.left - targetSide.sdir * sideBuffer - sdir * upDecal,
            };

            bool block = false;
            for (int i = 0; i < 4; i++) {
                var dir = (end[i] - start[i]).normalized;

                var a = start[i] + dir * lenghtBuffer - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;
                var b = end[i] - dir * lenghtBuffer - Vector3.up * GlobalRoomData.Get.balconyHeight / 2F;

                if (i > 1) {
                    var leftMid = left + (targetSide.right - left) / 2F;
                    var rightMid = right + (targetSide.left - right) / 2F;
                    var normalUp = Vector3.Cross(dir, (rightMid - leftMid).normalized);
                    a += normalUp * GlobalRoomData.Get.bridgeHeight;
                    b += normalUp * GlobalRoomData.Get.bridgeHeight;
                }

                RaycastHit hit;
                bool cast = Physics.Linecast(a, b, out hit, RoomGenerator.GenLayerMask);
                if (cast)
                    block = true;
                if (debug) {
                    if (cast) {
                        Gizmos.color = Color.red;
                        Gizmos.DrawLine(a, hit.point);
                        Gizmos.color = Color.yellow;
                        Gizmos.DrawLine(hit.point, b);
                    } else {
                        Gizmos.color = Color.green;
                        Gizmos.DrawLine(a, b);
                    }
                }
            }

           

            return block;
        }
        static Transform Parent;
        public void Build() {
            if (end == null || end.used || used)
                return;

            if (Parent == null) {
                Parent = new GameObject().transform;
                Parent.name = "Bridges";
            }

            parent = new GameObject().transform;
            parent.SetParent(Parent);
            parent.name = "Bridge";

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
            blocked = Physics.CheckBox(pos, scale / 2f, orientation, RoomGenerator.GenLayerMask);

            /*if (blocked) {
                Debug.Log($"blocked? ");
                // if blocked, close bridge
                Case.NewRamp(left, right);
                return;
            }*/


            // set used 
            end.used = true;
            used = true;

            float angle = Vector3.Angle(normal, dir);
            // check ladder angle
            if (angle > GlobalRoomData.Get.angleToLadder) {
                var ladder = PoolManager.Instance.RequestObject("ladder", parent).GetComponent<Ladder>();

                var positions = new Vector3[4] {
                    end.right,
                    end.left,
                    left,
                    right,

                };
                if ( end.right.y > right.y) {
                    positions = new Vector3[4] {
                        left,
                        right,
                        end.right,
                        end.left,
                    };
                }

                ladder.Init(positions);
                return;
            } else {
                Transform bridge_Tr = PoolManager.Instance.RequestObject("bridge", parent);
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

                // juste par clarté par de rampe
                /**/

                if (angle > GlobalRoomData.Get.angleToStairs) {
                    float stairWidth = GlobalRoomData.Get.stairWidth;
                    float dis = (end.mid - mid).magnitude;
                    int stairCount = (int)(dis / stairWidth);
                    for (int i = 0; i < (stairCount + 1); i++) {
                        Vector3 stair_Left = Vector3.Lerp(left, end.right, (float)i / stairCount);
                        Vector3 stair_Right = Vector3.Lerp(right, end.left, (float)i / stairCount);
                        Transform stairStep = PoolManager.Instance.RequestObject("stair step", parent);
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
                bool _blocked = Physics.CheckBox(pos, scale / 2f, orientation, RoomGenerator.GenLayerMask);
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