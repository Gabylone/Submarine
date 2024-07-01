using UnityEngine;
public class LadderControl : Interactable {
    // 🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕 //

    [Space]
    [Header("Player")]
    public float player_ClimbSpeed = 1f;
    public float player_LerpToPositionSpeed = 10f;
    float currentY = 0f;
    public Vector3 player_DecalToLadder = new Vector3(0f, 0.2f, -0.18f);
    private bool canMove = true;

    [Space]
    [Header("Generation")]
    public float height = 10f;
    public float width = 1f;
    private Transform[] ladderSteps;
    public float ladder_SpaceBetweenSteps;
    public float ladderSide_Width;

    [Space]
    [Header("Sound")]
    public AudioClip[] steps_Clips;
    private AudioSource steps_Source;

    [Space]
    [Header("IK")]
    int[] ik_indexes = new int[4];
    private Transform[] ik_Anchors;
    public Vector3 ik_decalToStep;

    // distance in units
    /*public float ik_FeetDistance = 0.3f;
    public float ik_HandFootDistance = 1.5f;
    public float ik_UpdateDistance = 0.25f;*/

    // distance in indexes
    public int feetDecal;
    public int feetToHandDecal;


    [Space]
    [Header("Exit")]
    public float exit_RaycastDistance = 2f;
    public Vector3 exit_Decal;
    public LayerMask exit_LayerMask;
    public GameObject exit_Feedback;

    [Space]
    [Header("Lerp")]
    private bool lerping = false;
    public float lerpDuration = 0.35f;
    private float lerpTimer;
    public Transform landAnchor;

    public enum Direction { Idle, Up, Down }
    public Direction direction;

    public override void Start() {
        base.Start();

        ik_Anchors = new Transform[4];

        for (int i = 0; i < 4; i++) {
            ik_Anchors[i] = new GameObject().transform;
            ik_Anchors[i].parent = GetTransform;
            ik_Anchors[i].name = $"Anchor ({(IKParam.Type)i})";
        }

        InitSteps();

        exit_Feedback.SetActive(false);
    }

    public Transform GetAnchor(IKParam.Type type) {
        return ik_Anchors[(int)type];
    }

    void InitSteps() {
        steps_Source = GetComponent<AudioSource>();

        Transform stepParent = new GameObject().transform;
        stepParent.parent = GetTransform;
        stepParent.name = "Steps";

        for (int i = -1; i < 2; i += 2) {
            Transform side = PoolManager.Instance.RequestObject("ladder side");
            side.position = GetTransform.position + GetTransform.up * height / 2f + GetTransform.right * i * width / 2f;
            side.rotation = GetTransform.rotation;
            side.localScale = new Vector3(ladderSide_Width, height, ladderSide_Width);
            side.parent = stepParent;
        }

        int stepsAmount = (int)(height / (ladder_SpaceBetweenSteps + ladderSide_Width));
        ladderSteps = new Transform[stepsAmount];
        for (int i = 0; i < stepsAmount; i++) {
            Transform step = PoolManager.Instance.RequestObject("ladder step");
            step.position = GetTransform.position + GetTransform.up * (ladder_SpaceBetweenSteps + (ladderSide_Width + ladder_SpaceBetweenSteps) * i);
            step.right = GetTransform.right;
            step.localScale = new Vector3(width - (ladderSide_Width), ladderSide_Width, ladderSide_Width);
            ladderSteps[i] = step;
            step.parent = stepParent;
        }

        Interactable_Trigger.transform.position = GetTransform.position + GetTransform.up * (height + 2) / 2f + GetTransform.forward * 0.5f;
        Interactable_Trigger.transform.localScale = new Vector3(width + 1f, height + 2f, 1f);
    }

