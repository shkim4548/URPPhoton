using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using Player = Photon.Realtime.Player;

public class Damage : MonoBehaviourPunCallbacks
{
    //사망 후 투명 처리를 위한 MeshRenderer 컴포넌트의 배열
    private Renderer[] renderers;

    //캐릭터의 초기 생명치
    private int initHp = 100;

    //캐릭터의 현재 생명치
    public int currHp = 100;

    private Animator anim;
    private CharacterController cc;

    //애니메이터 뷰에 생성한 파라미터의 해시값 추출
    private readonly int hashDie = Animator.StringToHash("Die");
    private readonly int hashRespwan = Animator.StringToHash("Respawn");

    //GameManager접근을 위한 변수
    private GameManager gameManager;

    void Awake()
    {
        //캐릭터 모델의 모든 Renderer 컴포넌트를 추출한 후 배열에 할당
        renderers = GetComponentsInChildren<Renderer>();
        anim = GetComponent<Animator>();
        cc = GetComponent<CharacterController>();

        //현재 생명치를 초기 생명치로 초깃값 설정
        currHp = initHp;

        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    
    void OnCollisionEnter(Collision coll)
    {
        //생명 수치가 0보다 크고 충돌체의 태그가 BULLET인 경우에 생명 수치를 차감
        if(currHp >0 && coll.collider.CompareTag("BULLET"))
        {
            currHp -= 20;
            if (currHp <= 0)
            {
                //자신의 phtonView일때만 메시지를 출력
                if (photonView.IsMine)
                {
                    //충돌한 탄의 ActorNumber를 추출
                    var actorNo = coll.collider.GetComponent<Bullet>().actorNumber;
                    //ActorNumber로 현재 룸에 입장한 플레이어를 추출
                    Player lastShootPlayer = PhotonNetwork.CurrentRoom.GetPlayer(actorNo);

                    //메시지 출력을 위한 문자열 포맷
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
        //메시지 출력
        gameManager.msgList.text += msg;
    }


    IEnumerator PlayerDie()
    {
        //CharacterController 컴포넌트 비활성화
        cc.enabled = false;
        //리스폰 비활성화
        anim.SetBool(hashRespwan, false);
        //캐릭터 사망 애니메이션 실행
        anim.SetTrigger(hashDie);

        yield return new WaitForSeconds(3.0f);

        //리스폰 활성화
        anim.SetBool(hashRespwan, true);

        //캐릭터 투명처리
        SetPlayerVisible(false);

        yield return new WaitForSeconds(1.5f);

        //생성위치를 재조정
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);

        //리스폰시 생명 초깃값 설정
        currHp = 100;
        //캐릭터를 다시 보이게 처리
        SetPlayerVisible(true);
        //CharacterController 컴포넌트 활성화
        cc.enabled = true;
    }

    //Renderer 컴포넌트를 활성/비활성화 하는 함수
    void SetPlayerVisible(bool isVisible)
    {
        for(int i = 0; i < renderers.Length; i++)
        {
            renderers[i].enabled = isVisible;
        }
    }
}
/*
모든 렌더러(Renderer)의 일반적인 기능을 갖고있는 클래스를 나타냅니다.

렌더러는 오브젝트가 스크린에 나타내도록 해줍니다. 
오브젝트, 메쉬 또는 파티클 시스템의 렌더러에 접근하기 위해 이 클래스를 사용합니다. 
렌더러는 오브젝트를 보이지 않도록 하기위해서, 비활성화 될 수 있고(enabled 확인), 렌더러를 통해서 해당 재질(Material)의 접근과 수정이 가능합니다. (material 확인)
 */
