using UnityEngine;
using System.Collections;
using UnityEditor;

// 5. 행렬에 의한 아핀 변환과 프로젝션 변환
// 행렬 내용 -> 실제 씬 내 오브젝트의 좌표 변환으로 적용 
[CustomEditor( typeof(Chapter05) )]
public class Chapter05Editor : Editor {

	Matrix4x4 matrix = Matrix4x4.identity;
	Matrix4x4 projectionMatrix = Matrix4x4.identity;

	float determinant3x3;
	float determinant4x4;
	
	Vector4 rhs;
	Vector4 result;

	Vector3 translation;
	Vector3 rotation;
	Vector3 scale = Vector3.one;

	// mf : 3D 모델의 정점 정보를 나타내는 MeshFilter
	// origVects, newVecrts : 실제 정점 정보를 저장하는 배열 
	
	private MeshFilter mf;
	private Vector3[] origVerts;
	private Vector3[] newVerts;

	public override void OnInspectorGUI() {
		// base.OnInspectorGUI ();

		// Hide Script property
		serializedObject.Update();
		DrawPropertiesExcluding(serializedObject, new string[]{"m_Script"});
		serializedObject.ApplyModifiedProperties();

		Chapter05 obj = target as Chapter05;

		EditorGUI.BeginChangeCheck();

		EditorGUILayout.BeginVertical( GUI.skin.box );

		EditorGUILayout.LabelField(new GUIContent("Model Transform"));

		matrix.SetRow(0, RowVector4Field(matrix.GetRow(0)));
		matrix.SetRow(1, RowVector4Field(matrix.GetRow(1)));
		matrix.SetRow(2, RowVector4Field(matrix.GetRow(2)));
		matrix.SetRow(3, RowVector4Field(matrix.GetRow(3)));

		if ( GUILayout.Button("Reset" ) ) {
			matrix = Matrix4x4.identity;
		}

		// Apply 버튼을 누르면,
		// matrix 변수에 들어있는 행렬을 좌표 변환으로서 origVerts에 있는 정점군에 차례로 적용해 새로운 정점군을 생성하고, 원래의 폴리곤 메시에 저장
		// mf는 Cube로부터 GetComponent를 이용해 가져온다 
		// MeshFilter : mesh 프로퍼티가 있고, 그 프로퍼티의 vertices 안에 가공하지 않은 정점 정보가 들어 있음
		// Matrix4x4.MultiplyPoint3x4() : 프로젝션 변환에는 사용x, 아핀 변환에는 충분한 곱셈
		if ( GUILayout.Button("Apply" ) ) {
			mf = obj.cube.GetComponent<MeshFilter>();
			origVerts = mf.mesh.vertices;
			newVerts = new Vector3[origVerts.Length];

			int i = 0;
			while (i < origVerts.Length) {
				newVerts[i] = matrix.MultiplyPoint3x4(origVerts[i]);
				i++;
			}

			mf.mesh.vertices = newVerts;
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical( GUI.skin.box );

		determinant3x3 = EditorGUILayout.FloatField("Determinant (3x3)", determinant3x3);
		determinant4x4 = EditorGUILayout.FloatField("Determinant (4x4)", determinant4x4);

		EditorGUILayout.EndVertical();

		EditorGUILayout.Space();

		EditorGUILayout.Space();

		EditorGUILayout.BeginVertical( GUI.skin.box );

		// 평행이동 행렬 : Matrix4x4.SetTRS()
		// SetTRS() : 이름 그대로 평행이동(T), 회전(R), 스케일(S)의 각 좌표 변환 행렬을 한 번에 Matrix4x4에 설정하는 메서드
		// -> 여기서는 평행이동만 원하므로 회전, 스케일에는 무변환에 해당하는 값을 지정 
		// Quaternion.identity : 무회전 상태
		// Vector3.one : 1배의 스케일 (= 1, 1, 1)
		// 작성한 행렬 m은 왼쪽부터 matrix에 곱하여 합성
		translation = EditorGUILayout.Vector3Field( "Translation", translation);

		if ( GUILayout.Button("Apply" ) ) {
			Matrix4x4 m = Matrix4x4.identity;
			m.SetTRS(translation, Quaternion.identity, Vector3.one);
			matrix = m * matrix;
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical( GUI.skin.box );

		rotation = EditorGUILayout.Vector3Field( "Rotation", rotation);

		if ( GUILayout.Button("Apply" ) ) {
			Matrix4x4 m = Matrix4x4.identity;
			m.SetTRS(Vector3.zero, Quaternion.Euler(rotation.x, rotation.y, rotation.z), Vector3.one);
			matrix = m * matrix;
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical( GUI.skin.box );
		
		scale = EditorGUILayout.Vector3Field( "Scale", scale);

		if ( GUILayout.Button("Apply" ) ) {
			Matrix4x4 m = Matrix4x4.identity;
			m.SetTRS(Vector3.zero, Quaternion.identity, scale);
			matrix = m * matrix;
		}

		EditorGUILayout.EndVertical();

		EditorGUILayout.BeginVertical( GUI.skin.box );
		
		EditorGUILayout.LabelField(new GUIContent("Projection Transform"));

		if ( GUILayout.Button("Perspective" ) ) {
			Camera.main.orthographic = false;
		}

		if ( GUILayout.Button("Orthographic" ) ) {
			Camera.main.orthographic = true;
		}

		projectionMatrix.SetRow(0, RowVector4Field(projectionMatrix.GetRow(0)));
		projectionMatrix.SetRow(1, RowVector4Field(projectionMatrix.GetRow(1)));
		projectionMatrix.SetRow(2, RowVector4Field(projectionMatrix.GetRow(2)));
		projectionMatrix.SetRow(3, RowVector4Field(projectionMatrix.GetRow(3)));

		// 회전, 스케일에도 마찬가지로 다른 두개의 아핀 변환에 아무것도 하지 않는 변환을 설정
		if ( GUILayout.Button("Camera.main.projectionMatrix" ) ) {
			bool dx = SystemInfo.graphicsDeviceType.ToString().IndexOf("Direct3D") > -1;
			Debug.Log(SystemInfo.graphicsDeviceType.ToString());

			Matrix4x4 pm = Camera.main.projectionMatrix;

			if (dx) {
				for (int i = 0; i < 4; i++) {
					pm[1, i] = -pm[1, i];
				}

				for (int i = 0; i < 4; i++) {
					pm[2, i] = pm[2, i] * 0.5f + pm[3, i] * 0.5f;
				}
			}

			projectionMatrix = pm;
		}

		// Camera.main.projectionMatrix() : 카메라의 프로젝션 변환 행렬을 가져옴
		if ( GUILayout.Button("GL.GetGPUProjectionMatrix" ) ) {
			projectionMatrix = GL.GetGPUProjectionMatrix(Camera.main.projectionMatrix, true);
		}

		if ( GUILayout.Button("Reset" ) ) {
			projectionMatrix = Matrix4x4.identity;
			Camera.main.ResetProjectionMatrix();
		}

		if ( GUILayout.Button("Set" ) ) {
			Camera.main.projectionMatrix = projectionMatrix;
		}

		EditorGUILayout.EndVertical();

		if (EditorGUI.EndChangeCheck()) {
			determinant3x3 = getDeterminant3x3(matrix);
			determinant4x4 = getDeterminant4x4(matrix);

			Undo.RecordObject(target, "Chapter05EditorUndo");
			EditorUtility.SetDirty(target);
		}
	}

	public float getDeterminant3x3(Matrix4x4 m) {
		return m.m00 * m.m11 * m.m22 - m.m00 * m.m12 * m.m21 - m.m01 * m.m10 * m.m22
			+ m.m01 * m.m12 * m.m20 + m.m02 * m.m10 * m.m21 - m.m02 * m.m11 * m.m20
		;
	}

	public float getDeterminant4x4(Matrix4x4 m) {
		return m.m03 * m.m12 * m.m21 * m.m30 - m.m02 * m.m13 * m.m21 * m.m30 - m.m03 * m.m11 * m.m22 * m.m30
			+ m.m01 * m.m13 * m.m22 * m.m30 + m.m02 * m.m11 * m.m23 * m.m30 - m.m01 * m.m12 * m.m23 * m.m30
			- m.m03 * m.m12 * m.m20 * m.m31 + m.m02 * m.m13 * m.m20 * m.m31 + m.m03 * m.m10 * m.m22 * m.m31
			- m.m00 * m.m13 * m.m22 * m.m31 - m.m02 * m.m10 * m.m23 * m.m31 + m.m00 * m.m12 * m.m23 * m.m31
			+ m.m03 * m.m11 * m.m20 * m.m32 - m.m01 * m.m13 * m.m20 * m.m32 - m.m03 * m.m10 * m.m21 * m.m32
			+ m.m00 * m.m13 * m.m21 * m.m32 + m.m01 * m.m10 * m.m23 * m.m32 - m.m00 * m.m11 * m.m23 * m.m32
			- m.m02 * m.m11 * m.m20 * m.m33 + m.m01 * m.m12 * m.m20 * m.m33 + m.m02 * m.m10 * m.m21 * m.m33
			- m.m00 * m.m12 * m.m21 * m.m33 - m.m01 * m.m10 * m.m22 * m.m33 + m.m00 * m.m11 * m.m22 * m.m33
		;
	}

	public static Vector4 RowVector4Field(Vector4 value, params GUILayoutOption[] options)
	{
		Rect position = EditorGUILayout.GetControlRect(true, 16f, EditorStyles.numberField, options);
		float[] values = new float[]{value.x, value.y, value.z, value.w};

		EditorGUI.BeginChangeCheck();

		EditorGUI.MultiFloatField(
			position,
			new GUIContent[]{new GUIContent(), new GUIContent(), new GUIContent(), new GUIContent()},
			values
		);

		if (EditorGUI.EndChangeCheck()) {
			value.Set(values[0], values[1], values[2], values[3]);
		}

		return value;
	}
}
