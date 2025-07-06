using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using System.Threading.Tasks;
using System;
using GameDevWare.Serialization;
using System.Linq;

public enum GameState {
    INIT,
    READY,
    RUNING,
    PAUSE,
    OVER
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public GameState gamestate;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); }
    }


    /// <summary>
    /// 存储房间容器
    /// </summary>
    private Dictionary<string, object> roomOptionsDictionary = new Dictionary<string, object>();

    private ColyseusRoom<Tanks.TanksState> room;
    //private ColyseusClient _client;




    // Use this for initialization
    async void Start()
    {
        Debug.Log("enter game manager");

        

        gamestate = GameState.INIT;

        Dictionary<string, object> roomOptions = new Dictionary<string, object>();
        MyColyseusManager.Instance.Initialize("TanksRoom", roomOptions);//这个房间名称必须是服务器约定的房间名称
        
        CreateUser();

        //MyColyseusManager.Instance.InitializeClient();//必须先初始化房间再用，不然初始化房间的时候会将client重置


        //FindAndJoinRoom();

        MyColyseusManager.Instance.joinOrCreateRoom();


        //ColyseusRoomAvailable[] rooms = await MyColyseusManager.Instance.GetRoomListAsync();
        //if (rooms.Length > 0)
        //{

        //    Debug.Log("有房间加入房间");
        //    MyColyseusManager.Instance.UserName = "joinZhangSan";//定义创建者玩家昵称
        //    MyColyseusManager.Instance.JoinExistingRoom(rooms[0].roomId, true);
        //}
        //else
        //{
        //    Debug.Log("没有房间创建房间");
        //    MyColyseusManager.Instance.UserName = "kakaxi";//定义加入者玩家昵称
        //    CreateRoom();
        //}





        //GetAvailableRooms();

        //Invoke(nameof(sendTestMessage),1f);





    }





    // 改进后的房间查找和加入逻辑
    async void FindAndJoinRoom()
    {
        try
        {
            // 获取所有可用房间列表
            ColyseusRoomAvailable[] rooms = await MyColyseusManager.Instance.GetRoomListAsync();

            // 过滤出可加入的房间（未锁定且未满）
            var joinableRooms = rooms
                .Where(r =>  r.clients < r.maxClients)
                .ToList();

            if (joinableRooms.Count > 0)
            {
                // 选择玩家最少的房间加入
                var bestRoom = joinableRooms.OrderBy(r => r.clients).First();

                Debug.Log($"加入房间: {bestRoom.roomId}, 当前玩家: {bestRoom.clients}/{bestRoom.maxClients}");
                MyColyseusManager.Instance.UserName = "joinZhangSan";

                try
                {
                    // 尝试加入选中的房间
                    MyColyseusManager.Instance.JoinExistingRoom(bestRoom.roomId, true);
                }
                catch (Exception e)
                {
                    throw e; // 抛出加入失败错误
                }
            }
            else
            {
                // 没有可加入的房间，创建新房间
                if (rooms.Length > 0)
                {
                    Debug.Log($"所有现有房间({rooms.Length}个)都已满或已锁定，创建新房间");
                }
                else
                {
                    Debug.Log("没有找到房间，创建新房间");
                }

                MyColyseusManager.Instance.UserName = "kakaxi";
                CreateRoom();
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"连接失败: {e.Message}");
            // 可以在这里添加重试逻辑或显示错误UI
        }
    }











    public void CreateUser()
    {

        ColyseusSettings clonedSettings = MyColyseusManager.Instance.CloneSettings();
        clonedSettings.colyseusServerAddress = "LOCALHOST";
        clonedSettings.colyseusServerPort = "2567";
        clonedSettings.useSecureProtocol = false;

        MyColyseusManager.Instance.OverrideSettings(clonedSettings);

        MyColyseusManager.Instance.InitializeClient();//必须先初始化房间再用，不然初始化房间的时候会将client重置

        MyColyseusManager.Instance.UserName = "kakaxi";//这个是用户名,昵称

        //if (isCreator) MyColyseusManager.Instance.UserName = "kakaxi";//这个是创建者ID
        //else { MyColyseusManager.Instance.UserName = "joinZhangSan"; }

    }


    public void CreateRoom()
    {
        MyColyseusManager.Instance.CreateNewRoom("roomname");//这个是房间名称，由创建者命名
    }



    void GetAvailableRooms()
    {

        //colyseus服务端不需要开发者自己同步大厅房间列表过来，底层做了同步，在客户端可以直接获取房间列表
        MyColyseusManager.Instance.GetAvailableRooms();
    }


    public GameObject enemyPrefab;


    public IndexedDictionary<float,GameObject> enemyDiction=new IndexedDictionary<float, GameObject>();

    public void createEnemyPlayer(float playerID) {
      
        GameObject enemyobj = Instantiate(enemyPrefab, new Vector3(1f,1f,0f), Quaternion.identity);
        enemyDiction.Add(playerID, enemyobj);
    }


    public void destroyLeave(float playerID) {
        Destroy(enemyDiction[playerID]);
        enemyDiction.Remove(playerID);
    }


    public void updateEnemyPositon(int playerId, Tanks.Vector2 coords) {
        Debug.LogWarning("ID:"+ playerId+"==="+ coords.x);
        //enemyDiction[playerId].transform.position.Set(coords.x,coords.y, enemyDiction[playerId].transform.position.z);
        // 正确方式：创建新的Vector3并赋值
        enemyDiction[playerId].transform.position = new Vector3(coords.x, coords.y, enemyDiction[playerId].transform.position.z);
    }



    // Update is called once per frame
    void Update()
    {

    }
}
