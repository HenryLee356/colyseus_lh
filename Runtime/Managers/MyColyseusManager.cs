//负责处理客户端与服务器的连接、房间管理和消息传递

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Colyseus;
using LucidSightTools;
using Tanks;
using UnityEngine;
using UnityEngine.SceneManagement;


//ColyseusManager<T>继承这个类可以让类变成单例
public class MyColyseusManager : ColyseusManager<MyColyseusManager>
{
    //核心功能与事件
    //事件回调：当收到服务器返回的可用房间列表时触发，通知其他模块更新 UI。
    public delegate void OnRoomsReceived(TanksRoomsAvailable[] rooms);
    public static OnRoomsReceived onRoomsReceived;


    /// <summary>
    /// Room 属性：对外暴露当前加入的房间，状态类型为TanksState（服务器同步的游戏状态）
    /// </summary>
    public ColyseusRoom<TanksState> Room
    {
        get
        {
            return  _roomController.Room;
        }
    }

    /// <summary>
    /// 房间控制器：负责具体的房间操作（如创建、加入、离开）。
    /// </summary>
    [SerializeField]
    private RoomController _roomController;

    private bool isInitialized;

    public static bool IsReady
    {
        get
        {
            return Instance != null;//对象加载完表示准备好了
        }
    }

    private string userName;

    /// <summary>
    ///     The display name for the user
    /// </summary>
    public string UserName
    {
        get { return userName; }
        set { userName = value; }
    }

    /// <summary>
    ///     <see cref="MonoBehaviour" /> callback when a script is enabled just before any of the Update methods are called the
    ///     first time.
    /// </summary>
    protected override void Start()
    {
        // For this example we're going to set the target frame rate
        // and allow the app to run in the background for continuous testing.
        Application.targetFrameRate = 60;
        Application.runInBackground = true;
        
    }

    /// <summary>
    /// 功能：初始化房间控制器，设置房间名称和选项（如地图、模式）。
    ///逻辑：防止重复初始化，创建RoomController实例并配置。
    /// </summary>
    /// <param name="roomName"></param>
    /// <param name="roomOptions"></param>
    public void Initialize(string roomName, Dictionary<string, object> roomOptions)
    {
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        //// Set up room controller
        _roomController = new RoomController { roomName = roomName };
        _roomController.SetRoomOptions(roomOptions);

        SetRoomOptions(roomOptions);
    }



    /// <summary>
    /// 存储房间容器
    /// </summary>
    public Dictionary<string, object> roomOptionsDictionary = new Dictionary<string, object>();

    public void SetRoomOptions(Dictionary<string, object> options)
    {
        roomOptionsDictionary = options;
    }



    /// <summary>
    /// 创建 Colyseus 客户端实例，连接到服务器，注册事件监听（如连接成功、断开连接等）
    /// Initialize the client
    /// </summary>
    public override void InitializeClient()
    {
        base.InitializeClient();

        _roomController.SetClient(client);
    }


    /// <summary>
    /// 功能：异步获取指定类型的可用房间列表。
    /// 回调：获取成功后触发onRoomsReceived事件。
    /// </summary>
    public async void GetAvailableRooms()
    {
        TanksRoomsAvailable[] rooms = await client.GetAvailableRooms<TanksRoomsAvailable>(_roomController.roomName);

        Debug.LogWarning(rooms.Length+"//"+_roomController.roomName);

        //onRoomsReceived?.Invoke(rooms);
    }

    /// <summary>
    /// 功能：通过房间 ID 加入已存在的房间。
    ///参数：isNewJoin可能用于区分是新加入还是重新连接。
    /// </summary>
    /// <param name="roomID"></param>
    /// <param name="isNewJoin"></param>
    public async void JoinExistingRoom(string roomID, bool isNewJoin)
    {
        await _roomController.JoinRoomId(roomID, isNewJoin);
        LoadNextScene((newScene) => {
            //Invoke(nameof(createAllEnemy), 1f);
            createAllEnemy();
            GameManager.Instance.gamestate = GameState.RUNING;
        });
    }


    /// <summary>
    /// 功能：创建指定 ID 的房间（通常用于私人房间）。
    /// </summary>
    /// <param name="roomID"></param>
    public async void CreateNewRoom(string roomID)
    {
        await _roomController.CreateSpecificRoom(client, _roomController.roomName, roomID);
        LoadNextScene((newScene) => {

            //Invoke(nameof(createAllEnemy),1f);

            GameManager.Instance.gamestate = GameState.RUNING;

        });
    }


