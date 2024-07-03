using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using System;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private CinemachineVirtualCameraBase cam;
    //[SerializeField] CinemachineVirtualCamera enemyCam;

    Transform mainTarget;

    private void Awake()
    {
        Architecture.Player.onOwnerSpawn.AddListener(Plyer_Spawned);
        //EventsHolder.onCritPunch.AddListener(Crit_Punched);
        //EventsHolder.onBtnCamClicked.AddListener(Cam_Clicked);
        EventsHolder.onStickmanDestroyed.AddListener(CountCharacters_Changed);
        EventsHolder.playerSpawnedAny.AddListener(CountCharacters_Changed);
    }

    private void CountCharacters_Changed(Player _)
    {
        SetEnemyCamTarget();
    }

    private void Cam_Clicked()
    {
        //SetEnemyCamTarget();

        //if (enemyCam.Priority == 9)
        //{
        //    enemyCam.Priority = 11;
        //}
        //else
        //{
        //    enemyCam.Priority = 9;
        //}
    }

    void SetEnemyCamTarget()
    {
        //var enemies = GameManager.Instance.allPlayers.FindAll(p => p.Team == Team.Hostile && !p.Ragdoll.IsDestroyed && p.gameObject.activeInHierarchy);

        //if (enemies.Count > 0)
        //{
        //    enemyCam.Follow = enemies[0].Hip;
        //}
        //else
        //{
        //    enemyCam.Follow = mainTarget;
        //}
    }

    private void Crit_Punched(Player player)
    {
        cam.Follow = player.Hip;
        LeanTween.delayedCall(3f, () => cam.Follow = mainTarget);
    }

    void Plyer_Spawned(Architecture.Player target)
    {
        mainTarget = target.transform;
        cam.Follow = target.transform;
    }
}
