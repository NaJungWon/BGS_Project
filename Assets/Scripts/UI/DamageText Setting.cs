using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageTextSetting : MonoBehaviour
{
    private Camera mainCamera;
    private float lifeTime = 1f;
    private float timer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // 메인 카메라 참조 가져오기
        mainCamera = Camera.main;
    }

    void OnEnable()
    {
        // 활성화될 때마다 타이머 초기화
        timer = 0f;
    }

    void LateUpdate()
    {
        timer += Time.deltaTime;
        if (timer >= lifeTime)
        {
            if (EnemyManager.Instance != null)
            {
                EnemyManager.Instance.ReturnDamageText(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }
            return;
        }

        if (mainCamera != null)
        {
            // 텍스트가 항상 카메라를 향하도록 회전
            transform.rotation = mainCamera.transform.rotation;
        }
    }
}
