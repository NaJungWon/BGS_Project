using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpGainRadiusSetting : MonoBehaviour
{
    private GameObject player;
    public PlayerStatus playerStatus;
    public float moveSpeed = 20f; // 이동 속도 설정
    private List<Collider> expColliders = new List<Collider>(); // Exp 오브젝트의 Collider 리스트

    // Start is called before the first frame update
    void Start()
    {
        player = transform.parent.gameObject;
        playerStatus = player.GetComponent<PlayerStatus>();
    }

    // Update is called once per frame
    void Update()
    {
        // player의 위치를 지속적으로 업데이트
        if (player != null)
        {
            // 영역 내의 Exp 오브젝트를 찾아서 끌어당김
            for (int i = expColliders.Count - 1; i >= 0; i--)
            {
                Collider hitCollider = expColliders[i];
                if (hitCollider == null || !hitCollider.gameObject.activeSelf)
                {
                    expColliders.RemoveAt(i);
                    continue;
                }

                if (hitCollider.CompareTag("Exp"))
                {
                    // player 방향으로의 벡터 계산
                    Vector3 direction = (player.transform.position - hitCollider.transform.position).normalized;
                    // Translate를 사용하여 이동
                    hitCollider.transform.Translate(direction * moveSpeed * Time.deltaTime, Space.World);
                }
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Exp"))
        {
            // Exp 오브젝트의 Collider를 Trigger로 설정
            Collider expCollider = other.GetComponent<Collider>();
            if (expCollider != null && expCollider.gameObject.activeSelf)
            {
                expCollider.isTrigger = true; // Trigger 활성화
                expColliders.Add(other); // 활성화된 Exp 오브젝트만 리스트에 추가
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Exp"))
        {
            expColliders.Remove(other);
        }
    }
}
