using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Photon.Pun;
using Photon.Realtime;

public class PhotonManager : MonoBehaviourPunCallbacks
{
    //���� ���� ����
    private readonly string version = "1.0";
    //���� �г��� ����
    private string userId = "Zack";
    //�������� �Է��� TextMeshPro Input Field
    public TMP_InputField userIF;
    //�� �̸��� �Է��� TextMeshPro Input Field
    public TMP_InputField roomNameIF;
    //�� ��Ͽ� ���� �����͸� �����ϱ� ���� ��ųʸ� �ڷ���
    private Dictionary<string, GameObject> rooms = new Dictionary<string, GameObject>();
    //�� ����� ǥ���� ������
    private GameObject roomItemPrefab;
    //RoomItem Prefab�� �߰��� ScrollContents
    public Transform scrollContent;

    void Awake()
    {
        //������ Ŭ���̾�Ʈ�� �� �ڵ� ����ȭ �ɼ� Ȱ��ȭ
        PhotonNetwork.AutomaticallySyncScene = true;
        //���� ���� ����
        PhotonNetwork.GameVersion = version;
        //���� ������ �г��� ����
        //PhotonNetwork.NickName = userId;

        //���� �������� �������� �ʴ� ����Ƚ��
        Debug.Log(PhotonNetwork.SendRate);

        //RoomItem ������ �ε�
        roomItemPrefab = Resources.Load<GameObject>("RoomItem");

        //���� ���� ����
        if (PhotonNetwork.IsConnected == false)
        {
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    void Start()
    {
        //����� �������� �ε�
        userId = PlayerPrefs.GetString("USER_ID", $"USER_{Random.Range(1, 21):00}");
        userIF.text = userId;
        //���� ������ �г��� ���
        PhotonNetwork.NickName = userId;
    }

    //�������� �����ϴ� ����
    public void SetUserId()
    {
        if (string.IsNullOrEmpty(userIF.text))
        {
            userId = $"USER_{Random.Range(1, 21):00}";
        }
        else 
        {
            userId = userIF.text;
        }
        //������ ����
        PlayerPrefs.SetString("USER_ID", userId);
        //���� ������ �г��� ���
        PhotonNetwork.NickName = userId;
    }

    //�� ���� �Է� ���θ� Ȯ���ϴ� ����
    string SetRoomName()
    {
        if (string.IsNullOrEmpty(roomNameIF.text))
        {
            roomNameIF.text = $"ROOM_{Random.Range(1, 101):000}";
        }
        return roomNameIF.text;
    }

    //���� ���� ���� �� ȣ��Ǵ� �ݹ��Լ�
    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected to Master");
        Debug.Log($"PhotonNetwork.InLobby={PhotonNetwork.InLobby}");
        PhotonNetwork.JoinLobby();
    }
    //�κ� ���� �� ȣ��Ǵ� �ݹ��Լ�
    public override void OnJoinedLobby()
    {
        Debug.Log($"PhotonNetwork.InLobby={PhotonNetwork.InLobby}");
        PhotonNetwork.JoinRandomRoom();
    }
    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.Log($"JoinRadom Failed {returnCode}:{message}");
        //���� �����ϴ� �Լ� ����
        OnMakeRoomClick();

        /*
        ���� �Ӽ� ����
        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;
        
        �� ����
        PhotonNetwork.CreateRoom("My Room", ro);
        */
    }

    //�� ������ �Ϸ�� �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnCreatedRoom()
    {
        Debug.Log("Created Room");
        Debug.Log($"Room Name = {PhotonNetwork.CurrentRoom.Name}");
    }

    //�뿡 ������ �� ȣ��Ǵ� �ݹ� �Լ�
    public override void OnJoinedRoom()
    {
        Debug.Log($"PhotonNetwork.InRoom={PhotonNetwork.InRoom}");
        Debug.Log($"Player Count={PhotonNetwork.CurrentRoom.PlayerCount}");

        foreach (var player in PhotonNetwork.CurrentRoom.Players)
        {
            Debug.Log($"{player.Value.NickName}, {player.Value.ActorNumber}");
        }
        /*
        //��Ʈ��ũ ����ȭ�� ĳ���� ����
        //���� ��ġ ������ �迭�� ����
        Transform[] points = GameObject.Find("SpawnPointGroup").GetComponentsInChildren<Transform>();
        int idx = Random.Range(1, points.Length);

        //��Ʈ��ũ�� ĳ���� ����
        PhotonNetwork.Instantiate("Player", points[idx].position, points[idx].rotation, 0);
        */
        //������ Ŭ���̾�Ʈ�� ��� BattleField Scene�� �ε��Ѵ�.(LoadLevel)
        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.LoadLevel("BattleField");
        }
    }

    //�� ����� �����ϴ� �ݹ��Լ�
    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        //������ RoomItem �������� ������ �ӽú���
        GameObject tempRoom = null;

        foreach(var roomInfo in roomList)
        {
            //���� ������ ���
            if (roomInfo.RemovedFromList == true)
            {
                //��ųʸ����� �� �̸����� �˻��� ����� RoomItem �������� ����
                rooms.TryGetValue(roomInfo.Name, out tempRoom);
                //RoomItem ������ ����
                Destroy(tempRoom);
                //��ųʸ����� �ش� �� �̸��� �����͸� ����
                rooms.Remove(roomInfo.Name);
            }
            //�� ������ ����� ���
            else
            {
                //�� �̸��� ��ųʸ��� ���� ��� ���� �߰�
                if (rooms.ContainsKey(roomInfo.Name) == false)
                {
                    //RoomInfo �������� scrollContent ������ �����Ѵ�.
                    GameObject roomPrefab = Instantiate(roomItemPrefab, scrollContent);

                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;

                    //��ųʸ� �ڷ����� ������ �߰�
                    rooms.Add(roomInfo.Name, roomPrefab);
                }
                //�� �̸��� ��ųʸ��� ���°�� �� ������ ����
                else
                {
                    rooms.TryGetValue(roomInfo.Name, out tempRoom);
                    tempRoom.GetComponent<RoomData>().RoomInfo = roomInfo;
                }
            }
            Debug.Log($"Room={roomInfo.Name} : ({roomInfo.PlayerCount}/{roomInfo.MaxPlayers})");
        }
    }

    #region UI_BUTTON_EVENT
    public void OnLoginClick()
    {
        //������ ����
        SetUserId();

        //�������� ������ ������ ����
        PhotonNetwork.JoinRandomRoom();
    }

    public void OnMakeRoomClick()
    {
        SetUserId();

        RoomOptions ro = new RoomOptions();
        ro.MaxPlayers = 20;
        ro.IsOpen = true;
        ro.IsVisible = true;

        //�� ����
        PhotonNetwork.CreateRoom(SetRoomName(), ro);
    }
    #endregion
}
