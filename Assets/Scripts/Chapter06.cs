using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using UnityEngine.Assertions;

// 큐브 처음 1회 시계방향으로 180도 스핀
// 카메라는 그 주변을 비스듬히 위에서 반시계방향으로 선회하여 큐브를 계속 비춤
// 상하 키 : 월드 좌표계의 x축에서 회전
// 좌우 키 : z축에서 회전
public class Chapter06 : MonoBehaviour {

	private GameObject cube;

	private float cubeRotationTime;
	
	private Quaternion cubeRotationFrom;
	private Quaternion cubeRotationTo;

	private bool spinning = true;
	private bool rotating;

	// Use this for initialization
	void Start () {
		cube = GameObject.Find("Cube");
	}

	// Update is called once per frame
	void Update () {
		// 큐브의 반시계 방향 180도 회전
		if (spinning) {
			// Quaternion.AngleAxix() : 각도와 회전축으로부터 그 회전을 실행하는 사원수를 산출
			// 반시계 방향 -> -180.0f로 음수값을 지정, 축 : 로컬 좌표의 위쪽(Vector3.up)
			Quaternion cubeSpinRotation = Quaternion.AngleAxis (-180.0f, Vector3.up);
			// 원래 위치로부터 Quaternion.Slerp에서 지정한 페이스로 회전 
			cube.transform.rotation = Quaternion.Slerp (cube.transform.rotation, cubeSpinRotation, 0.05f);
		}

		// 스크립트가 추가된 카메라를 제어
		// Quaternion.LookRotation() : 벡터를 사원수로 변환하는 메서드
		// 카메라 위치(transform.position)에서 큐브의 약간 위쪽(cube.transform.position + new Vector3(0, 0.5f, 0))을 향하는 벡터를 사원수로 변환
		Quaternion cameraRotation = Quaternion.LookRotation(cube.transform.position + new Vector3(0, 0.5f, 0) - transform.position);
		// 벡터의 방향을 Quaternion.Slerp를 사용해 서서히 카메라의 기울기에 반영
		transform.rotation = Quaternion.Slerp(transform.rotation, cameraRotation, Time.deltaTime);
		transform.Translate(0.02f, 0.005f, 0.5f * Time.deltaTime);

		// 키 입력이 있으면 ResetCubeRotation으로 대응하는 회전축 설정, rotation -> true로 하여 큐브에 회전을 적용해간다
		// cubeRotationFrom : 회전 전 / cubeRotationTo : 회전 후
		if (rotating)	{
			cubeRotationTime += Time.deltaTime / 0.5f;
			cube.transform.rotation = Quaternion.Slerp(cubeRotationFrom, cubeRotationTo, cubeRotationTime);

			if (cubeRotationTime >= 1.0f) {
				rotating = false;
				cubeRotationTime = 0;
			}
		} else {
			if (Input.GetKeyDown (KeyCode.UpArrow)) {
				ResetCubeRotation (Vector3.right);
				rotating = true;
			} else if (Input.GetKeyDown (KeyCode.DownArrow)) {
				ResetCubeRotation (Vector3.left);
				rotating = true;
			} else if (Input.GetKeyDown (KeyCode.RightArrow)) {
				ResetCubeRotation (Vector3.forward);
				rotating = true;
			} else if (Input.GetKeyDown (KeyCode.LeftArrow)) {
				ResetCubeRotation (Vector3.back);
				rotating = true;
			}
		}
	}

	public class QuaternionComparer : IEqualityComparer<Quaternion>
	{
		public bool Equals(Quaternion lhs, Quaternion rhs) {
			return lhs == rhs;
		}

		public int GetHashCode(Quaternion obj) {
			return obj.GetHashCode();
		}
	}

	// cubeRotationFrom(회전 전), cubeRotationTo(회전 후) 설정
	void ResetCubeRotation (Vector3 axis) {
		spinning = false;
		cubeRotationFrom = cube.transform.rotation;

		Quaternion q = Quaternion.AngleAxis(90.0f, Quaternion.Inverse(cubeRotationFrom) * axis);
		cubeRotationTo = cubeRotationFrom * q;

		Assert.IsTrue(Quaternion.Inverse(cubeRotationFrom) * axis == cube.transform.InverseTransformVector(axis));
		Assert.AreEqual<Quaternion>(cubeRotationFrom * q, Quaternion.AngleAxis(90.0f, axis) * cubeRotationFrom, null, new QuaternionComparer());
	}
}

