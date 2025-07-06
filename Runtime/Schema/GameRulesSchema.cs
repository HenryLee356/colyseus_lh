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
	public partial class GameRulesSchema : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public GameRulesSchema() { }
		[Type(0, "int32")]
		public int MaxActionPoints = default(int);

		[Type(1, "int32")]
		public int MovementActionPointCost = default(int);

		[Type(2, "int32")]
		public int FiringActionPointCost = default(int);

		[Type(3, "int32")]
		public int ProjectileSpeed = default(int);

		[Type(4, "int32")]
		public int MaxHitPoints = default(int);

		[Type(5, "int32")]
		public int MovementTime = default(int);
	}
}
