using UnityEngine;
using DG.Tweening;

public class Ladder : Interactable
{
    // 🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕🦕 //

    public float speedToPosition = 2f;

    float currentHeight = 0f;
    public float decalToLadder_Y = 0.0f;
    public float decalToLadder_Z = 0.18f;
    public float speedToTurnToLadder = 10f;

    public AudioClip[] steps_Clips;
    public AudioSource steps_Source;

    public float climbSpeed = 1f;

    public Transform[] ladderSteps;

    public int step_Current = 0;
    public int step_FootDecal = 1;
    public int step_FootToHand = 4;
    public int step_HandDecal = 1;

    public Transform ladder_Origin;
    public Transform ladder_End;

    public float distanceToGetOff = 2f;
    public float decalToGetOff = 1.5f;
    public LayerMask getOff_LayerMask;
    public float decalUpToGetOff = 0.5f;
    public int step_Rate = 3;

    public GameObject getOff_Feedback;

    public float bufferHeight = 0f;

    public float distanceToUpdateIKs = 0.25f;

    public float iks_decalRight = 0.5f;
    public float iks_decalForward = 0.1f;

    public Transform step_parent;

    bool skip = false;
    public int skipDecal = 1;
    int currSkip = 0;

    private bool lerping = false;
    public float lerpDuration = 0.35f;
    public Transform landAnchor;
    private float lerpTimer;
    public Transform initAnchor;

    public override void Start()
    {
        base.Start();

        distanceToUpdateIKs = ladderSteps[0].position.y - ladderSteps[1].position.y;
        distanceToUpdateIKs *= step_Rate;

        getOff_Feedback.SetActive(false);
    }

    public override void Update()
    {
        base.Update();

        if (lerping)
        {
            lerpTimer += Time.deltaTime;

            float lerp = lerpTimer / lerpDuration;

            Vector3 v = Vector3.Lerp( Player.Instance.GetTransform.position , landAnchor.position , lerp );
            Player.Instance.GetTransform.position = v;
        }
    }

    public override void Interact_Start()
    {
        Vector3 p = GetPositionOnSegment(ladder_Origin.position, ladder_End.position, Player.Instance.GetTransform.position);

        for (int i = 0; i < ladderSteps.Length; i++)
        {
            float disToClosest = Vector3.Distance(ladderSteps[step_Current].position, p);
            float dis = Vector3.Distance(ladderSteps[i].position, p);
            if ( dis < disToClosest )
            {
                SetStep(i);
            }
        }

        Debug.Log("ladder _ start");

        currentHeight = Vector3.Distance(p, ladder_Origin.position);

        base.Interact_Start();

        //Player.Instance._boxCollider.enabled = false;

        //Player.Instance.GetComponent<Rigidbody>().useGravity = false;

        UpdateIKs();

    }

    public override void CheckInput()
    {
        //base.CheckInput();
    }

    public override void Interact_Update()
    {
        base.Interact_Update();

        Vector3 pos = ladder_Origin.position + (-player_anchor.forward * decalToLadder_Z) + (player_anchor.up * (currentHeight + decalToLadder_Y));

        Player.Instance.GetTransform.position = Vector3.Lerp(Player.Instance.GetTransform.position, pos, speedToPosition * Time.deltaTime * 2f);

        Quaternion d = Quaternion.Lerp(Player.Instance.Body.rotation, player_anchor.rotation, Time.deltaTime * 2f);
        Player.Instance.Body.rotation = d;

        float input = Input.GetAxis("Vertical");

        if (input<0)
        {
            if (step_Current < ladderSteps.Length-1)
            {
                currentHeight += input * climbSpeed * Time.deltaTime;
            }
        }
        
        if (input > 0)
        {
            int l = step_FootToHand + step_HandDecal +1;
            if (step_Current > l)
            {
                currentHeight += input * climbSpeed * Time.deltaTime;
            }
        }

        Vector3 p = GetPositionOnSegment(ladder_Origin.position, ladder_End.position, Player.Instance.GetTransform.position);
        float disToStep = Vector3.Distance(ladderSteps[step_Current].position, ladder_Origin.position);

        // next step
        if ( currentHeight > disToStep + distanceToUpdateIKs)
        {
            if (step_Current == 0)
            {

            }
            else
            {
                SetStep(step_Current - step_Rate);
                UpdateIKs();
            }
        }

        // previous
        if (currentHeight < disToStep - distanceToUpdateIKs)
        {
            if (step_Current == ladderSteps.Length - 1)
            {
                
            }
            else
            {
                SetStep(step_Current + step_Rate);
                UpdateIKs();
            }
            
        }

        RaycastHit hit;

        if (Physics.Raycast(Player.Instance.GetTransform.position + player_anchor.right * decalToGetOff + player_anchor.up * decalUpToGetOff, -player_anchor.up, out hit, distanceToGetOff, getOff_LayerMask))
        {
            getOff_Feedback.transform.position = hit.point;
            getOff_Feedback.SetActive(true);

            if (ExitInput())
            {
                //Player.Instance._boxCollider.enabled = false;
                landAnchor.position = hit.point;
                lerping = true;

                lerpTimer = 0f;
                initAnchor.position = Player.Instance.GetTransform.position;
                Invoke("LerpEnd", lerpDuration);

                IKManager.Instance.StopAll();

                getOff_Feedback.SetActive(false);

                base.Interact_Exit();

                return;

            }
        }
        else
        {
            getOff_Feedback.SetActive(false);
        }

    }

