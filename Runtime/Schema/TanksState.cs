// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 3.0.39
// 

using Colyseus.Schema;
#if UNITY_5_3_OR_NEWER
using UnityEngine.Scripting;
#endif

namespace Tanks {
	public partial class TanksState : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public TanksState() { }
		[Type(0, "ref", typeof(GameRulesSchema))]
		public GameRulesSchema gameRules = null;

		[Type(1, "map", typeof(MapSchema<Player>))]
		public MapSchema<Player> players = new MapSchema<Player>();

		[Type(2, "array", typeof(ArraySchema<Weapon>))]
		public ArraySchema<Weapon> weapons = null;

		[Type(3, "ref", typeof(World))]
		public World world = null;

		[Type(4, "map", typeof(MapSchema<Projectile>))]
		public MapSchema<Projectile> projectiles = null;

		[Type(5, "string")]
		public string gameState = default(string);

		[Type(6, "string")]
		public string previousGameState = default(string);

		[Type(7, "number")]
		public float currentTurn = default(float);

		[Type(8, "number")]
		public float turnNumber = default(float);

		[Type(9, "string")]
		public string statusMessage = default(string);

		[Type(10, "boolean")]
		public bool inProcessOfQuitingGame = default(bool);
	}
}
