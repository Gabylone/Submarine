using Cinemachine.Utility;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.TerrainUtils;

public class Ladder : Interactable {
    // 🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕 //

    [Space]
    [Header("Player")]
    public float player_ClimbSpeed = 1f;
    public float player_LerpToPositionSpeed = 10f;
    public float currentY = 0f;
    public float maxY = 0f;
    public Vector3 player_DecalToLadder = new Vector3(0f, 0.2f, -0.18f);
    private bool canMove = true;
    public bool debug_Ladder = false;

    [Space]
    [Header("Generation")]
    public Transform[] ladderSteps;
    public float ladder_SpaceBetweenSteps;
    public float ladderSide_Width;
    // left, right, top left, top right
    public Vector3[] positions;


    [Space]
    [Header("Sound")]
    public AudioClip[] steps_Clips;
    public AudioSource steps_Source;

    [Space]
    [Header("IK")]
    public int[] ik_indexes = new int[4];
    private Transform[] ik_Anchors;
    public Vector3 ik_decalToStep;
    public float upDecal = 0f;
    public float ik_Rate = 1f;
    public bool ik_Switch = false;
    public float ik_Current = 0f;

    // distance in units
    /*public float ik_FeetDistance = 0.3f;
    public float ik_HandFootDistance = 1.5f;
    public float ik_UpdateDistance = 0.25f;*/

    // distance in indexes
    public float[] decals = new float[4];

    [Range(0f, 1f)]
    public float lerp = 0f;


    [Space]
    [Header("Exit")]
    public float exit_RaycastDistance = 2f;
    public Vector3 exit_Decal;
    public LayerMask exit_LayerMask;
    private float exit_CurrentBodyRotation = 0f;
    public float exit_TargetBodyRotation = 90f;
    public float exit_SpeedBodyRotation = 5f;
    public GameObject[] exit_Feedbacks;
    public Vector3 exitDecal_Up;
    public float exit_UpBuffer = 1.58f;
    public Vector3 exitDecal_Down;

    [Space]
    [Header("Lerp")]
    private bool lerping = false;
    public float lerpDuration = 0.35f;
    private float lerpTimer;
    public Transform landAnchor;

    float moveTimer = 0f;
    public float moveRate = 0.2f;
    
    public enum Direction { Idle, Up, Down }
    public Direction direction;

    public Transform[] debug_anchors;

    public override void Start() {
        base.Start();
        if (debug_Ladder) {
            Init(debug_anchors.ToList().Select(x => x.position).ToArray()); 
        }

    }

    public Transform GetAnchor(IKParam.Type type) {
        return ik_Anchors[(int)type];
    }

    public void Init(Vector3[] _positions) {
        // creating anchors
        ik_Anchors = new Transform[4];
        for (int i = 0; i < 4; i++) {
            ik_Anchors[i] = new GameObject().transform;
            ik_Anchors[i].parent = GetTransform;
            ik_Anchors[i].name = $"Anchor ({(IKParam.Type)i})";
        }

        // hiding feedback
        foreach (var item in exit_Feedbacks)
            item.SetActive(false);

        // creating overall parent
        Transform stepParent = new GameObject().transform;
        stepParent.parent = GetTransform;
        stepParent.name = "Steps";

        // instantiating sides 
        positions = _positions;

        int posIndex = 0;
        for (int i = -1; i < 2; i += 2) {
            var upDir = (positions[posIndex+2] - positions[posIndex]);
            Transform side = PoolManager.Instance.RequestObject("ladder side");
            side.position = positions[posIndex] + upDir / 2f;
            side.up = upDir.normalized;
            side.localScale = new Vector3(ladderSide_Width, upDir.magnitude, ladderSide_Width);
            side.parent = stepParent;
            posIndex++;
        }

        // instantiating each steps
        int stepsAmount = (int)(LeftDir.magnitude / (ladder_SpaceBetweenSteps + ladderSide_Width));
        ladderSteps = new Transform[stepsAmount];
        for (int i = 0; i < stepsAmount; i++) {

            Transform step = PoolManager.Instance.RequestObject("ladder step");

            var leftPos = positions[0] + LeftDir * i / stepsAmount;
            var rightPos= positions[1] + RightDir * i / stepsAmount;

            step.position = leftPos + (rightPos - leftPos) /2f;
            step.right = (rightPos-leftPos).normalized;

            var width = (rightPos-leftPos).magnitude;
            step.localScale = new Vector3(width, 1f, 1f);

            ladderSteps[i] = step;
            step.parent = stepParent;
        }

        float h = MidDir.magnitude + 2f;
        float w = (positions[1] - positions[0]).magnitude +3f;
        var bottomNormal = Vector3.Cross(MidDir.normalized, (positions[1] - positions[0]).normalized);
        var sidePos = positions[0] + (positions[1] - positions[0]) / 2f;
        var forPos = bottomNormal * 1f;
        var upPos = MidDir.normalized * (h / 2f);

        exit_CurrentBodyRotation = 0f;

        var pos = sidePos + upPos + forPos;
        Interactable_Trigger.transform.position = pos;
        Interactable_Trigger.transform.rotation = Quaternion.LookRotation(bottomNormal, MidDir);
        Interactable_Trigger.transform.localScale = new Vector3(w, h, 2f);
    }