    void LerpEnd()
    {
        lerping = false;

        Player.Instance.EnableMovements();

        //Player.Instance.GetComponent<Rigidbody>().useGravity = true;
    }

    public Vector3 GetPositionOnSegment(Vector3 A, Vector3 B, Vector3 point)
    {
        Vector3 projection = Vector3.Project(point - A, B - A);
        return projection + A;
    }

    private bool CanGetOffLadder(out Vector3 pos)
    {
        RaycastHit hit;

        if ( Physics.Raycast(Player.Instance.GetTransform.position + player_anchor.right * decalToGetOff + player_anchor.up * decalUpToGetOff, -player_anchor.up, out hit, distanceToGetOff, getOff_LayerMask) ){
            pos = hit.point;
            return true;
        }

        pos = Vector3.zero;

        return false;
    }

    void SetStep(int step)
    {
        AudioClip clip = steps_Clips[Random.Range(0, steps_Clips.Length)];
        steps_Source.clip = clip;
        steps_Source.Play();

        step_Current = step;
        step_Current = Mathf.Clamp(step_Current, step_FootToHand + step_HandDecal + 1, ladderSteps.Length - 1);
    }

    void UpdateIKs()
    {
        ikTrigger.leftFoot_Target.position = ladderSteps[this.step_Current - currSkip].position - player_anchor.right * iks_decalRight + player_anchor.forward* iks_decalForward;
        ikTrigger.rightFoot_Target.position     =   ladderSteps[this.step_Current - step_FootDecal].position + player_anchor.right * iks_decalRight + player_anchor.forward * iks_decalForward;
        ikTrigger.leftHand_Target.position      =    ladderSteps[this.step_Current - step_FootToHand - currSkip].position - player_anchor.right * iks_decalRight + player_anchor.forward * iks_decalForward;
        ikTrigger.rightHand_Target.position     =   ladderSteps[this.step_Current - step_FootToHand - step_HandDecal].position + player_anchor.right * iks_decalRight + player_anchor.forward * iks_decalForward;

        skip = !skip;
        currSkip = skip ? 0 : skipDecal;

        CheckIKs();
    }

    private void OnDrawGizmos()
    {

        if (interacting)
        {
            Transform playerTransform = GameObject.FindObjectOfType<Player>().transform;

            RaycastHit hit;
            if (Physics.Raycast(playerTransform.position + player_anchor.right * decalToGetOff + player_anchor.up * decalUpToGetOff, -player_anchor.up, out hit, distanceToGetOff, getOff_LayerMask))
            {
                Gizmos.color = Color.green;
                Gizmos.DrawRay(playerTransform.position + player_anchor.right * decalToGetOff + player_anchor.up * decalUpToGetOff, -player_anchor.up * hit.distance);
                Gizmos.DrawSphere(hit.point, 0.1f);
            }
            else
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawRay(playerTransform.position + player_anchor.right * decalToGetOff + player_anchor.up * decalUpToGetOff, -player_anchor.up * distanceToGetOff);
            }

            Vector3 pos = GetPositionOnSegment(player_anchor.position, player_anchor.position, Player.Instance.GetTransform.position);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pos, 0.1f);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(ladderSteps[step_Current].position, 0.1f);
        }
    }
}
