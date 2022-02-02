using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;

public class Damage : MonoBehaviourPunCallbacks
{
    //��� �� ���� ó���� ���� MeshRenderer ������Ʈ�� �迭
    private Renderer[] renderers;

    //ĳ������ �ʱ� ����ġ
    private int initHp = 100;

    //ĳ������ ���� ����ġ
    public int currHp = 100;

    private Animator anim;
    private CharacterController cc;

    //�ִϸ����� �信 ������ �Ķ������ �ؽð� ����
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashRespwan = Animator.StringToHash("Respawn");

    //GameManager������ ���� ����
    private GameManager gameManager;

    void Awake()
    {
        //ĳ���� ���� ��� Renderer ������Ʈ�� ������ �� �迭�� �Ҵ�
        renderers = GetComponentsInChildren<Renderer>();
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

        //���� ����ġ�� �ʱ� ����ġ�� �ʱ갪 ����
        currHp = initHp;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    
    void OnCollisionEnter(Collision coll)
    {
        //���� ��ġ�� 0���� ũ�� �浹ü�� �±װ� BULLET�� ��쿡 ���� ��ġ�� ����
        if(currHp >0 && coll.collider.CompareTag("BULLET"))
        {
            currHp -= 20;
            if (currHp <= 0)
            {
                //�ڽ��� phtonView�϶��� �޽����� ���
                if (photonView.IsMine)
                {
                    //�浹�� ź�� ActorNumber�� ����
                    var actorNo = coll.collider.GetComponent<Bullet>().actorNumber;
                    //ActorNumber�� ���� �뿡 ������ �÷��̾ ����
                    Player lastShootPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);

                    //�޽��� ����� ���� ���ڿ� ����
                    string msg = string.Format($"\n < color = #00ff00 >{0}</ color > is killed by <color=#ff0000>{1}", photonView.Owner.NickName, lastShootPlayer.NickName);
                    photonView.RPC("KillMessage", RpcTarget.AllBufferedViaServer, msg);
                }
                StartCoroutine(PlayerDie());
            }
        }
    }
    [PunRPC]
    void KillMessage(string msg)
    {
        //�޽��� ���
        gameManager.msgList.text += msg;
    }


    IEnumerator PlayerDie()
    {
        //CharacterController ������Ʈ ��Ȱ��ȭ
        cc.enabled = false;
        //������ ��Ȱ��ȭ
        anim.SetBool(hashRespwan, false);
        //ĳ���� ��� �ִϸ��̼� ����
        anim.SetTrigger(hashDie);

        yield return new WaitForSeconds(3.0f);

        //������ Ȱ��ȭ
        anim.SetBool(hashRespwan, true);

        //ĳ���� ����ó��
        SetPlayerVisible(false);

        yield return new WaitForSeconds(1.5f);

        //������ġ�� ������
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);

        //�������� ���� �ʱ갪 ����
        currHp = 100;
        //ĳ���͸� �ٽ� ���̰� ó��
        SetPlayerVisible(true);
        //CharacterController ������Ʈ Ȱ��ȭ
        cc.enabled = true;
    }

    //Renderer ������Ʈ�� Ȱ��/��Ȱ��ȭ �ϴ� �Լ�
    void SetPlayerVisible(bool isVisible)
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = isVisible;
        }
    }
}
/*
��� ������(Renderer)�� �Ϲ����� ����� �����ִ� Ŭ������ ��Ÿ���ϴ�.

�������� ������Ʈ�� ��ũ���� ��Ÿ������ ���ݴϴ�. 
������Ʈ, �޽� �Ǵ� ��ƼŬ �ý����� �������� �����ϱ� ���� �� Ŭ������ ����մϴ�. 
�������� ������Ʈ�� ������ �ʵ��� �ϱ����ؼ�, ��Ȱ��ȭ �� �� �ְ�(enabled Ȯ��), �������� ���ؼ� �ش� ����(Material)�� ���ٰ� ������ �����մϴ�. (material Ȯ��)
 */
