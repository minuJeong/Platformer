using UnityEngine;
using System.Collections;

public class Toss : MonoBehaviour {
	
	Vector3 start;
	
	public GameObject visual;
	public bool swipeToJump = false;
	
	#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
	Rigidbody2D body;
	#else
	Rigidbody body;
	#endif
	
	void Start () {
		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		body = rigidbody2D;
		body.gravityScale = 1;
		#else
		body = rigidbody;
		body.useGravity = true;
		#endif
		visual.GetComponent<Ferr2D_Animator>().SetAnimation("fly");
	}
	
	void Update () {
		
		#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
		if (body.gravityScale > 0)
			#else
			if (body.useGravity)
				#endif
		{
			visual.transform.rotation = Quaternion.FromToRotation(Vector3.right, body.velocity);
			visual.transform.eulerAngles -= new Vector3(0, 0, 90);
		}
		
		if (Input.GetButtonDown("Fire1"))
		{
			start = Input.mousePosition;
		}
		else if (Input.GetButtonUp("Fire1"))
		{
			Ray   ray  = Camera.main.ScreenPointToRay(Input.mousePosition);
			float dist = 0;
			new Plane(-Vector3.forward, transform.position.z).Raycast(ray, out dist);
			
			if (swipeToJump)
				body.AddForce( (Input.mousePosition - start) * 2.1f );
			else
				body.AddForce( (ray.GetPoint(dist) - transform.position) * 100f );
			
			visual.GetComponent<Ferr2D_Animator>().SetAnimation("jump");
			#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
			body.gravityScale = 1;
			#else
			body.useGravity = true;
			#endif
		}
	}
	
	#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
	void OnCollisionEnter2D(Collision2D collision)
		#else
		void OnCollisionEnter(Collision collision)
			#endif
	{
		//if (collision.gameObject.isStatic)
		{
			visual.GetComponent<Ferr2D_Animator>().SetAnimation("land");
			#if !(UNITY_4_2 || UNITY_4_1 || UNITY_4_1 || UNITY_4_0 || UNITY_3_5 || UNITY_3_4 || UNITY_3_3 || UNITY_3_1 || UNITY_3_0)
			body.gravityScale = 0;
			#else
			body.useGravity = false;
			#endif
			body.velocity = Vector3.zero;
			body.Sleep();
			
			visual.transform.rotation = Quaternion.FromToRotation(Vector3.right, collision.contacts[0].normal);
			
			if (visual.transform.eulerAngles.y != 0)
			{
				visual.transform.eulerAngles = new Vector3(0, 0, visual.transform.eulerAngles.z - 270);
			}
			else
			{
				visual.transform.eulerAngles = new Vector3(0, 0, visual.transform.eulerAngles.z - 90);
			}
		}
	}
}
