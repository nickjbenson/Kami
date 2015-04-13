using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour {

	public float speed;
	public float angularSpeed;
	private int count;

	void Start ()
	{
		count = 0;
	}

	void FixedUpdate() 
	{
		///FORWARD MOTION
//		float moveForward = Input.GetAxis ("Vertical");
		float moveForward = 0.0f;
		if (Input.GetKey ("up")) { ///up
			moveForward = 1.0f;
		}
		if (Input.GetKey ("down")) { ///down
			moveForward = -1.0f;
		}
		Vector3 movement = new Vector3(0.0f, 0.0f, moveForward);
		transform.Translate (movement * speed * Time.deltaTime);
		///GetComponent<Rigidbody>().AddRelativeForce(movement * speed * Time.deltaTime);

		///RIGHT/LEFT ROTATION
		float rotate_y = Input.GetAxis ("Horizontal");
		transform.Rotate (0.0f, rotate_y * angularSpeed * Time.deltaTime, 0.0f);///, Space.World);

		///UP/DOWN ROTATION
		float x = 0.0f;
		if (Input.GetKey ("w")) { ///up
			x = -1.0f;
		}
		if (Input.GetKey ("s")) { ///down
			x = 1.0f;
		}
		transform.Rotate (x * angularSpeed * Time.deltaTime, 0.0f, 0.0f);///, Space.World);
	}

	void OnTriggerEnter(Collider other)
	{
		if(other.gameObject.tag == "PickUp"){
			other.gameObject.SetActive(false);
			count += 1;
		}
	}
}
