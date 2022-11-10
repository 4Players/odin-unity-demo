using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OdinNative.Unity.Samples
{
	/// <summary>
	/// Demo Example where everything in #region Demo does not correlate with Odin
	/// </summary>
	public class DemoPlayer : MonoBehaviour
	{
		#region Demo
		public float moveSpeed = 7;
		public float smoothMoveTime = .1f;
		public float turnSpeed = 8;

		private float Angle;
		private float SmoothInputMagnitude;
		private float SmoothMoveVelocity;
		private Vector3 Velocity;

		private Rigidbody Body;

		void Start()
		{
			Body = GetComponent<Rigidbody>();
		}

		void Update()
		{
			Vector3 inputDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
			float inputMagnitude = inputDirection.magnitude;
			SmoothInputMagnitude = Mathf.SmoothDamp(SmoothInputMagnitude, inputMagnitude, ref SmoothMoveVelocity, smoothMoveTime);

			float targetAngle = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg;
			Angle = Mathf.LerpAngle(Angle, targetAngle, Time.deltaTime * turnSpeed * inputMagnitude);

			Velocity = transform.forward * moveSpeed * SmoothInputMagnitude;
		}

		void FixedUpdate()
		{
			Body.MoveRotation(Quaternion.Euler(Vector3.up * Angle));
			Body.MovePosition(Body.position + Velocity * Time.deltaTime);
		}

		private Vector3 MoveOffset;
		private float MouseZCoord;

		private void OnMouseDown()
		{
			MouseZCoord = Camera.main.WorldToScreenPoint(gameObject.transform.position).z;
			MoveOffset = gameObject.transform.position - GetMouseWorldPos();
		}

		private Vector3 GetMouseWorldPos()
		{
			Vector3 mousePoint = Input.mousePosition;
			mousePoint.z = MouseZCoord;

			return Camera.main.ScreenToWorldPoint(mousePoint);
		}

		private void OnMouseDrag()
		{
			gameObject.transform.position = MoveOffset + GetMouseWorldPos();
		}
		#endregion Demo
	}
}
