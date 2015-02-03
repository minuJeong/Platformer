using UnityEngine;
using System.Collections;

public class AutoBoxCollider : MonoBehaviour {

    public bool force3D = false;

	void Awake () {
#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
        if (!force3D) {
            Rigidbody body = GetComponent<Rigidbody>();
            bool rotateFreeze = ((int)body.constraints & (int)RigidbodyConstraints.FreezeRotationZ) > 0;
            bool continuous   = body.collisionDetectionMode == CollisionDetectionMode.Continuous;
            if (body != null) DestroyImmediate(body);

            BoxCollider   box   = GetComponent<BoxCollider  >();
            BoxCollider2D box2D = GetComponent<BoxCollider2D>();
            if (box != null && box2D == null) {
                Vector3 center = box.center;
                Vector3 size   = box.size;
                DestroyImmediate(box);

                box2D        = gameObject.AddComponent<BoxCollider2D>();
                box2D.center = new Vector2(center.x, center.y);
                box2D.size   = new Vector2(size  .x, size  .y);

            } else if (box2D == null) {
                box2D        = gameObject.AddComponent<BoxCollider2D>();
            }

            Rigidbody2D body2D = GetComponent<Rigidbody2D>();
            if (body2D == null) body2D                        = gameObject.AddComponent<Rigidbody2D>();
            if (rotateFreeze  ) body2D.fixedAngle             = true;
            if (continuous    ) body2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        } else if (GetComponent<BoxCollider>() == null) {
            gameObject.AddComponent<BoxCollider>();
        }
#else
		if (GetComponent<BoxCollider>() == null) {
			gameObject.AddComponent<BoxCollider>();
		}
#endif
		Destroy (this);
	}
}
