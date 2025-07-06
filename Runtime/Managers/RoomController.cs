using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Colyseus;
using Colyseus.Schema;
using GameDevWare.Serialization;
using NativeWebSocket;
using Tanks;
using UnityEngine;
using Vector2 = UnityEngine.Vector2;

/// <summary>
/// 基于 Colyseus 框架的坦克游戏房间控制器，负责管理客户端与服务器间的房间连接、状态同步和事件处理
///-管理与 Colyseus 服务器的房间连接
///-处理房间状态同步和事件广播
///-管理玩家和 projectile（炮弹）对象
///-提供房间创建、加入和离开的接口
///     Manages the rooms of a server connection.
/// </summary>
[Serializable]
public class RoomController
{
    // Network Events
    //==========================
    //通过 C# 委托实现的事件系统，用于通知 UI 或游戏逻辑
    //Custom game delegate functions
    //======================================
    public delegate void OnRoomStateChanged(TanksState state, bool isFirstState);
    public static event OnRoomStateChanged onRoomStateChanged;//UI 可以订阅onRoomStateChanged来更新游戏状态显示

    public delegate void OnUserStateChanged(Player state);

    /// <summary>
    ///     The Client that is created when connecting to the Colyseus server.
    /// </summary>
    private ColyseusClient _client;

    public delegate void OnWorldChanged(List<DataChange> changes);
    public static event OnWorldChanged onWorldChanged;

    public delegate void OnWorldGridChanged(string index, float value);
    public static event OnWorldGridChanged onWorldGridChanged;

    public delegate void OnPlayerChange(int playerId, List<DataChange> changes);
    public static event OnPlayerChange onPlayerChange;

    public delegate void OnProjectileAdded(Projectile projectile);
    public static event OnProjectileAdded onProjectileAdded;

    public delegate void OnProjectileRemoved(Projectile cachedProjectile);
    public static event OnProjectileRemoved onProjectileRemoved;

    public delegate void OnProjectileUpdated(Projectile projectile, List<DataChange> changes);
    public static event OnProjectileUpdated onProjectileUpdated;

    public delegate void OnTankMoved(int player, Tanks.Vector2 newCoords);
    public static OnTankMoved onTankMoved;

    //==========================

    /// <summary>
    ///     The current or active Room we get when joining or creating a room.
    /// </summary>
    private ColyseusRoom<TanksState> _room;

    /// <summary>
    /// 玩家管理：
    ///    OnUserAdd：添加玩家并监听位置变化
    ///    OnUserRemove：移除玩家并清理资源
    ///    炮弹管理：
    ///     OnProjectileAdd：生成炮弹并监听移动
    ///     OnProjectileRemove：销毁炮弹并触发特效

    ///     Collection for tracking users that have joined the room.
    /// </summary>
    public IndexedDictionary<string, Player> _users =
        new IndexedDictionary<string, Player>();

    /// <summary>
    /// 存储自己的playerID
    /// </summary>
    public float myID = 0;

    private Dictionary<string, Projectile> _projectiles = new Dictionary<string, Projectile>();

    /// <summary>
    ///     The name of the room clients will attempt to create or join on the Colyseus server.
    /// </summary>
    public string roomName = "NO_ROOM_NAME_PROVIDED";

    private Dictionary<string, object> roomOptionsDictionary = new Dictionary<string, object>();

    /// <summary>
    ///     All the connected rooms.
    /// </summary>
    public List<IColyseusRoom> rooms = new List<IColyseusRoom>();

    public ColyseusRoom<TanksState> Room
    {
        get { return _room; }
    }

    public void SetRoomOptions(Dictionary<string, object> options)
    {
        roomOptionsDictionary = options;
    }

    /// <summary>
    ///     Set the client of the <see cref="ColyseusRoomManager" />.
    /// </summary>
    /// <param name="client"></param>
    public void SetClient(ColyseusClient client)
    {
        _client = client;
    }

    /// <summary>
    /// 创建带指定 ID 的房间（通常用于私人房间）
    ///     Create a room with the given roomId.
    /// </summary>
    /// <param name="roomId">The ID for the room.</param>
    public async Task CreateSpecificRoom(ColyseusClient client, string roomName, string roomId)
    {
        Debug.LogWarning($"Creating Room {roomId}");

        try
        {
            //Populate an options dictionary with custom options provided elsewhere as well as the critical option we need here, roomId
            Dictionary<string, object> options = new Dictionary<string, object> {["roomId"] = roomId, ["creatorId"] = MyColyseusManager.Instance.UserName };//将房间创建者的ID设为自己的ID
            foreach (KeyValuePair<string, object> option in roomOptionsDictionary)
            {
                options.Add(option.Key, option.Value);
            }

            _room = await client.Create<Tanks.TanksState>(roomName, options);//创建房间，无需手动同步，会自动同步房间数据
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to create room {roomId} : {ex.Message}");
            return;
        }

        Debug.LogWarning($"Created Room: {_room.Id}");
        RegisterRoomHandlers();
    }

