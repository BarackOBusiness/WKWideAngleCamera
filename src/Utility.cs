using UnityEngine;

namespace WideAngleCamera;

public static class Utility {
	public static float ExpDecay(float a, float b, float decay, float dt) {
		return b+(a-b)*Mathf.Exp(-decay*dt);
	}

	// This is dead code for now, I'll save the implementation details for when
	// I have a good solution identified
	// This function should transform world points to viewport points
	// it will hand off actual projecting to a series of projection functions
	// for the user's selected projection.
	public static Vector3 WorldToViewportPoint(Transform seer, Vector3 worldPoint) {
		return Vector3.zero;
	}

	// A projection function like this should be responsible for projection
	// and fov scaling, only.
	public static (float, float) StereographicProject(float x, float y, float z) {
		float s = 1f / Mathf.Tan(CameraManager.Instance.FOV * Mathf.Deg2Rad * 0.25f);
		float u = s * (x / (1f - z));
		float v = s * (y / (1f - z));
		return (u, v);
	}
}

