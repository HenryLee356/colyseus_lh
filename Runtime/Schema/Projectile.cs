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
	public partial class Projectile : Schema {
#if UNITY_5_3_OR_NEWER
[Preserve]
#endif
public Projectile() { }
		[Type(0, "string")]
		public string key = default(string);

		[Type(1, "ref", typeof(Vector2))]
		public Vector2 coords = null;
	}
}