    public override void Update() {
        base.Update();
        if (lerping) {
            lerpTimer += Time.deltaTime;

            float lerp = lerpTimer / lerpDuration;

            Vector3 v = Vector3.Lerp(Player.Instance.GetTransform.position, landAnchor.position, lerp);
            Player.Instance.GetTransform.position = v;
            Player.Instance.Body.rotation = Quaternion.Lerp(Player.Instance.Body.rotation, landAnchor.rotation, lerp);

        }
    }

    int GetClosestStep(Vector3 p) {
        int closestStep = 0;
        for (int i = 0; i < ladderSteps.Length; i++) {
            float disToClosest = Vector3.Distance(ladderSteps[closestStep].position, p);
            float dis = Vector3.Distance(ladderSteps[i].position, p);
            if (dis < disToClosest) {
                closestStep = i;
            }
        }
        return closestStep;
    }



    public override void Interact_Start() {
        for (int i = 0; i < ik_indexes.Length; i++) {
            ik_indexes[i] = 0;
        }

        base.Interact_Start();
        Player.Instance.DisableGravity();
        IKManager.Instance.StopAll();
        currentY = Vector3.Distance(ladderSteps[GetClosestStep(Player.Instance.GetTransform.position)].position, BottomMid);
        ik_Current = ik_Rate + 1f;
        for (int i = 0; i < 2; i++)
            UpdateDecalIKs();

        
        /*int startIndex = GetClosestStep(Player.Instance.GetTransform.position);

        if (startIndex + feetToHandDecal + (feetDecal * 2) >= ladderSteps.Length) {
            startIndex = ladderSteps.Length - (feetToHandDecal + (feetDecal * 2));
        }*/

        /*SetIKPos(IKParam.Type.LeftFoot, startIndex);
        SetIKPos(IKParam.Type.RightFoot, startIndex + feetDecal);
        SetIKPos(IKParam.Type.LeftHand, startIndex + feetToHandDecal);
        SetIKPos(IKParam.Type.RightHand, startIndex + feetToHandDecal + feetDecal);*/



        //Player.Instance.GetTransform.DOMove(ladderSteps[startIndex].position + GetTransform.forward * player_DecalToLadder.z + GetTransform.up * player_DecalToLadder.y, 0.2f);

    }


    public override void Interact_Update() {
        base.Interact_Update();
        if (!canMove)
            return;

        UpdateRotation();
        UpdateMovement();
        //UpdateExit();
    }

    void UpdateRotation() {
        lerp = currentY / (TopMid - BottomMid).magnitude;
        var normal = Vector3.Cross(MidDir.normalized, CurrSideDir.normalized);
        Quaternion q = Quaternion.LookRotation(-normal, MidDir.normalized);
        q *= Quaternion.Euler(0, exit_CurrentBodyRotation, 0);
        player_anchor.rotation = q;
        Player.Instance.Body.rotation = Quaternion.Lerp(Player.Instance.Body.rotation, q, player_LerpToPositionSpeed * Time.deltaTime); ;
    }

    void UpdateMovement() {
        float input = Input.GetAxis("Vertical");
        if (input > 0) {
            direction = Direction.Up;
        } else if (input < 0) {
            direction = Direction.Down;
        }

        var UpDir = (TopMid - BottomMid);
        maxY = UpDir.magnitude;
        if (moveTimer > 0) {
            moveTimer -= Time.deltaTime;
        } else {
            currentY += input * player_ClimbSpeed * Time.deltaTime;

            ik_Current += Mathf.Abs(input) * player_ClimbSpeed * Time.deltaTime;
            if (ik_Current > ik_Rate) {
                UpdateDecalIKs();
                ik_Current = 0f;
            }
        }


        // update exit
        if ( direction == Direction.Up) {
            RaycastHit upHit;
            bool exitUp = Physics.Raycast(Player.Instance.GetTransform.position + player_anchor.TransformDirection(exitDecal_Up), -Vector3.up, out upHit, exit_RaycastDistance, exit_LayerMask);
            exit_Feedbacks[0].SetActive(exitUp);
            if (exitUp)
                exit_Feedbacks[0].transform.position = upHit.point;
            if ( currentY >= maxY - exit_UpBuffer) {
                ExitLadder(upHit.point);
                Debug.Log("exit up");
            }
        }

        if ( direction == Direction.Down) {
            RaycastHit downHit;
            bool exitDown = Physics.Raycast(Player.Instance.GetTransform.position + player_anchor.TransformDirection(exitDecal_Down), -Vector3.up, out downHit, exit_RaycastDistance, exit_LayerMask);
            exit_Feedbacks[1].SetActive(exitDown);
            if (exitDown)
                exit_Feedbacks[1].transform.position = downHit.point;
            if ( currentY <= 0) {
                ExitLadder(downHit.point);
                Debug.Log($"exit down");
            }
        }

        // position
        Vector3 p = BottomMid + UpDir.normalized * (currentY+ player_DecalToLadder.y) + player_anchor.forward * player_DecalToLadder.z;
        p = Vector3.Lerp(Player.Instance.GetTransform.position, p, player_LerpToPositionSpeed * Time.deltaTime);
        Player.Instance.GetTransform.position = p;
    }

