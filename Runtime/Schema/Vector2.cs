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
	public partial class Vector2 : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public Vector2() { }
		[Type(0, "number")]
		public float x = default(float);

		[Type(1, "number")]
		public float y = default(float);
	}
}
