using UnityEngine;
using TMPro;
using Photon.Pun;
using UnityEngine.UI;

public class codePlayer : MonoBehaviourPunCallbacks
{
    [Header("Interface:")]
    [SerializeField] TMP_Text nickname;

    [Header("Configurações:")]
    [SerializeField] public bool disableInputs = false;
    [SerializeField] float moveSpeed;
    [SerializeField] float jumpSpeed;
    [SerializeField] float airTime;
    [SerializeField] float positionRadius;

    [Header("Atribuições:")]
    [SerializeField] GameObject playerCamera;
    [SerializeField] GameObject bulletObject;
    [SerializeField] Transform firePosition;
    [SerializeField] Transform playerPos;
    [SerializeField] LayerMask ground;

    [Header("Joystick:")]
    RectTransform joystick;
    RectTransform joystickArea;
    RectTransform dot;

    Vector2 moveDirection, dotPosition, dotStartPos;

    Rigidbody2D rb;
    Animator anim;
    SpriteRenderer sr;

    Button jumpButton;

    int extraJumps = 1;
    float airTimeCount;
    float lastTouchTime;
    bool grounded;
    bool inAir;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        joystick = GameObject.Find("Joystick").GetComponent<RectTransform>();
        joystickArea = GameObject.Find("JoystickArea").GetComponent<RectTransform>();
        dot = GameObject.Find("Dot").GetComponent<RectTransform>();
        jumpButton = GameObject.Find("JumpButton").GetComponent<Button>();

        if (photonView.IsMine)
        {
            jumpButton.onClick.RemoveAllListeners();
            jumpButton.onClick.AddListener(() => Jump());
        }

        dotStartPos = dot.position;
    }

    void Awake()
    {
        if (photonView.IsMine)
        {
            playerCamera.SetActive(true);
            nickname.text = PhotonNetwork.NickName;
        }
        else
        {
            nickname.text = photonView.Owner.NickName;
            nickname.color = Color.red;
        }
    }

    void Update()
    {
        if (photonView.IsMine && !disableInputs)
        {
            CheckInput();
            HandleJoystick();
        }
    }

    void CheckInput()
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).tapCount >= 2 && !RectTransformUtility.RectangleContainsScreenPoint(jumpButton.GetComponent<RectTransform>(), Input.GetTouch(0).position))
        {
            if (Time.time - lastTouchTime > 0.5f)
            {
                Shoot();
                lastTouchTime = Time.time;
            }
        }

        if (moveDirection.x < 0)
            photonView.RPC("FlipTrue", RpcTarget.AllBuffered);

        else if (moveDirection.x > 0)
            photonView.RPC("FlipFalse", RpcTarget.AllBuffered);

        if (moveDirection.x != 0)
            anim.SetBool("Walking", true);
        else
            anim.SetBool("Walking", false);

        //if (grounded)
        //{
        //    inAir = false;
        //    rb.velocity = new Vector2(rb.velocity.x, 0f);
        //}
    }

    void Jump()
    {
        if (!photonView.IsMine)
            return;

        grounded = Physics2D.OverlapCircle(playerPos.position, positionRadius, ground);

        string groundedString = grounded ? "Sim" : "Não";
        string extraJumpsString = extraJumps > 0 ? "Sim" : "Não";

        print("Jogador está no chão: " + groundedString);
        print("Jogador tem pulos extras: " + extraJumpsString);

        if (grounded || extraJumps > 0)
        {
            rb.velocity = new Vector2(rb.velocity.x, 0f);
            rb.AddForce(Vector2.up * jumpSpeed, ForceMode2D.Impulse);
            if(!grounded)
                extraJumps--;
        }

        if(extraJumps <= 0 && grounded)
            extraJumps = 1;
    }

    void HandleJoystick()
    {
        if (Input.touchCount > 0)
        {
            Touch oneTouch = Input.GetTouch(0);
            Vector2 touchPosition = oneTouch.position;
            
            if (RectTransformUtility.RectangleContainsScreenPoint(joystickArea, touchPosition))
            {
                if(oneTouch.phase == TouchPhase.Moved)
                {
                    dotPosition = oneTouch.position - (Vector2)joystick.position;
                    dotPosition = Vector2.ClampMagnitude(dotPosition, 50f);

                    dot.position = (Vector2)joystick.position + dotPosition;
                }

                if (oneTouch.phase == TouchPhase.Ended || oneTouch.phase == TouchPhase.Canceled)
                    ResetPositionJoystick();
            }

            else
                ResetPositionJoystick();
        }

        moveDirection = (dot.position - joystick.position).normalized;

        if (moveDirection.x != 0)
            rb.velocity = new Vector2(moveDirection.x * moveSpeed, rb.velocity.y);
        else
            rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void ResetPositionJoystick()
    {
        dotPosition = Vector2.zero;
        dot.position = dotStartPos;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    void Shoot()
    {
        if(sr.flipX == false)
        {
            GameObject obj = PhotonNetwork.Instantiate(bulletObject.name,
            new Vector2(firePosition.transform.position.x,
            firePosition.transform.position.y), Quaternion.identity, 0);
        }

        if(sr.flipX == true)
        {
            GameObject obj = PhotonNetwork.Instantiate(bulletObject.name,
            new Vector2(firePosition.transform.position.x,
            firePosition.transform.position.y), Quaternion.identity, 0);

            obj.GetComponent<PhotonView>().RPC("ChangeDir_Left", RpcTarget.AllBuffered);
        }

        anim.SetBool("Shoot", true);
        disableInputs = true;
    }

    public void ChangeShoot()
    {
        anim.SetBool("Shoot", false);
        disableInputs = false;
    }

    [PunRPC]
    void FlipTrue()
    {
        sr.flipX = true;
        transform.Find("FirePosition").GetComponent<Transform>().position = new Vector2(transform.position.x -0.75f, transform.position.y + 0.075f);
    }

    [PunRPC]
    void FlipFalse()
    {
        sr.flipX = false;
        transform.Find("FirePosition").GetComponent<Transform>().position = new Vector2(transform.position.x + 0.75f, transform.position.y + 0.075f);
    }
}