    /// <summary>
    /// 功能：自动选择可用房间加入，或创建新房间
    ///重试逻辑：连接失败时会自动重试（每 5 秒一次）
    ///     Join an existing room or create a new one using <see cref="roomName" /> with no options.
    ///     <para>Locked or private rooms are ignored.</para>
    /// </summary>
    public async Task JoinOrCreateRoom(Action<bool> onComplete = null)//onComplete回调函数参数，用于在当前函数执行完成后，自动触发一些后续操作
    {
        try
        {
            Debug.LogWarning($"Join Or Create Room - Name = {roomName}.... ");

            // Populate an options dictionary with custom options provided elsewhere
            Dictionary<string, object> options = new Dictionary<string, object>() { ["joiningId"] = MyColyseusManager.Instance.UserName };//将房间创建者的ID设为自己的ID
            foreach (KeyValuePair<string, object> option in roomOptionsDictionary)
            {
                options.Add(option.Key, option.Value);
            }

            _room = await _client.JoinOrCreate<Tanks.TanksState>(roomName, options);//加入或创建房间，无需手动同步，会自动同步房间数据
        }
        catch (Exception ex)
        {
            Debug.LogError($"Room Controller Error - {ex.Message + ex.StackTrace}");

            //?. 是 C# 中的安全调用符，作用是：如果 onComplete 不为 null，才执行后面的 Invoke；如果为 null，则跳过，不报错。
            onComplete?.Invoke(false); //等价于 onComplete(false);

            return;
        }

        onComplete?.Invoke(true);//等价于 onComplete(true);

        Debug.LogWarning($"Joined / Created Room: {_room.Id}");

        RegisterRoomHandlers();
    }

    /// <summary>
    /// 功能：优雅地离开所有房间
    ///参数：consented（是否主动离开）
    /// </summary>
    /// <param name="consented"></param>
    /// <param name="onLeave"></param>
    /// <returns></returns>
    public async Task LeaveAllRooms(bool consented, Action onLeave = null)
    {
        if (_room != null && rooms.Contains(_room) == false)
        {
            await _room.Leave(consented);
        }

        for (int i = 0; i < rooms.Count; i++)
        {
            await rooms[i].Leave(consented);
        }

        _users.Clear();

        ClearRoomHandlers();

        onLeave?.Invoke();
    }

    /// <summary>
    /// 核心方法：注册所有房间事件监听器
    ///    监听的事件：
    ///OnStateChange：房间状态变更
    ///players.OnAdd/OnRemove：玩家加入 /xw 离开
    ///projectiles.OnAdd/OnRemove：炮弹生成 / 销毁
    ///coords.OnChange：玩家或炮弹位置变更
    ///     Subscribes the manager to <see cref="room" />'s networked events
    ///     and starts measuring latency to the server.
    /// </summary>
    public virtual void RegisterRoomHandlers()
    {
        _room.OnLeave += OnLeaveRoom;


        _room.OnStateChange += OnStateChangeHandler;
        

        ////Custom game logic
        ////========================
        //_room.State.world.OnChange += OnWorldChange;

        //_room.State.world.grid.OnChange += OnWorldGridChange;

        _room.State.players.OnAdd += OnUserAdd;
        _room.State.players.OnRemove += OnUserRemove;

        //_room.State.projectiles.OnAdd += OnProjectileAdd;
        //_room.State.projectiles.OnRemove += OnProjectileRemove;
        //========================

        _room.State.TriggerAll();

        _room.colyseusConnection.OnError += Room_OnError;
        _room.colyseusConnection.OnClose += Room_OnClose;


        //自定义消息
        _room.OnMessage<float>("yourID",(data)=> {
            Debug.Log("recived myID:"+data);
            myID = data;
        });
    }

    

    private void OnLeaveRoom(WebSocketCloseCode code)
    {
        Debug.Log("ROOM: ON LEAVE =- Reason: " + code);
        
        _room = null;
    }

    /// <summary>
    ///     Unsubscribes <see cref="Room" /> from networked events."/>
    /// </summary>
    private void ClearRoomHandlers()
    {
        if (_room == null)
        {
            return;
        }

        _room.OnStateChange -= OnStateChangeHandler;

        //_room.State.world.OnChange -= OnWorldChange;
        //_room.State.world.grid.OnChange -= OnWorldGridChange;
        //_room.State.projectiles.OnAdd -= OnProjectileAdd;
        //_room.State.projectiles.OnRemove -= OnProjectileRemove;

        _room.State.players.OnAdd -= OnUserAdd;
        _room.State.players.OnRemove -= OnUserRemove;

        _room.colyseusConnection.OnError -= Room_OnError;
        _room.colyseusConnection.OnClose -= Room_OnClose;

        _room.OnLeave -= OnLeaveRoom;

        _room = null;
    }

    /// <summary>
    ///     Asynchronously gets all the available rooms of the <see cref="_client" />
    ///     named <see cref="roomName" />
    /// </summary>
    public async Task<ColyseusRoomAvailable[]> GetRoomListAsync()
    {
        Debug.Log("roomname;;: " + roomName);
        Debug.LogWarning(roomName);
        return await _client.GetAvailableRooms(roomName);

    }

