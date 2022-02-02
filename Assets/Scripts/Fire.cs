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
            //źȯ �߻� ������ ������ȣ�� �ٿ� �Լ��� ����
            FireBullet(pv.Owner.ActorNumber);
            //FireBullet();
            //RPC�� �������� �ִ� �Լ��� ȣ��
            pv.RPC("FireBullet", RpcTarget.Others, pv.Owner.ActorNumber);//�Լ� ����ȭ ����, �� ������Ʈ�Ӱ� ���콺 Ŭ���� Ȯ�εǾ��� ��
        }
    }
    [PunRPC]// ������ ����ȭ �ϴ� ���� �ƴ� �Լ��� ������ ����ȭ�Ѵ�. �������� �Լ��� ȣ���Ѵ�.
    void FireBullet(int actorNo)
    {
        if (!muzzleFlash) muzzleFlash.Play(true);

        GameObject bullet = Instantiate(bulletPrefab, firePos.position, firePos.rotation);

        bullet.GetComponent<Bullet>().actorNumber = actorNo;
    }
}
/*
ź �߻�� ���� �̺�Ʈ�� ������ ��Ʈ��ũ ������ ������ ��, RPC(Remote Procedure Calls)�� ���� �����ϴ� ���� �Ϲ����̴�
RPC�� ���� ���ν��� ȣ���̶� �ǹ̷� ���������� �������ִ� �ٸ� ����̽��� �Լ��� ȣ���ϴ� ����̴�. RPC�Լ� ȣ��� ��Ʈ��ũ�� ���� �ٸ� ������� ��ũ��Ʈ���� �ش��Լ��� ȣ��ȴ�.
 */