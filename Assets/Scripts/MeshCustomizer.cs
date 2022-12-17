using HalfEdge;
using MyBox;
using UnityEditor;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class MeshCustomizer : MonoBehaviour {
	[Header("Object")]
	[SerializeField] private ObjectType type = ObjectType.None;
	[SerializeField] [ConditionalField("type", false, ObjectType.Box, ObjectType.Chips, ObjectType.RegularPolygon, ObjectType.PacMan)] private Vector3 halfSize = new Vector3(3, 3, 3);
	[SerializeField] [ConditionalField("type", false, ObjectType.RegularPolygon, ObjectType.PacMan)] private int nSectors = 6;
	[SerializeField] [ConditionalField("type", false, ObjectType.PacMan)] private Angle startAngle = new Angle(1, 3);
	[SerializeField] [ConditionalField("type", false, ObjectType.PacMan)] private Angle endAngle = new Angle(5, 3);
	[SerializeField] [ConditionalField("type", false, ObjectType.Tree)] private float baseSize = 3;
	[SerializeField] [ConditionalField("type", false, ObjectType.Tree)] private float stemRadius = 1;
	[SerializeField] [ConditionalField("type", false, ObjectType.Tree)] private float stemHeight = 10;
	[SerializeField] [ConditionalField("type", false, ObjectType.Tree)] private float leavesSize = 5;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [Range(3, 100)] private int helicalPrecision = 4;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [Range(3, 100)] private int tubePrecision = 4;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [PositiveValueOnly] private float helicalRadius = 5;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [PositiveValueOnly] private float tubeRadius = 1;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [Range(1, 25)] private int n = 5;
	[SerializeField] [ConditionalField("type", false, ObjectType.HelicalTube)] [Range(1, 5)] private float extension = 1;

	[Header("Catmull-Clark")]
	[SerializeField] private bool enableCatmullClark;
	[SerializeField] [ConditionalField("enableCatmullClark")] [Range(1, 5)] private int subdivisionCount = 3;

	[Header("Gizmos")]
	[SerializeField] private bool drawGizmos;
	[SerializeField] [ConditionalField("drawGizmos")] private bool drawEdgesLines;
	[SerializeField] [ConditionalField("drawGizmos")] private bool drawEdgesLabels;
	[SerializeField] [ConditionalField("drawGizmos")] private bool drawVertices;
	[SerializeField] [ConditionalField("drawGizmos")] private bool drawFaces;

	private new Transform transform;
	private MeshFilter mf;
	private HalfEdgeMesh mesh;
	private bool meshModified;

	void Awake() {
		this.transform = this.GetComponent<Transform>();
		this.mf = this.GetComponent<MeshFilter>();

		this.GenerateMesh();
	}

	private void Update() {
		if (this.meshModified && this.mf) {
			// Update the mesh if modified
			this.mf.mesh = this.mesh == null ? null : this.mesh.ConvertToFaceVertexMesh();
			this.meshModified = false;
		}
	}

	private void OnValidate() {
		// Regenerate mesh each time a serialized field was modified in the Unity editor
		this.GenerateMesh();
	}

	private void GenerateMesh() {
		Mesh m = this.GetObject();
		// Can be null if object type "none" is selected
		if (!m) {
			this.mesh = null;
			this.meshModified = true;
			return;
		}

        this.mesh = new HalfEdgeMesh(m);
		// Compute CatmullClark subdivisions if enabled
        if (this.enableCatmullClark)
        	for (int i = 0; i < this.subdivisionCount; i++)
        		this.mesh.SubdivideCatmullClark();
        // Mark the mesh as modified to update the real Unity mesh in the next update
        this.meshModified = true;
	}

	/// <summary>
	/// Create the base mesh using the customized properties.
	/// </summary>
	/// <returns>The customized mesh, or null if none is selected</returns>
	private Mesh GetObject() {
		switch (this.type) {
			case ObjectType.Box:
				return MeshUtils.CreateBox(this.halfSize);
			case ObjectType.Chips:
				return MeshUtils.CreateChips(this.halfSize);
			case ObjectType.RegularPolygon:
				return MeshUtils.CreateRegularPolygon(this.halfSize, this.nSectors);
			case ObjectType.PacMan:
				return MeshUtils.CreatePacMan(this.halfSize, this.nSectors, this.startAngle, this.endAngle);
			case ObjectType.Tree:
				return MeshUtils.CreateTree(this.baseSize, this.stemRadius, this.stemHeight, this.leavesSize);
			case ObjectType.HelicalTube:
				return MeshUtils.CreateHelicalTube(this.helicalPrecision, this.tubePrecision, this.helicalRadius, this.tubeRadius, this.n, this.extension);
			default:
				return null;
		}
	}

	private void OnDrawGizmos() {
		// Show object gizmos if possible
		if (this.drawGizmos && this.mesh != null && this.transform != null)
			this.mesh.DrawGizmos(this.transform.TransformPoint, this.drawVertices, this.drawEdgesLines, this.drawEdgesLabels, this.drawFaces);
	}

	private void OnGUI() {
		// Display "Copy CSV" button if the object is selected
		if (this.mesh != null && Selection.activeGameObject == this.gameObject) {
			GUILayout.BeginArea(new Rect(10, 10, Screen.width - 20, 25));
			if (GUILayout.Button($"Copy CSV ({this.name})", GUILayout.ExpandWidth(false)))
				// Copy CSV to clipboard using property systemCopyBuffer
				GUIUtility.systemCopyBuffer = this.mesh.ConvertToCSVFormat();
			GUILayout.EndArea();
		}
	}
}
