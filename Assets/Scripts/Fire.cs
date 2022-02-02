using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Fire : MonoBehaviour
{
    public Transform firePos;
    public GameObject bulletPrefab;
    private ParticleSystem muzzleFlash;

    private PhotonView pv;
    private bool isMouseClick => Input.GetMouseButtonDown(0);

    void Start()
    {
        pv = GetComponent<PhotonView>();
        muzzleFlash = firePos.Find("MuzzleFlash").GetComponent<ParticleSystem>();
    }

    void Update()
    {
        if(pv.IsMine && isMouseClick)
        {
            //탄환 발사 유저에 고유번호를 붙여 함수를 실행
            FireBullet(pv.Owner.ActorNumber);
            //FireBullet();
            //RPC로 원격지에 있는 함수를 호출
            pv.RPC("FireBullet", RpcTarget.Others, pv.Owner.ActorNumber);//함수 동기화 시점, 내 오브젝트임과 마우스 클릭이 확인되었을 때
        }
    }
    [PunRPC]// 변수를 동기화 하는 것이 아닌 함수를 서버와 동기화한다. 원격지의 함수를 호출한다.
    void FireBullet(int actorNo)
    {
        if (!muzzleFlash) muzzleFlash.Play(true);

        GameObject bullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);

        bullet.GetComponent<Bullet>().actorNumber = actorNo;
    }
}
/*
탄 발사와 같은 이벤트성 동작을 네트워크 유저와 공유할 때, RPC(Remote Procedure Calls)를 통해 구현하는 것이 일반적이다
RPC는 원격 프로시저 호출이란 의미로 물리적으로 떨어져있는 다른 디바이스의 함수를 호출하는 기능이다. RPC함수 호출시 네트워크를 통해 다른 사용자의 스크립트에서 해당함수가 호출된다.
 */