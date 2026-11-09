using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bs_DashAttack : MonoBehaviour
{
    private BossPattern bossPattern;

    public Transform L_dashStart, L_dashEnd, R_dashStart, R_dashEnd;
    private Transform dashStart, dashEnd;
    public Transform mouthTransform;

    public Vector3 origin;

    public GameObject warningPrefab;
    public float warningTime = 1.5f, dashTime = 1f, returnTime = 0f;

    public Animator animator;
    public float dashDamage = 200f;

    public Vector3 damageBoxSize = new Vector3(5f, 5f, 5f); // 데미지 영역 박스 크기

    public GameObject warning;

    public GameObject earthShatterPrefab;
    //public AudioSource dashAudio;
    private GameObject dashEffectInstance;
    private bool isFlame = false;

    // Start is called before the first frame update
    void Start()
    {
        bossPattern = this.GetComponent<BossPattern>();
        origin = this.transform.position;

        L_dashStart = GameObject.Find("L_dashStart").transform;
        L_dashEnd = GameObject.Find("L_dashEnd").transform;
        R_dashStart = GameObject.Find("R_dashStart").transform;
        R_dashEnd = GameObject.Find("R_dashEnd").transform;
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void DashWarning()
    {
        Debug.Log("Dash Warning");

        // 경고 표시 생성 위치: 대시 시작점
        Vector3 warningPos = dashStart.position;

        // 경고 표시의 회전: 대시 방향으로
        Vector3 dir = dashEnd.position - dashStart.position;
        Quaternion warningRot = Quaternion.LookRotation(dir);

        // 경고 표시 생성
        warning = Instantiate(warningPrefab, warningPos, warningRot);

        // 경고 표시의 크기(Scale) 조정: z축을 대시 길이만큼 늘림
        float dashLength = dir.magnitude;
        Vector3 scale = warning.transform.localScale;
        warning.transform.localScale = new Vector3(scale.x, scale.y, dashLength);
    }

    public void StartDash()
    {
        Debug.Log("Dash Start");

        transform.position = dashStart.position;
    }

    public void EndDash()
    {
        Debug.Log("Dash end");

        if (warning != null)
            Destroy(warning);

        if (dashEffectInstance != null)
        {
            var ps = dashEffectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var emission = ps.emission;
                emission.enabled = false; // 파티클만 중지
                Destroy(dashEffectInstance, ps.main.startLifetime.constantMax); // 남은 파티클이 사라질 때 Destroy
            }
            else
            {
                Destroy(dashEffectInstance);
            }
            dashEffectInstance = null;
        }
    }

    public void FinishDash()
    {
        Debug.Log("Dash finish");
        if (bossPattern != null)
            bossPattern.OnDashAttackFinished();
    }

    public IEnumerator Execute()
    {
        if (bossPattern == null || !bossPattern.bossSetting.playerInRoom) yield break;

        Vector3 originPosition = transform.position;
        Quaternion originRotation = transform.rotation;

        animator.SetTrigger("Fly");

        DashPathType randomPath = (Random.Range(0, 2) == 0) ? DashPathType.LeftToRight : DashPathType.RightToLeft;
        SetDashPath(randomPath);

        yield return new WaitForSeconds(2.6f);

        if (!bossPattern.bossSetting.playerInRoom) yield break;

        Vector3 dashDir = (dashEnd.position - dashStart.position).normalized;
        if (dashDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(dashDir);

        animator.SetTrigger("StartDash");

        StartDash();

        isFlame = true;

        yield return StartCoroutine(MoveTo(dashStart.position, dashEnd.position, dashTime, true));

        if (!bossPattern.bossSetting.playerInRoom) yield break;

        isFlame = false;

        EndDash();

        transform.rotation = originRotation;

        yield return StartCoroutine(MoveTo(dashEnd.position, origin, returnTime, false));

        if (!bossPattern.bossSetting.playerInRoom) yield break;

        animator.SetTrigger("Land");
        FinishDash();
    }

    public enum DashPathType { LeftToRight, RightToLeft }

    public void SetDashPath(DashPathType pathType)
    {
        if (pathType == DashPathType.LeftToRight)
        {
            dashStart = L_dashStart;
            dashEnd = L_dashEnd;
        }
        else
        {
            dashStart = R_dashStart;
            dashEnd = R_dashEnd;
        }
    }

    IEnumerator MoveTo(Vector3 from, Vector3 to, float duration, bool applyDamage)
    {
        if (applyDamage)
        {
            Vector3 fireDir = (mouthTransform.forward + mouthTransform.up * 135f).normalized;
            Quaternion fireRot = Quaternion.LookRotation(fireDir, mouthTransform.up);

            dashEffectInstance = Instantiate(
                earthShatterPrefab,
                mouthTransform.position,
                fireRot,
                mouthTransform // 부모가 필요하면 mouthTransform, 아니면 null
            );
            //dashAudio.Play();

            // 파티클 속도 조정 설정
            var ps = dashEffectInstance.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                var main = ps.main;
                main.startSpeed = 50f; // 원하는 속도로 조정
            }

            isFlame = false;
        }

        bool damageGiven = true;
        float t = 0f;
        while (t < duration)
        {
            transform.position = Vector3.Lerp(from, to, t / duration);
            t += Time.deltaTime;

            if (applyDamage && damageGiven)
            {
                Vector3 boxCenter = transform.position;
                Collider[] hits = Physics.OverlapBox(boxCenter, damageBoxSize / 2, Quaternion.identity);
                foreach (var hit in hits)
                {
                    if (hit.CompareTag("Player"))
                    {
                        PlayerStatus playerStatus = hit.GetComponent<PlayerStatus>();
                        Hitdecision hitdecision = hit.GetComponent<Hitdecision>();
                        if (hitdecision != null && playerStatus != null)
                        {
                            hitdecision.ApplyDamage(playerStatus, dashDamage);
                            Debug.Log($"Player hit by dash attack! Damage: {dashDamage}");
                            damageGiven = false;
                        }
                    }
                }
            }

            yield return null;
        }
        transform.position = to;
    }

    void OnDrawGizmos()
    {
        // 데미지 영역의 박스 표시 (디버그용)
        Gizmos.color = Color.red;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, damageBoxSize);
    }
}
