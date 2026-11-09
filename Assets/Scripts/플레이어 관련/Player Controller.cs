using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public UpgradeInGame upgradeInGame;
    PlayerStatus playerStatus;
    // 전역 변수로 선언된 입력값 수정
    private float hInput;
    private float vInput;
    private bool cInput;

    public AimingSetting aimSet;

    [Header("플레이어 설정")]
    // 이동 속도 설정
    //public float moveSpeed = 5f;
    // 회전 속도 설정
    public float rotateSpeed = 10f;
    // 공격 설정
    //public float attackCooldown = 3f;
    public float attackTimer = 0f;
    public float attackRotateSpeed = 30f;
    public float attackAnimSpeed = 1f; // 기본 공격 애니메이션 재생 속도
    
    public bool AutoAiming = false;
    public bool canMove = true; // 이동 가능 여부
    public bool isMove = false; // 이동중 여부
    public bool useESkill = false; // E 스킬 사용 여부
    public bool useQSkill = false; // Q 스킬 사용 여부
    public bool isAttack = false; // 공격 중 여부
    public bool isSlow = false; // 슬로우 중 여부

    public GameObject player;
    public Animator anim;
    //public GameObject playerDfAttack; //캐릭터 기본 공격

    public Quaternion attackDirection; // 공격 시의 정면 방향 저장

    public GameObject DirectionTest; // 저장된 정면방향 확인용

    private float originalSpeed; // 원래 속도를 저장할 변수 추가
    private float currentSlowDuration; // 현재 슬로우 지속 시간

    // Start is called before the first frame update
    void Start()
    {
        upgradeInGame = GetComponent<UpgradeInGame>();
        playerStatus = GetComponent<PlayerStatus>();
        //attackCooldown = playerStatus.attackSpeed;
        //moveSpeed = playerStatus.speed;
        player = transform.GetChild(0).gameObject;
        //playerDfAttack = transform.GetChild(4).GetChild(0).gameObject;
        aimSet = GetComponent<AimingSetting>();
        anim = player.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        GetInput();
    }
    
    void FixedUpdate()
    {
        Attack();

        if(canMove)
        {
            Move();
        }
        UpdateDirectionTest();
    }


    void GetInput()
    {
        // 지역 변수 선언 제거하고 전역 변수에 직접 할당
        hInput = 0f;
        vInput = 0f;

        if (Input.GetKey(KeyCode.D)) hInput = 1f;
        if (Input.GetKey(KeyCode.A)) hInput = -1f;
        if (Input.GetKey(KeyCode.W)) vInput = 1f;
        if (Input.GetKey(KeyCode.S)) vInput = -1f;

        // C 키를 눌렀을 때 AutoAiming의 값을 토글
        if (Input.GetKeyDown(KeyCode.C))
        {
            if(AutoAiming) AutoAiming = false;
            else if(!AutoAiming) AutoAiming = true;
        }
    }

    void Move()
    {
        // 이동 입력을 벡터로 변환
        Vector3 moveDirection = new Vector3(hInput, 0f, vInput);

        if (moveDirection != Vector3.zero)
        {
            isMove = true;
            anim.SetBool("IsMove", true);
            moveDirection.Normalize();

            // 이동 적용 (일반 이동)
            transform.Translate(moveDirection * playerStatus.speed * Time.fixedDeltaTime, Space.World);

            if (!isAttack)
            {
                // 일반 이동 시 플레이어 회전
                Quaternion toRotation = Quaternion.LookRotation(moveDirection, Vector3.up);
                player.transform.rotation = Quaternion.Lerp(player.transform.rotation, toRotation, rotateSpeed * Time.fixedDeltaTime);

                // 일반 이동 시에는 원래 입력값을 그대로 적용
                anim.SetFloat("HInput", hInput);
                anim.SetFloat("VInput", vInput);
            }
            else
            {
                // 공격 중일 때는 공격 방향 기준으로 현재 이동 방향 변환
                Vector3 attackForward = attackDirection * Vector3.forward;
                Vector3 attackRight = attackDirection * Vector3.right;

                // 이동 방향이 공격 방향에 맞춰 적절히 변환된 입력값으로 전달
                float adjustedHInput = Vector3.Dot(moveDirection, attackRight);
                float adjustedVInput = Vector3.Dot(moveDirection, attackForward);

                // 애니메이터에 변환된 값 적용 (공격 시)
                anim.SetFloat("HInput", adjustedHInput);
                anim.SetFloat("VInput", adjustedVInput);
            }
        }
        else
        {
            isMove = false;
            anim.SetBool("IsMove", false);
            anim.SetFloat("HInput", 0f);
            anim.SetFloat("VInput", 0f);
        }
    }

    public void LaunchPlayer(Vector3 force, float duration)
    {
        StartCoroutine(LaunchRoutine(force, duration));
    }

    private IEnumerator LaunchRoutine(Vector3 force, float duration)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.AddForce(force, ForceMode.Impulse);

            canMove = false;
            yield return new WaitForSeconds(duration);
            canMove = true;
        }
    }

    public void ApplySlow(float slowMultiplier, float duration)
    {
        if (!isSlow)
        {
            currentSlowDuration = duration;
            StartCoroutine(SlowRoutine(slowMultiplier, duration));
        }
        else
        {
            // 이미 슬로우 상태라면 지속 시간만 갱신
            currentSlowDuration = duration;
        }
    }

    private IEnumerator SlowRoutine(float slowMultiplier, float duration)
    {
        if (!isSlow)
        {
            playerStatus.speed *= slowMultiplier;        // 이동 속도 감소
            isSlow = true;                      // 슬로우 상태로 변경
        }

        while (currentSlowDuration > 0)
        {
            yield return new WaitForSeconds(0.1f);
            currentSlowDuration -= 0.1f;
        }

        upgradeInGame.PlayerStatusUpgradeSetting(
            upgradeInGame.playerInfo,
            upgradeInGame.upgradeWeaponValues,
            upgradeInGame.upgradeAbilityValues,
            upgradeInGame.abilityCardValues,
            upgradeInGame.abilityPassiveValues
        );
        isSlow = false;          // 슬로우 상태 해제
    }

    void Attack()
    {
        anim.SetLayerWeight(1, 1);
        if(!useESkill && !useQSkill)
        {
            attackTimer += Time.deltaTime;
        }

        if (attackTimer >= playerStatus.attackSpeed && !isAttack)
        {
            isAttack = true;
            // 공격 시작 시 aimingdirection의 방향 저장
            Vector3 direction = aimSet.aimingdirection.transform.forward;
            direction.y = 0f; // y축 회전 방지
            attackDirection = Quaternion.LookRotation(direction, Vector3.up);
            
            // 기본 공격 애니메이션 속도 설정
            anim.SetFloat("AttackSpeed", attackAnimSpeed);
            anim.SetTrigger("Attack");
        }

        if (anim.GetCurrentAnimatorStateInfo(1).IsName("Df Attack"))
        {
            isAttack = true;
            // 저장된 방향으로 회전
            player.transform.rotation = Quaternion.Lerp(player.transform.rotation, attackDirection, attackRotateSpeed * Time.deltaTime);
        }
        else
        {
            if (isAttack)
            {
                // 애니메이션이 종료되었을 때 attackTimer를 0으로 설정
                attackTimer = 0f;
            }
            isAttack = false;
        }
    }

    void UpdateDirectionTest()
    {
        if (DirectionTest != null)
        {
            // attackDirection 방향으로 거리 5만큼 이동
            Vector3 offset = attackDirection * Vector3.forward * 2f;
            DirectionTest.transform.position = player.transform.position + offset;
        }
    }
}
