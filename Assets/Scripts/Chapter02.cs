using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Based on the code at http://wiki.unity3d.com/index.php/SphericalCoordinates with minor refactoring changes.
// This code is under Creative Commons Attribution Share Alike http://creativecommons.org/licenses/by-sa/3.0/


// 원점상의 구면좌표계 위를, 촬영 대상을 계속 보면서 이동하는 카메라
// 키보드 좌우키, 좌우 드래그 -> 카메라 좌우로 이동
// 키보드 상하키, 상하 드래그 -> 카메라 상하로 이동
// 마우스 휠 앞으로 -> 물체에 가까이 / 휠 뒤로 -> 물체에서 멀어짐
public class Chapter02 : MonoBehaviour {

	public float rotateSpeed = 1f;
	public float scrollSpeed = 200f;

	// 정육면체 : 메인 카메라가 보는 대상
	public Transform pivot;
	
	// (중첩 클래스) SphericalCoordinates : 직교좌표와 구면좌표의 변환을 담당
	[System.Serializable] // 인스펙터 창에서 프로퍼티 값 설정 가능하도록
	public class SphericalCoordinates
	{
		// 구면좌표 : P( r , ϕ , θ )
		
		// azimuth(방위각) : 좌우키로 회전할 수 있는 방향의 이동량 <- 라디안
		// 카메라가 바라보는 대상이 놓여있다고 간주하는 x축과 z축으로 이루어진 평면으로부터 카메라가 얼마나 위쪽(y축 방향)으로 움직이는지 
		public float _radius, _azimuth, _elevation;
		
		public float radius
		{ 
			get { return _radius; }
			private set
			{
				// Mathf.Clamp(v, min, max) : 최솟값 ~ 최댓값 넘지 않도록
				_radius = Mathf.Clamp( value, _minRadius, _maxRadius );
			}
		}

		public float azimuth
		{ 
			get { return _azimuth; }
			private set
			{ 
				// Mathf.Repeat(0, max) : 0 ~ 최댓값(식 : 최댓값-최솟값) 넘지 않도록
				_azimuth = Mathf.Repeat( value, _maxAzimuth - _minAzimuth ); 
			}
		}

		public float elevation
		{ 
			get{ return _elevation; }
			private set
			{
				_elevation = Mathf.Clamp( value, _minElevation, _maxElevation ); 
			}
		}

		public float _minRadius = 3f;
		public float _maxRadius = 20f;

		public float minAzimuth = 0f;
		private float _minAzimuth;

		public float maxAzimuth = 360f;
		private float _maxAzimuth;

		public float minElevation = 0f;
		private float _minElevation;

		public float maxElevation = 90f;
		private float _maxElevation;
		
		public SphericalCoordinates(){}
		
		// 직교좌표의 수치를 인수로 받아, 구면좌표의 초깃값으로 설정 (직교좌표 -> 구면좌표 변환)
		public SphericalCoordinates(Vector3 cartesianCoordinate)
		{
			// azimuth, elevation : 도수 -> 라디안으로 변환
			_minAzimuth = Mathf.Deg2Rad * minAzimuth;
			_maxAzimuth = Mathf.Deg2Rad * maxAzimuth;

			_minElevation = Mathf.Deg2Rad * minElevation;
			_maxElevation = Mathf.Deg2Rad * maxElevation;

			// Vector3.magnitude : 직교좌표의 원점으로부터의 거리(카메라 자신과 원점의 거리)
			radius = cartesianCoordinate.magnitude;
			// azimuth : 아크탄젠트로 구할 수 있음 (직교좌표축의 x좌표 = 밑변, z좌표 = 높이 -> 두 변이 이루는 직각삼각형의 내각)
			azimuth = Mathf.Atan2(cartesianCoordinate.z, cartesianCoordinate.x);
			// elevation : 아크사인(y좌표 = 높이, 반지름 r = 빗변 -> 직각삼각형의 내각)
			elevation = Mathf.Asin(cartesianCoordinate.y / radius);
		}
		
		public Vector3 toCartesian
		{
			get
			{
				float t = radius * Mathf.Cos(elevation);
				return new Vector3(t * Mathf.Cos(azimuth), radius * Mathf.Sin(elevation), t * Mathf.Sin(azimuth));
			}
		}
		
		public SphericalCoordinates Rotate(float newAzimuth, float newElevation){
			azimuth += newAzimuth;
			elevation += newElevation;
			return this;
		}
		
		public SphericalCoordinates TranslateRadius(float x) {
			radius += x;
			return this;
		}
	}
	
	public SphericalCoordinates sphericalCoordinates;
	
	// Use this for initialization
	void Start () {
		sphericalCoordinates = new SphericalCoordinates(transform.position);
		transform.position = sphericalCoordinates.toCartesian + pivot.position;
	}

	// Update is called once per frame
	void Update () {
		float kh, kv, mh, mv, h, v;
		kh = Input.GetAxis( "Horizontal" );
		kv = Input.GetAxis( "Vertical" );
		
		bool anyMouseButton = Input.GetMouseButton(0) | Input.GetMouseButton(1) | Input.GetMouseButton(2);
		mh = anyMouseButton ? Input.GetAxis( "Mouse X" ) : 0f;
		mv = anyMouseButton ? Input.GetAxis( "Mouse Y" ) : 0f;
		
		h = kh * kh > mh * mh ? kh : mh;
		v = kv * kv > mv * mv ? kv : mv;
		
		if (h * h > Mathf.Epsilon || v * v > Mathf.Epsilon) {
			transform.position
				= sphericalCoordinates.Rotate(h * rotateSpeed * Time.deltaTime, v * rotateSpeed * Time.deltaTime).toCartesian + pivot.position;
		}

		float sw = -Input.GetAxis("Mouse ScrollWheel");
		if (sw * sw > Mathf.Epsilon) {
			transform.position = sphericalCoordinates.TranslateRadius(sw * Time.deltaTime * scrollSpeed).toCartesian + pivot.position;
		}

		transform.LookAt(pivot.position);
	}
}