    float getDistance(Vector3 p) {
        return Vector3.Distance(BottomMid, p);
    }

    void UpdateDecalIKs() {

        moveTimer = moveRate;

        var pos = Player.Instance.GetTransform.position;
        var dir = (TopMid - BottomMid).normalized;
        var decal = direction == Direction.Up ? upDecal : 0f;

        ik_Switch = !ik_Switch;
        if (ik_Switch) {
            SetIKPos(IKParam.Type.RightFoot, GetClosestStep(pos + dir * (decals[0] + decal)));
            //SetIKPos(IKParam.Type.LeftFoot, GetClosestStep(pos + dir * decals[1]));
            SetIKPos(IKParam.Type.RightHand, GetClosestStep(pos + dir * (decals[2] + decal)));
            //SetIKPos(IKParam.Type.LeftHand, GetClosestStep(pos + dir * decals[3]));
        } else {
            SetIKPos(IKParam.Type.LeftFoot, GetClosestStep(pos + dir * (decals[0] + decal)));
            //SetIKPos(IKParam.Type.RightFoot, GetClosestStep(pos + dir * decals[1]));
            SetIKPos(IKParam.Type.LeftHand, GetClosestStep(pos + dir * (decals[2] + decal)));
            //SetIKPos(IKParam.Type.RightHand, GetClosestStep(pos + dir * decals[3]));
        }
    }



    void SetDirection(Direction dir) {
        if (direction != dir) {
            switch (dir) {
                case Direction.Idle:
                    IKManager.Instance.SetTarget(IKParam.Type.Head, null);
                    break;
                case Direction.Up:
                    IKManager.Instance.SetTarget(IKParam.Type.Head, ladderSteps[ladderSteps.Length - 1]);
                    break;
                case Direction.Down:
                    IKManager.Instance.SetTarget(IKParam.Type.Head, GetTransform);
                    break;
                default:
                    break;
            }
        }

        direction = dir;
    }

    public void DecalIks(IKParam.Type type, int way) {
        /*steps_Source.clip = steps_Clips[Random.Range(0, steps_Clips.Length)];
        steps_Source.Play();

        SetIKPos(type, ik_indexes[(int)type] + ((feetDecal * 2) * way));
        SetIKPos((type + 2), ik_indexes[(int)(type + 2)] + ((feetDecal * 2) * way));*/
    }

    public void SetIKPos(IKParam.Type type, int targetIndex) {
        ik_indexes[(int)type] = targetIndex;

        float decalRight = (type == IKParam.Type.RightFoot || type == IKParam.Type.RightHand) ? ik_decalToStep.x : -ik_decalToStep.x;
        GetAnchor(type).position = ladderSteps[targetIndex].position + ladderSteps[targetIndex].right * decalRight + Normal * ik_decalToStep.z;
        //GetAnchor(type).position = ladderSteps[targetIndex].position;
        IKManager.Instance.SetTarget(type, GetAnchor(type));
    }


