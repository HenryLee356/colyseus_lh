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
	public partial class Player : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public Player() { }
		[Type(0, "string")]
		public string sessionId = default(string);

		[Type(1, "string")]
		public string readyState = default(string);

		[Type(2, "number")]
		public float playerId = default(float);

		[Type(3, "string")]
		public string name = default(string);

		[Type(4, "number")]
		public float hp = default(float);

		[Type(5, "ref", typeof(Vector2))]
		public Vector2 coords = null;

		[Type(6, "number")]
		public float currentWeapon = default(float);

		[Type(7, "number")]
		public float aimAngle = default(float);

		[Type(8, "number")]
		public float currentMovement = default(float);

		[Type(9, "number")]
		public float currentActionPoints = default(float);

		[Type(10, "number")]
		public float timestamp = default(float);

		[Type(11, "boolean")]
		public bool connected = default(bool);
	}
}