    public override void Update() {
        base.Update();

        if (lerping) {
            lerpTimer += Time.deltaTime;

            float lerp = lerpTimer / lerpDuration;

            Vector3 v = Vector3.Lerp(Player.Instance.GetTransform.position, landAnchor.position, lerp);
            Player.Instance.GetTransform.position = v;
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

        int startIndex = GetClosestStep(Player.Instance.GetTransform.position);

        if (startIndex + feetToHandDecal + (feetDecal * 2) >= ladderSteps.Length) {
            startIndex = ladderSteps.Length - (feetToHandDecal + (feetDecal * 2));
        }

        SetIKPos(IKParam.Type.LeftFoot, startIndex);
        SetIKPos(IKParam.Type.RightFoot, startIndex + feetDecal);
        SetIKPos(IKParam.Type.LeftHand, startIndex + feetToHandDecal);
        SetIKPos(IKParam.Type.RightHand, startIndex + feetToHandDecal + feetDecal);

        currentY = getDistance(ladderSteps[startIndex].position);


        //Player.Instance.GetTransform.DOMove(ladderSteps[startIndex].position + GetTransform.forward * player_DecalToLadder.z + GetTransform.up * player_DecalToLadder.y, 0.2f);

        base.Interact_Start();
    }


    public override void Interact_Update() {
        base.Interact_Update();

        float input = Input.GetAxis("Vertical");

        UpdateDecalIKs(input);

        UpdateExit();
        if (!canMove) {
            return;
        }

        Vector3 p = GetTransform.position + GetTransform.up * currentY + GetTransform.up * player_DecalToLadder.y + GetTransform.forward * player_DecalToLadder.z;
        p = Vector3.Lerp(Player.Instance.GetTransform.position, p, player_LerpToPositionSpeed * Time.deltaTime);
        Player.Instance.GetTransform.position = p;

        Quaternion q = Quaternion.Lerp(Player.Instance.Body.rotation, player_anchor.rotation, player_LerpToPositionSpeed * Time.deltaTime);
        Player.Instance.Body.rotation = q;

        currentY += input * player_ClimbSpeed * Time.deltaTime;

        /*Player.Instance.GetTransform.Translate(GetTransform.up * input * player_ClimbSpeed * Time.deltaTime);
        Quaternion d = Quaternion.Lerp(Player.Instance.Body.rotation, GetTransform.rotation, Time.deltaTime * 2f);
        Player.Instance.Body.forward = Vector3.Lerp(Player.Instance.Body.forward, -transform.forward, Time.deltaTime);*/

    }

    float getDistance(Vector3 p) {
        return Vector3.Distance(GetTransform.position, p);
    }

    void UpdateDecalIKs(float input) {
        if (input > 0f) {
            SetDirection(Direction.Up);

            for (int i = 0; i < 2; i++) {
                if (getDistance(GetAnchor((IKParam.Type)i).position) < getDistance(Player.Instance.GetTransform.position)) {
                    if (ik_indexes[i + 2] + feetDecal * 2 >= ladderSteps.Length) {
                        canMove = false;
                        return;
                    }

                    DecalIks((IKParam.Type)i, 1);

                }
            }
        } else if (input < 0f) {
            SetDirection(Direction.Down);
            for (int i = 0; i < 2; i++) {
                if (getDistance(GetAnchor((IKParam.Type)i).position) > getDistance(Player.Instance.GetTransform.position) + GetUpdateDistance()) {
                    int targetIndex = ik_indexes[i] - (feetDecal * 2);
                    if (targetIndex < 0) {
                        canMove = false;
                        return;
                    }

                    DecalIks((IKParam.Type)i, -1);

                }
            }
        } else {
            canMove = true;
            SetDirection(Direction.Idle);
        }
    }

    float GetUpdateDistance() {
        return Vector3.Distance(ladderSteps[feetDecal].position, ladderSteps[0].position);
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
        steps_Source.clip = steps_Clips[Random.Range(0, steps_Clips.Length)];
        steps_Source.Play();

        SetIKPos(type, ik_indexes[(int)type] + ((feetDecal * 2) * way));
        SetIKPos((type + 2), ik_indexes[(int)(type + 2)] + ((feetDecal * 2) * way));
    }

    public void SetIKPos(IKParam.Type type, int targetIndex) {

        switch (type) {
            case IKParam.Type.LeftFoot:
                break;
            case IKParam.Type.RightFoot:
                break;
            case IKParam.Type.LeftHand:
                break;
            case IKParam.Type.RightHand:
                break;
            default:
                break;
        }

        ik_indexes[(int)type] = targetIndex;

        float decalRight = (type == IKParam.Type.RightFoot || type == IKParam.Type.RightHand) ? -ik_decalToStep.x : ik_decalToStep.x;
        float zDecal = type == IKParam.Type.RightFoot || type == IKParam.Type.LeftFoot ? ik_decalToStep.z : 0f;

        GetAnchor(type).position = ladderSteps[targetIndex].position + GetTransform.right * decalRight + GetTransform.up * ik_decalToStep.y + GetTransform.forward * zDecal;
        IKManager.Instance.SetTarget(type, GetAnchor(type));
    }

    void UpdateExit() {
        RaycastHit hit;

        Vector3 origin = Player.Instance.GetTransform.position + GetTransform.right * exit_Decal.x + RotationRef.Instance.GetUpDirection() * exit_Decal.y + GetTransform.up * exit_Decal.z;
        bool raycast = Physics.Raycast(origin, -RotationRef.Instance.GetUpDirection(), out hit, exit_RaycastDistance, exit_LayerMask);

        exit_Feedback.SetActive(raycast);
        if (raycast) {
            exit_Feedback.transform.position = hit.point;
            Debug.DrawLine(origin, hit.point, Color.green);

            if (ExitInput()) {
                //Player.Instance._boxCollider.enabled = false;
                landAnchor.position = hit.point;
                lerping = true;

                lerpTimer = 0f;
                Invoke("LerpEnd", lerpDuration);

                IKManager.Instance.StopAll();

                exit_Feedback.SetActive(false);

                base.Interact_Exit();

                return;

            }
        } else {
            Debug.DrawRay(origin, -RotationRef.Instance.GetUpDirection() * exit_RaycastDistance, Color.red);

        }
    }




    void LerpEnd() {
        lerping = false;

        Player.Instance.EnableMovements();

        //Player.Instance.GetComponent<Rigidbody>().useGravity = true;
    }

    public Vector3 GetPositionOnSegment(Vector3 A, Vector3 B, Vector3 point) {
        Vector3 projection = Vector3.Project(point - A, B - A);
        return projection + A;
    }

    private void OnDrawGizmos() {
        // LADDER PREVIEW
        Gizmos.matrix = transform.localToWorldMatrix;

        Gizmos.color = !canMove ? Color.red : Color.green;
        Gizmos.DrawLine(Vector3.zero, Vector3.up * height);

        for (int i = -1; i < 2; i += 2) {
            Gizmos.DrawCube(Vector3.up * height / 2f + Vector3.right * i * width / 2f, new Vector3(ladderSide_Width, height, ladderSide_Width));
        }

        int stepsAmount = (int)(height / (ladder_SpaceBetweenSteps + ladderSide_Width));
        for (int i = 0; i < stepsAmount; i++) {
            Gizmos.DrawCube(Vector3.up * (ladder_SpaceBetweenSteps + (ladderSide_Width + ladder_SpaceBetweenSteps) * i), new Vector3(width - (ladderSide_Width), ladderSide_Width, ladderSide_Width));
        }

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