    bool[] raisingHand = new bool[2];
    void UpdateExit() {
        int index = 0;
        // for each side of the ladder
        for (int way = -1; way < 2; way += 2) {

            // ray cast param
            RaycastHit hit;
            var x = CurrSideDir.normalized * way * (CurrSideDir.magnitude/2f+exit_Decal.x);
            var y = MidDir.normalized * exit_Decal.y;
            var z = Normal * exit_Decal.z;
            var rayOrigin = CurrMid + x + y + z;
            var ray = new Ray(rayOrigin, -RotationRef.Instance.GetUpDirection() * exit_RaycastDistance);
            bool raycast = Physics.Raycast(ray, out hit, exit_RaycastDistance, exit_LayerMask);

            if (raycast) {

                // place feedback
                var exit_Feedback = exit_Feedbacks[index];
                exit_Feedback.SetActive(true);
                exit_Feedback.transform.position = hit.point;
                exit_Feedback.transform.up = hit.normal;
                Debug.DrawLine(rayOrigin, hit.point, Color.green);

                // hands
                var input = Input.GetAxis("Horizontal");
                bool pressingInput = way < 0 ? input < 0 : input > 0;
                IKParam.Type ikType = way < 0 ? IKParam.Type.LeftHand: IKParam.Type.RightHand;
                if (pressingInput) {
                    if (!raisingHand[index]) {
                        exit_CurrentBodyRotation = Mathf.Lerp(exit_CurrentBodyRotation, exit_TargetBodyRotation * way, exit_SpeedBodyRotation * Time.deltaTime);
                        raisingHand[index] = true;
                        IKManager.Instance.SetTarget(ikType, exit_Feedback.transform);
                        IKManager.Instance.SetTarget(IKParam.Type.Head, exit_Feedback.transform);
                    }
                    if (ExitInput())
                        ExitLadder(hit.point);
                } else if (raisingHand[index]) {
                    IKManager.Instance.SetTarget(ikType, GetAnchor(ikType));
                    raisingHand[index] = false;
                    exit_CurrentBodyRotation = Mathf.Lerp(exit_CurrentBodyRotation, 0f, exit_SpeedBodyRotation * Time.deltaTime);
                    IKManager.Instance.Stop(IKParam.Type.Head);
                }

            } else {
                Debug.DrawRay(ray.origin, ray.direction * exit_RaycastDistance, Color.red);
                exit_Feedbacks[index].SetActive(false);
                raisingHand[index] = false;
            }

            ++index;

        }
    }

    void ExitLadder(Vector3 targetPos) {
        //Player.Instance._boxCollider.enabled = false;
        landAnchor.position = targetPos;
        lerping = true;
        lerpTimer = 0f;
        Invoke("LerpEnd", lerpDuration);
        IKManager.Instance.StopAll();

        foreach (var item in exit_Feedbacks) {
            item.SetActive(false);
        }

        base.Interact_Exit();
    }



    public override void CheckInput() {
        //base.CheckInput();
    }

    void LerpEnd() {
        lerping = false;

        Player.Instance.EnableMovements();
        Player.Instance.EnableGravity();

        //Player.Instance.GetComponent<Rigidbody>().useGravity = true;
    }

    public Vector3 GetPositionOnSegment(Vector3 A, Vector3 B, Vector3 point) {
        Vector3 projection = Vector3.Project(point - A, B - A);
        return projection + A;
    }

    private Vector3 BottomLeft => positions[0];
    private Vector3 BottomRight => positions[1];
    private Vector3 TopLeft => positions[2];
    private Vector3 TopRight => positions[3];

    private Vector3 LeftDir => TopLeft- BottomLeft;
    private Vector3 RightDir => TopRight - BottomRight;
    private Vector3 BottomDir => BottomRight - BottomLeft;
    private Vector3 TopDir => BottomLeft - TopLeft;
    private Vector3 BottomMid => BottomLeft + (BottomRight - BottomLeft) / 2f;
    private Vector3 TopMid => TopLeft + (TopRight - TopLeft) / 2f;
    private Vector3 MidDir => TopMid - BottomMid;
    private Vector3 CurrMid => BottomMid + MidDir * lerp;
    private Vector3 CurrLeft => BottomLeft + LeftDir * lerp;
    private Vector3 CurrRight => BottomRight + RightDir * lerp;

    private Vector3 CurrSideDir => CurrRight - CurrLeft;
    private Vector3 Normal => Vector3.Cross(MidDir.normalized, CurrSideDir.normalized);

    private void OnDrawGizmos() {

        if (positions.Length == 0 || (RoomManager.Instance != null &&!RoomManager.Instance.debugLadders))
            return;

        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(positions[0], 0.2f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(positions[1], 0.2f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(positions[2], 0.2f);
        Gizmos.DrawSphere(positions[3], 0.2f);

        if (!debug_Ladder)
            return;

        Gizmos.matrix = Matrix4x4.identity;


        // sides
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(positions[0], positions[1]);
        Gizmos.DrawLine(positions[2], positions[3]);

         // LADDER PREVIEW
         Gizmos.matrix = transform.localToWorldMatrix;

         if (ik_Anchors == null) {
             return;
         }

         // IKS
         Gizmos.matrix = Matrix4x4.identity;
         Gizmos.color = Color.blue;
         foreach (var item in ik_Anchors) {
             Gizmos.DrawSphere(item.position, 0.1f);
         }
     }
}
