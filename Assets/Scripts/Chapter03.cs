using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.EventSystems;

// Based on the code at http://wiki.unity3d.com/index.php/SphericalCoordinates
// This code is under Creative Commons Attribution Share Alike http://creativecommons.org/licenses/by-sa/3.0/

// 간이 충돌 판정 (<- 벡터의 외적, 내적 성질을 사용)
// 점 P, 삼각형 ABC 내부에 있는 경우 : 외적(각각의 변과 각 정점에서 점 P로 향한 직선과의 법선벡터) -> 모든 법선벡터는 같은 방향을 향한다
// 점 P, 삼각형 ABC 외부에 있는 경우 : 외적으로써 얻어지는 법선벡터의 방향이 반전
// "벡터의 내적, 외적 -> 점, 직선, 평면의 위치 관계를 많은 계산비용을 들이지 않고도 확인할 수 있다"
public class Chapter03 : MonoBehaviour {

	public float rotateSpeed = 1f;
	public float scrollSpeed = 200f;

	public Transform pivot;

	[System.Serializable]
	public class SphericalCoordinates
	{
		public float _radius, _azimuth, _elevation;

		public float radius
		{ 
			get { return _radius; }
			private set
			{
				_radius = Mathf.Clamp( value, _minRadius, _maxRadius );
			}
		}

		public float azimuth
		{ 
			get { return _azimuth; }
			private set
			{ 
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
		
		public SphericalCoordinates(Vector3 cartesianCoordinate)
		{
			_minAzimuth = Mathf.Deg2Rad * minAzimuth;
			_maxAzimuth = Mathf.Deg2Rad * maxAzimuth;

			_minElevation = Mathf.Deg2Rad * minElevation;
			_maxElevation = Mathf.Deg2Rad * maxElevation;

			radius = cartesianCoordinate.magnitude;
			azimuth = Mathf.Atan2(cartesianCoordinate.z, cartesianCoordinate.x);
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

	// Chapter02와 다른 내용
	// triangleVertices : Vector3의 리스트 (큐브 안의 정점 좌표를 세 개 넣어 충돌 판정의 대상으로 삼는 삼각형으로 다루기)
	private List<Vector3> triangleVertices = new List<Vector3>();

	// Use this for initialization
	void Start () {
		sphericalCoordinates = new SphericalCoordinates(transform.position);
		transform.position = sphericalCoordinates.toCartesian + pivot.position;

		// 큐브에 있는 MeshFilter 컴포넌트를 GetComponent 메서드로 가져와서, mesh 프로퍼티에 있는 Mesh 클래스의 오브젝트의 메시 정보에 직접 액세스
		// Mesh 클래스 : vertices 프로퍼티로서 폴리곤 메시의 정점 좌표 배열을 가짐
		Mesh mesh = pivot.gameObject.GetComponent<MeshFilter>().mesh;
		for (int i = 0; i < mesh.vertices.Length; i++)
		{
			// 처음 3개를 뽑아 triangleVertices에 넣는다
			if (triangleVertices.Count < 3) {
				triangleVertices.Add(mesh.vertices[i]);
			}
		}
	}

	// Update is called once per frame
	void Update () {
		DrawCameraLine ();

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

	// 선, 충돌 판정 역할
	void DrawCameraLine() {
		// 큐브 z축 방향으로 파란 선 긋기
		// transform.forward : 월드 좌표상에서 z축 방향 벡터
		Debug.DrawLine(pivot.position, pivot.transform.forward * 2, Color.blue);
		
		// 카메라에서 나온 선 끝 좌표
		Vector3 cameraPoint = transform.position + transform.forward * 5;
		
		// triangleVertices에 들어간 세 개의 정점 좌표
		// -> 삼각형의 각 변에 해당하는 벡터 구하기
		Vector3 edge1 = triangleVertices [1] - triangleVertices [0];
		Vector3 edge2 = cameraPoint - triangleVertices [1];
		
		Vector3 edge3 = triangleVertices [2] - triangleVertices [1];
		Vector3 edge4 = cameraPoint - triangleVertices [2];
		
		Vector3 edge5 = triangleVertices [0] - triangleVertices [2];
		Vector3 edge6 = cameraPoint - triangleVertices [0];
		
		// 양쪽 벡터의 외적 : Vector3.Cross() -> 3정점만큼 구하기
		// cf. Vector3.Cross() : 두 벡터의 외적
		Vector3 cp1 = Vector3.Cross (edge1, edge2);
		Vector3 cp2 = Vector3.Cross (edge3, edge4);
		Vector3 cp3 = Vector3.Cross (edge5, edge6);

		// cp1과 cp2, cp3와의 내적 구해서 부호 확인
		// 내적 = 양수 : 법선벡터가 반대 방향이 아니라는 뜻
		// 다른 법선벡터 양쪽과 반대방향 x = 모든 정점의 법선벡터가 같은 방향을 향함
		// => 카메라에서 나온 선의 끝이 triangleVertices에 설정한 삼각형 위에 있음
		if (Vector3.Dot (cp1, cp2) > 0 && Vector3.Dot (cp1, cp3) > 0) {
			Debug.DrawLine (transform.position, cameraPoint, Color.red);
		} else {
			Debug.DrawLine (transform.position, cameraPoint, Color.green);
		}
	}
}

