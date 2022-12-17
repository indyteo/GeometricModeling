using System;
using MyBox;
using UnityEngine;

/// <summary>
/// Data class to represent an angle (in radians) in the form A * PI / B.
/// Only used as serialize field in the Unity editor.
/// </summary>
[Serializable]
public class Angle {
	[Tooltip("Angle in radians: A * PI / B")]
	[SerializeField] private int a;
	[Tooltip("Angle in radians: A * PI / B")]
	[SerializeField] [PositiveValueOnly] private int b;

	/// <summary>
	/// Compute the angle value in radians.
	/// </summary>
	public float Value => this.a * Mathf.PI / this.b;

	public Angle(int a = 1, int b = 1) {
		this.a = a;
		this.b = b;
	}

	public static implicit operator float(Angle angle) => angle.Value;
}
