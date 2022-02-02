using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;
using Photon.Realtime;
using Photon.Pun;

public class Movement : MonoBehaviour
{
    //������Ʈ�� ĳ�� ó���ϱ� ���� ����(�ʵ�)
    private CharacterController controller;
    private new Transform transform;
    private Animator animator;
    private new Camera camera;

    //������ Plane�� ����ĳ�����ϱ����� ����
    private Plane plane;
    private Ray ray;
    private Vector3 hitPoint;

    //PhotonView ������Ʈ ĳ�� ó���� ���� ����
    private PhotonView pv;

    //Cinemachine ���� ī�޶� ������ ����
    private CinemachineVirtualCamera virtualCamera;

    public float moveSpeed = 10.0f;

    //���ŵ� ��ġ�� ȸ������ ������ ����
    private Vector3 receivePos;
    private Quaternion receiveRot;

    //���ŵ� ��ǥ�� �̵� �� ȸ���ӵ��� �ΰ���
    public float damping = 10.0f;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        transform = GetComponent<Transform>();
        animator = GetComponent<Animator>();
        camera = Camera.main;

        pv = GetComponent<PhotonView>();
        virtualCamera = GameObject.FindObjectOfType<CinemachineVirtualCamera>();

        //PhotonView�� �ڽ��� ���� ��� �ó׸ӽ� ����ī�޶� ����
        if (pv.IsMine)
        {
            virtualCamera.Follow = transform;
            virtualCamera.LookAt = transform;
        }

        //������ �ٴ��� ���ΰ��� ��ġ�� �������� ����
        plane = new Plane(transform.up, transform.position);
    }

    float v => Input.GetAxis("Vertical");
    float h => Input.GetAxis("Horizontal");

    void Update()
    {
        if (pv.IsMine)
        {
            Move();
            Turn();
        }
        else
        {
            transform.position = Vector3.Lerp(transform.position, receivePos, Time.deltaTime * damping);
            transform.rotation = Quaternion.Lerp(transform.rotation, receiveRot, Time.deltaTime * damping);
        }
    }
    void Move()
    {
        Vector3 cameraForward = camera.transform.forward;
        Vector3 cameraRight = camera.transform.right;
        cameraForward.y = 0.0f;
        cameraRight.y = 0.0f;

        //�̵��� ���⺤�� ���
        Vector3 moveDir = (cameraForward * v) + (cameraRight * h);
        moveDir.Set(moveDir.x, 0.0f, moveDir.z);

        //���ΰ� ĳ���� �̵�ó��
        controller.SimpleMove(moveDir * moveSpeed);

        //���ΰ� ĳ������ �ִϸ��̼� ó��
        float forward = Vector3.Dot(moveDir, transform.forward);
        float strafe = Vector3.Dot(moveDir, transform.right);

        animator.SetFloat("Forward", forward);
        animator.SetFloat("Strafe", strafe);
    }

    void Turn()
    {
        //���콺�� 2���� ��ǩ���� �̿��� 3���� ����(����)�� �����Ѵ�.
        ray = camera.ScreenPointToRay(Input.mousePosition);

        float enter = 0.0f;

        //������ �ٴڿ� ���̸� �߻��� �ߵ��� ������ �Ÿ��� enter������ ��ȯ
        plane.Raycast(ray, out enter);

        //������ �ٴڿ� ���̰� �浹�� ��ǩ�� ����
        hitPoint = ray.GetPoint(enter);

        //ȸ���ؾ��� ������ ���͸� ���
        Vector3 lookDir = hitPoint - transform.position;
        lookDir.y = 0;

        //���ΰ� ĳ������ ȸ���� ����
        transform.localRotation = Quaternion.LookRotation(lookDir);
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //�ڽ��� ���� ĳ������ ��� �ڽ��� �����͸� �ٸ� ��Ʈ��ũ �������� �۽�
        if (stream.IsWriting)
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else
        {
            receivePos = (Vector3)stream.ReceiveNext();
            receiveRot = (Quaternion)stream.ReceiveNext();
        }
    }

}
