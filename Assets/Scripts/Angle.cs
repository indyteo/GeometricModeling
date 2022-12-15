using System;
using MyBox;
using UnityEngine;

[Serializable]
public class Angle {
	[Tooltip("Angle in radians: (A * PI) / B")]
	[SerializeField] private int a;
	[Tooltip("Angle in radians: (A * PI) / B")]
	[SerializeField] [PositiveValueOnly] private int b;

	public float Value => this.a * Mathf.PI / this.b;

	public Angle(int a = 1, int b = 1) {
		this.a = a;
		this.b = b;
	}

	public static implicit operator float(Angle angle) => angle.Value;
}
