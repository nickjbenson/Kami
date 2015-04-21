using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public PlayerController player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position;
	}

	void FixedUpdate() {
		float y = Input.GetAxis ("Horizontal");
		float x = -Input.GetAxis ("Vertical");
		float angularSpeed = player.angularSpeed;
//		float x = 0.0f;
//		float y = 0.0f;

//		if (Input.GetKey ("a")) { ///left
//			y = -1.0f;
//		}
//		if (Input.GetKey ("d")) { ///right
//			y = 1.0f;
//		}
//		if (Input.GetKey ("w")) { ///up
//			x = -1.0f;
//		}
//		if (Input.GetKey ("s")) { ///down
//			x = 1.0f;
//		}

		///if (x + transform.rotation.x > 0) { /// don't let it get below ground (x ~ 0)
		///	x = -transform.rotation.x;
		///}

		transform.Rotate(x * angularSpeed * Time.deltaTime, 0.0f, 0.0f, Space.Self);
		transform.Rotate (0.0f, y * angularSpeed * Time.deltaTime, 0.0f, Space.World);
	}
	
	void LateUpdate () {
//		transform.position = player.transform.position + offset;
		///transform.rotation = player.transform.rotation;
	}

}
