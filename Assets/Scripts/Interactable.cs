using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour {
    public bool interacting = false;
    public bool selected = false;
    public bool disableMovements = true;

    [Header("Components")]
    public Interactable_Trigger Interactable_Trigger;
    public Transform player_anchor;

    [Header("Outline")]
    public GameObject outlineGroup;
    private Renderer[] outlineRenderers;
    float outlineSpeed = 3f;
    float outlineValue = 0f;

    public CameraRoom contextuelCam;

    [Header("Lerp")]
    public bool lerpBody = true;
    public float lerp_MoveSpeed = 2f;
    public float lerp_RotSpeed = 10f;

    private float timer = 0f;


    public virtual void Start() {
        InitOutline();

        if (Interactable_Trigger == null) { Interactable_Trigger = GetComponentInChildren<Interactable_Trigger>(); }
    }

    public virtual void Update() {
        if (interacting) {
            Interact_Update();
        }
    }

    private void LateUpdate() {
        if (interacting) {
            Interact_LateUpdate();
        }

        if (selected) {
            UpdateSelect();
        }
    }

    #region selection
    public void Select() {
        selected = true;
        EnableOutline();

    }

    public void UpdateSelect() {
        UpdateOutline();

        if (Input.GetButtonDown("Interact")) {
            Interact_Start();
        }
    }

    public void Deselect() {
        selected = false;
        DisableOutline();

    }
    #endregion

    #region states
    public virtual void Interact_Start() {
        Deselect();

        interacting = true;
        InteractableManager.Instance.interacting = true;

        timer = 0f;

        if (disableMovements) {
            Player.Instance.DisableMovement();
        }

        if (contextuelCam != null) {
            contextuelCam.Trigger(false);
        }
    }

    public virtual void Interact_Update() {
        CheckInput();

        timer += Time.deltaTime;
    }

    public virtual void Interact_LateUpdate() {

    }

    public virtual void Interact_Exit() {
        interacting = false;
        InteractableManager.Instance.interacting = false;

        if (disableMovements) {
            Player.Instance.EnableMovements();
        }

        if (contextuelCam != null) {
            CameraRoom.previous.Trigger(false);
        }
    }
    #endregion

    #region outline
    private void InitOutline() {
        outlineGroup = outlineGroup == null ? gameObject : outlineGroup;

        outlineValue = 1f;

        outlineRenderers = outlineGroup.GetComponentsInChildren<Renderer>();

        List<Renderer> tmpRends = new List<Renderer>();
        Renderer[] rends = outlineGroup.GetComponentsInChildren<Renderer>();
        foreach (var rend in rends) {
            if (!rend.material.IsKeywordEnabled("_EMISSION")) {
                tmpRends.Add(rend);
            }
        }

        outlineRenderers = tmpRends.ToArray();
    }
    public void EnableOutline() {
        Tween.Bounce(outlineGroup.transform);
        /*outlineGroup.transform.DOLocalMove(outlineGroup.transform.localPosition + Vector3.up * 0.1f, 0.1f).SetEase(Ease.OutBounce);
        outlineGroup.transform.DOLocalMove(outlineGroup.transform.localPosition - Vector3.up * 0.1f, 0.1f).SetDelay(0.1f);*/

        foreach (var item in outlineRenderers) {
            item.material.EnableKeyword("_EMISSION");
        }
    }

    public void UpdateOutline() {
        outlineValue = Mathf.PingPong(Time.time * outlineSpeed, 1f);

        foreach (var item in outlineRenderers) {
            item.material.SetColor("_EmissionColor", Color.Lerp(Color.black, Color.white, outlineValue));
        }
    }

    public void DisableOutline() {
        foreach (var item in outlineRenderers) {
            item.material.DisableKeyword("_EMISSION");
        }
    }
    #endregion

    #region input
    public virtual void CheckInput() {
        if (ExitInput()) {
            Interact_Exit();
        }
    }

    public bool ExitInput() {
        return timer >= 0.2f && Input.GetButtonDown("Interact");
    }
    #endregion

    private void OnDrawGizmos() {
        if (player_anchor != null) {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(player_anchor.position, 0.3f);
        }
    }

    private Transform _transform;
    public Transform GetTransform {
        get {
            if (_transform == null) {
                _transform = transform;
            }

            return _transform;
        }
    }
}