    public async void joinOrCreateRoom() {
        await _roomController.JoinOrCreateRoom();
        LoadNextScene((newScene) => {
            createAllEnemy();
            GameManager.Instance.gamestate = GameState.RUNING;
        });
    }



    private void createAllEnemy() {
        foreach (var player in _roomController._users.Values)
        {
            if (player.playerId != _roomController.myID)
            {
                GameManager.Instance.createEnemyPlayer(player.playerId);//如果不是自己就创建敌人预制体玩家
            }
        }

        
    }


    private void LoadNextScene(Action<Scene> onComplete)
    {
        StartCoroutine(LoadSceneAsync("war", onComplete));
    }


    /// <summary>
    /// 获取当前场景
    //异步加载新场景（使用 additive 模式，即不卸载当前场景）
    //等待场景加载进度达到 90%（Unity 的异步加载最多到 90%，剩下的 10% 需要手动激活）
    //执行回调函数（例如创建或加入房间）
    //允许场景激活（完成最后的 10% 加载）
    //卸载当前场景
    /// </summary>
    /// <param name="scene"></param>
    /// <param name="onComplete"></param>
    /// <returns></returns>
    //private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    //{
    //    Scene currScene = SceneManager.GetActiveScene();
    //    AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
    //    while (op.progress <= 0.9f)
    //    {
    //        //Wait until the scene is loaded
    //        yield return new WaitForEndOfFrame();
    //    }

    //    onComplete.Invoke();
    //    op.allowSceneActivation = true;
    //    SceneManager.UnloadSceneAsync(currScene);
    //}

    private IEnumerator LoadSceneAsync(string scene, Action<Scene> onComplete)
    {
        Scene currScene = SceneManager.GetActiveScene();

        // 加载新场景
        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        op.allowSceneActivation = false;

        // 等待加载完成
        while (op.progress < 0.9f)
            yield return null;

        op.allowSceneActivation = true;

        // 等待场景完全激活
        while (!op.isDone)
            yield return null;

        // 获取新加载的场景
        Scene newScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        // 设置新场景为活动场景
        SceneManager.SetActiveScene(newScene);


        // 传递新场景到回调函数
        onComplete?.Invoke(newScene);

        // 卸载当前场景
        yield return SceneManager.UnloadSceneAsync(currScene);
    }


    /// <summary>
    /// 功能：离开所有房间，并在完成后执行回调。
    /// </summary>
    /// <param name="onLeave"></param>
    public async void LeaveAllRooms(Action onLeave)
    {
        await _roomController.LeaveAllRooms(true, onLeave);
    }

    /// <summary>
    ///     On detection of <see cref="OnApplicationQuit" /> will disconnect
    ///     from all <see cref="rooms" />.
    /// </summary>
    private void CleanUpOnAppQuit()
    {
        if (client == null)
        {
            return;
        }

        _roomController.CleanUp();
    }

    /// <summary>
    /// 功能：应用退出时清理资源，确保优雅断开网络连接。
    ///     <see cref="MonoBehaviour" /> callback that gets called just before app exit.
    /// </summary>
    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();

        _roomController.LeaveAllRooms(true);

        CleanUpOnAppQuit();
    }

#if UNITY_EDITOR
    public void OnEditorQuit()
    {
        OnApplicationQuit();
    }
#endif

    /// <summary>
    ///     Send an action and message object to the room.
    /// </summary>
    /// <param name="action">The action to take</param>
    /// <param name="message">The message object to pass along to the room</param>
    public static void NetSend(string action, object message = null)
    {
        if (Instance._roomController.Room == null)
        {
            Debug.LogError($"Error: Not in room for action {action} msg {message}");
            return;
        }

        _ = message == null
            ? Instance._roomController.Room.Send(action)
            : Instance._roomController.Room.Send(action, message);
    }


    /// <summary>
    /// 获取房间列表
    /// </summary>
    /// <returns></returns>
    public async Task<ColyseusRoomAvailable[]> GetRoomListAsync()
    {
        return await _roomController.GetRoomListAsync();
    }


    //public async Task<int> GetRoomListAsync()
    //{
    //    ColyseusRoomAvailable[] allrooms= await _roomController.GetRoomListAsync();
    //    return allrooms.Length;
    //}

}