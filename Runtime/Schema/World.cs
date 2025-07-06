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
	public partial class World : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public World() { }
		[Type(0, "number")]
		public float width = default(float);

		[Type(1, "number")]
		public float height = default(float);

		[Type(2, "map", typeof(MapSchema<float>), "number")]
		public MapSchema<float> grid = null;
	}
}
