using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAnimationEventReceiver : MonoBehaviour
{
    public Bs_Breath breathComponent;
    public BossPattern bossPattern;
    public Bs_DashAttack bs_DashAttack;

    private void Start()
    {
        
    }

    public void StartBreath()
    {
        if (bossPattern != null && bossPattern.bossSetting != null && bossPattern.bossSetting.playerInRoom)
        {
            breathComponent.BeginBreath();
        }
    }

    public void StopBreath()
    {
        if (bossPattern != null && bossPattern.bossSetting != null && bossPattern.bossSetting.playerInRoom)
        {
            breathComponent.EndBreath();
        }
    }

    public void OnTornadoPattern()
    {
        if (bossPattern != null && bossPattern.bossSetting != null && bossPattern.bossSetting.playerInRoom)
        {
            Debug.Log("Tornado pattern triggered");
            bossPattern.SummonTornado();
        }
    }

    public void OnDashAttackPattern()
    {
        if (bossPattern != null && bossPattern.bossSetting != null && bossPattern.bossSetting.playerInRoom)
        {
            Debug.Log("Dash");
            StartCoroutine(bs_DashAttack.Execute());
        }
    }
}
