using UnityEngine;

public class Wheel : Interactable {
    public int index_left = 0;
    public int index_right = 3;
    public Transform[] anchors;

    public int indexRate = 2;

    private float angle = 0f;
    public float angleToStop = 0f;
    private float wheelTimer = 0f;
    public float timeBetweenTurns = 1f;

    private bool turning = false;

    public AudioClip handleClip;
    public AudioClip[] stopClips;
    public AudioClip[] rollClips;
    public AudioClip[] startClips;
    public float minPitch = 0.6f;
    public float maxPitch = 1f;
    public AudioSource effectSource;
    public AudioSource loopSource;

    bool playedStopSound = false;
    public float stopSoundBuffer;

    [SerializeField]
    private float value = 0f;
    public float valueMultiplier = 1f;
    public float valueSpeed = 0.05f;

    public Transform _targetTransform;
    public float rotateSpeed = 5f;

    public bool playedSound = false;

    [Header("Reset")]
    public float reset_Duration = 1f;
    private float reset_Timer = 0f;
    public float reset_RotateSpeed = 50f;
    private float reset_LerpAngle;
    private float reset_LerpValue;
    public bool reseting = false;

    public override void Start() {
        base.Start();

        Submarine.Instance.onResetMecanisms += HandleOnCrash;
    }

    public override void Update() {
        base.Update();

        if (reseting) {
            Reset_Update();
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Reset_Start();
        }
    }

    public override void Interact_Start() {
        base.Interact_Start();

        PlaySoundEffect(handleClip);

        UpdateAnchors();
    }

    public override void Interact_Update() {
        base.Interact_Update();

        Vector3 p = Vector3.Lerp(Player.Instance.GetTransform.position, player_anchor.position, Time.deltaTime * 2f);
        Player.Instance.GetTransform.position = p;
        Quaternion d = Quaternion.Lerp(Player.Instance.Body.rotation, player_anchor.rotation, Time.deltaTime * 2f);
        Player.Instance.Body.rotation = d;

        Turn_Update();
    }

    public override void Interact_Exit() {
        base.Interact_Exit();

        PlaySoundEffect(handleClip);

        Player.Instance.EnableMovements();

        IKManager.Instance.StopHands();
        Invoke("Interact_ExitDelay", 0.01f);
    }
    void Interact_ExitDelay() {
        loopSource.Stop();

    }
    void Turn_Update() {
        float lerp = value < 0f ? -value : value;
        float pitch = Mathf.Lerp(minPitch, maxPitch, lerp);
        effectSource.pitch = pitch;
        loopSource.pitch = pitch;

        if (turning) {
            if (Input.GetAxis("Horizontal") > -.1f
                &&
                Input.GetAxis("Horizontal") < .1f
                ) {
                loopSource.Stop();

                PlaySoundEffect(stopClips[Random.Range(0, stopClips.Length)]);

                playedSound = false;
                playedStopSound = false;
                return;
            }

            if ((value >= stopSoundBuffer || value <= -stopSoundBuffer) && !playedStopSound) {
                PlaySoundEffect(stopClips[Random.Range(0, stopClips.Length)]);

                playedStopSound = true;

                Debug.Log("wheel stop clip ");
            }

            if (Input.GetAxis("Horizontal") < 0) {
                if (value <= -1) {
                    loopSource.Stop();
                    return;
                }

                if (!playedSound) {
                    PlaySoundLoop(rollClips[0]);

                    playedSound = true;
                }

                angle -= rotateSpeed * Time.deltaTime;
                value -= valueSpeed * Time.deltaTime;
                _targetTransform.Rotate(Vector3.forward * rotateSpeed * Time.deltaTime);
            } else {
                if (value >= 1) {
                    loopSource.Stop();
                    return;
                }

                if (!playedSound) {
                    PlaySoundLoop(rollClips[1]);

                    playedSound = true;
                }

                angle += rotateSpeed * Time.deltaTime;
                value += valueSpeed * Time.deltaTime;
                _targetTransform.Rotate(-Vector3.forward * rotateSpeed * Time.deltaTime);
            }

            if (angle >= angleToStop || angle <= -angleToStop) {
                if (angle >= angleToStop) {
                    ChangeAnchor(false);
                }

                if (angle <= -angleToStop) {
                    ChangeAnchor(true);
                }

                wheelTimer = 0f;
                turning = false;
                PlaySoundEffect(stopClips[Random.Range(0, stopClips.Length)]);
                loopSource.Stop();
            }
        } else {
            wheelTimer += Time.deltaTime;

            if (wheelTimer >= timeBetweenTurns) {
                angle = 0f;
                turning = true;
            }
        }
    }

    public void PlaySoundLoop(AudioClip clip) {
        loopSource.clip = clip;
        loopSource.Play();
    }

    public void PlaySoundEffect(AudioClip clip) {
        effectSource.clip = clip;
        effectSource.Play();
    }

    public float GetLerp() {
        return Mathf.InverseLerp(-value, value, _targetTransform.localRotation.x);
    }

    void ChangeAnchor(bool left) {
        PlaySoundEffect(startClips[Random.Range(0, startClips.Length)]);

        playedSound = false;

        if (left) {
            index_left -= indexRate;
            index_right -= indexRate;

            if (index_right < 0) {
                index_right += anchors.Length;
            }

            if (index_left < 0) {
                index_left += anchors.Length;
            }
        } else {
            index_left += indexRate;
            index_right += indexRate;

            if (index_right >= anchors.Length) {
                index_right -= anchors.Length;
            }

            if (index_left >= anchors.Length) {
                index_left -= anchors.Length;
            }
        }

        UpdateAnchors();
    }

    void UpdateAnchors() {
        IKManager.Instance.SetTarget(IKParam.Type.LeftHand, anchors[index_left]);
        IKManager.Instance.SetTarget(IKParam.Type.RightHand, anchors[index_right]);
    }

    public float GetValue() {
        return value * valueMultiplier;
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        foreach (var anchor in anchors) {
            Gizmos.DrawWireSphere(anchor.position, 0.075f);
        }

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(anchors[index_left].position, 0.1f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(anchors[index_right].position, 0.1f);
    }

    #region reset
    void HandleOnCrash() {
        Reset_Start();
    }

    public void Reset_Start() {
        if (interacting) {
            Interact_Exit();
        }

        reseting = true;
        reset_LerpValue = value;
        reset_LerpAngle = angle;

        reset_Timer = 0f;

        loopSource.Play();
    }
    public void Reset_Update() {
        float lerp = reset_Timer / reset_Duration;

        if (value >= 0) {
            _targetTransform.Rotate(Vector3.forward * reset_RotateSpeed * Time.deltaTime);
        } else {
            _targetTransform.Rotate(-Vector3.forward * reset_RotateSpeed * Time.deltaTime);
        }

        value = Mathf.Lerp(reset_LerpValue, 0f, lerp);
        angle = Mathf.Lerp(reset_LerpAngle, 0f, lerp);

        reset_Timer += Time.deltaTime;

        if (reset_Timer >= reset_Duration) {
            Reset_Exit();
        }
    }
    public void Reset_Exit() {
        reseting = false;

        loopSource.Stop();

    }
    #endregion
}