    /// <summary>
    ///     Join a room with the given <see cref="roomId" />.
    /// </summary>
    /// <param name="roomId">ID of the room to join.</param>
    public async Task JoinRoomId(string roomId, bool isNewJoin)
    {
        ClearRoomHandlers();

        try
        {
            while (_room == null || !_room.colyseusConnection.IsOpen)
            {
                Dictionary<string, object> options = new Dictionary<string, object>();

                options.Add("joiningId", MyColyseusManager.Instance.UserName);

                _room = await _client.JoinById<Tanks.TanksState>(roomId, options);//加入房间，房间数据自动同步

                if (_room == null || !_room.colyseusConnection.IsOpen)
                {
                    Debug.LogWarning($"Failed to Connect to {roomId}.. Retrying in 5 Seconds...");
                    await Task.Delay(5000);
                }
            }
            Debug.LogWarning($"Connected to {roomId}..");
            
            RegisterRoomHandlers();
        }
        catch (Exception ex)
        {
            Debug.LogError(ex.Message);
            Debug.LogError("Failed to join room");
        }
    }

    /// <summary>
    ///     Callback for when a <see cref="ExampleNetworkedUser" /> is added to a room.
    /// </summary>
    /// <param name="user">The user object</param>
    /// <param name="key">The user key</param>
    private void OnUserAdd(string key, Tanks.Player user)
    {
        Debug.LogError("-----" + user.playerId);
        // Add "player" to map of players
        _users.Add(key, user);
        

        //如果游戏正在进行说明加入的是其他玩家，所以需把其他玩家创建
        if (GameManager.Instance.gamestate == GameState.RUNING && user.playerId!=myID) {
            GameManager.Instance.createEnemyPlayer(user.playerId);
        }


            user.coords.OnChange += coordChanges =>
        {
            if(GameManager.Instance.gamestate == GameState.RUNING) enemyPlayerMove((int)user.playerId, user.coords);
            //onTankMoved?.Invoke((int)user.playerId, user.coords);
        };

        // On entity update...
        user.OnChange += changes =>
        {
            onPlayerChange?.Invoke((int)user.playerId, changes);
        };
    }

    private void enemyPlayerMove(int playerId, Tanks.Vector2 coords)
    {
        if (playerId != myID) {
            Debug.Log("enemy moving:"+ playerId+"||" + coords.x+"//"+coords.y);//不是自己就更新位置
            GameManager.Instance.updateEnemyPositon(playerId,coords);
        }
    }



    
    /// <summary>
    ///     Callback for when a user is removed from a room.
    /// </summary>
    /// <param name="user">The removed user.</param>
    /// <param name="key">The user key.</param>
    private void OnUserRemove(string key, Player/*ExampleNetworkedUser*/ user)
    {
        Debug.LogWarning($"user [{user.__refId} | {user.sessionId/*id*/} | key {key}] Left");

        GameManager.Instance.destroyLeave(_users[key].playerId);

        _users.Remove(key);

        

    }

    /// <summary>
    ///     Callback for when the room's connection closes.
    /// </summary>
    /// <param name="closeCode">Code reason for the connection close.</param>
    private static void Room_OnClose(WebSocketCloseCode closeCode)
    {
        Debug.LogError("Room_OnClose: " + closeCode);
    }

    /// <summary>
    /// 错误处理：记录网络错误和连接关闭原因
    //自动重连：在JoinRoomId中实现了失败自动重试逻辑
    ///     Callback for when the room get an error.
    /// </summary>
    /// <param name="errorMsg">The error message.</param>
    private static void Room_OnError(string errorMsg)
    {
        Debug.LogError("Room_OnError: " + errorMsg);
    }

    /// <summary>
    ///     Callback when the room state has changed.
    /// </summary>
    /// <param name="state">The room state.</param>
    /// <param name="isFirstState">Is it the first state?</param>
    private static void OnStateChangeHandler(Tanks.TanksState state, bool isFirstState)
    {
        // Setup room first state
        onRoomStateChanged?.Invoke(state, isFirstState);
    }

    public async void CleanUp()
    {
        List<Task> leaveRoomTasks = new List<Task>();

        foreach (IColyseusRoom roomEl in rooms)
        {
            leaveRoomTasks.Add(roomEl.Leave(false));
        }

        if (_room != null)
        {
            leaveRoomTasks.Add(_room.Leave(false));
        }

        await Task.WhenAll(leaveRoomTasks.ToArray());
    }
}




//这个控制器是多人坦克游戏的核心网络组件，负责：

//房间管理（创建、加入、离开）
//状态同步（玩家位置、炮弹、游戏世界）
//事件广播（通知游戏逻辑更新 UI 或执行动作）
//错误处理（网络异常、连接断开）

//它通过事件系统实现了与游戏逻辑的解耦，使 UI 和游戏系统可以独立响应网络事件