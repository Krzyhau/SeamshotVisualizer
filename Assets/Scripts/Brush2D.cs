using UnityEngine;
using System.Linq;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(MeshFilter))]


// Brush 2D class that uses polygon collider for creating a visual mesh
public class Brush2D : MonoBehaviour
{
	private PolygonCollider2D polygon;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;

	public bool solid = true;

	public Material material;
	public Color brushColor;
	public Color shadowColor;
	public Shooter shadowShooterTarget;
	public float shadowInterpolation;
	public bool allowDumbBindForShadowPreview;

	private Vector2[] shadowInterpOffsets = null;

	private void Start() {
		OnValidate();
    }
    private void OnValidate() {
		polygon = GetComponent<PolygonCollider2D>();
		meshFilter = GetComponent<MeshFilter>();
		meshRenderer = GetComponent<MeshRenderer>();

		if ((!meshRenderer.sharedMaterials[0] || !meshRenderer.sharedMaterials[1]) && material) {
			var materials = meshRenderer.sharedMaterials; 
			materials[0] = new Material(material);
			materials[1] = new Material(material);
			meshRenderer.sharedMaterials = materials;
		}

	}

	private void Update() {
		VerifyAndFixBrush();

        if (allowDumbBindForShadowPreview && Input.GetKeyDown(KeyCode.F2) && !IsBox()) {
			if(brushColor.a == 1.0f) {
				brushColor.a = 0.2f;
				shadowColor.a = 1.0f;
            } else {
				brushColor.a = 1.0f;
				shadowColor.a = 0.0f;
            }
        }

		if(meshRenderer.sharedMaterials[0] && meshRenderer.sharedMaterials[1]) {
			meshRenderer.sharedMaterials[0].SetColor("Brush_Color", brushColor);
			meshRenderer.sharedMaterials[1].SetColor("Brush_Color", shadowColor);
		}
	}

	private void VerifyAndFixBrush() {
		/*var verts = polygon.GetPath(0);
		if(prevVerts == null || verts.Length != prevVerts.Length || Enumerable.SequenceEqual(prevVerts, verts)) {
			prevVerts = new Vector2[verts.Length];
			verts.CopyTo(verts, 0);
			UpdateVisuals();
		}*/

		UpdateVisuals();
	}

	private void UpdateVisuals() {

		var path = polygon.GetPath(0);
		Vector2 middlePoint = GetLocalMiddle();

		//cleaning previous mesh
		if (meshFilter.sharedMesh) {
			DestroyImmediate(meshFilter.sharedMesh);
		}

		if (path.Length < 3) return;

		//generate new mesh
		Mesh mesh = new Mesh();
		mesh.subMeshCount = 2;

		var verts = new List<Vector3>();
		var uvs = new List<Vector2>();
		var tris1 = new List<int>();
		var tris2 = new List<int>();

		// normal mesh
		for (int i = 0; i < path.Length; i++) {
			var vert1 = path[i];
			var vert2 = path[(i + 1) % path.Length];
			verts.AddRange(new Vector3[] { vert1, vert2, middlePoint });

			var vertDir = (vert2 - vert1).normalized;
			var projPoint = vert1 + vertDir * Vector2.Dot(vertDir, middlePoint - vert1);
			var dist = (middlePoint - projPoint).magnitude;

			uvs.AddRange(new Vector2[] { Vector2.zero, Vector2.left, Vector2.up * dist });
			tris1.AddRange(new int[] { i * 3, i * 3 + 1, i * 3 + 2 });
		}

		if (shadowShooterTarget) {
			int j = verts.Count;
			float DIST_EPSILON = shadowShooterTarget.distanceEpsilon;
			Transform player = shadowShooterTarget.transform;
			Vector3 ppos = transform.InverseTransformPoint(player.position);

			if(shadowInterpOffsets==null || shadowInterpOffsets.Length != path.Length) {
				shadowInterpOffsets = new Vector2[path.Length];
            }

			List<Vector2> shiftedPath = new List<Vector2>();

			// visual shadow brush mesh
			if (IsBox()) {
				// simple brush -  visible part is "extended" towards the player

				Bounds brushBounds = new Bounds(middlePoint, Vector3.zero);

				int leftestCorner = 0, rightestCorner = 0;
				float leftestAng = 1000, rightestAng = -1000;
				float leftestDist = 9999, rightestDist = 9999;

				var forwardDir = ((Vector3)middlePoint - ppos).normalized;

				for (int i = 0; i < 4; i++) {
					Vector3 diffvec = (Vector3)path[i] - ppos;
					float ang = Vector3.SignedAngle(forwardDir, diffvec.normalized, Vector3.forward);
					float dist = diffvec.magnitude;
					if (ang < leftestAng || (ang == leftestAng && dist < leftestDist)) {
						leftestAng = ang;
						leftestCorner = i;
						leftestDist = dist;
					}
					if (ang > rightestAng || (ang == rightestAng && dist < rightestDist)) {
						rightestAng = ang;
						rightestCorner = i;
						rightestDist = dist;
					}

					brushBounds.Encapsulate(path[i]);
				}

				var leftRightVec = (path[rightestCorner] - path[leftestCorner]).normalized;

				// find intersections with expanded brushes for leftest and rightest corners
				Vector3 leftestDir = ((Vector3)path[leftestCorner] - ppos) * 2;
				Vector3 rightestDir = ((Vector3)path[rightestCorner] - ppos) * 2;
				float leftestCornerExpanded = IntersectRay(shadowShooterTarget, new Ray2D(player.position, leftestDir.normalized), leftestDir.magnitude, 2);
				float rightestCornerExpanded = IntersectRay(shadowShooterTarget, new Ray2D(player.position, rightestDir.normalized), rightestDir.magnitude, 2);
				var expandedWallRightestPoint = ppos + rightestDir * rightestCornerExpanded;
				brushBounds.Encapsulate(expandedWallRightestPoint);
				var expandedWallLeftestPoint = ppos + leftestDir * leftestCornerExpanded;
				brushBounds.Encapsulate(expandedWallLeftestPoint);

				brushBounds.Expand(0.0001f);

				// first, add rightmost "virtual wall"
				shiftedPath.Add(expandedWallRightestPoint);
				shiftedPath.Add(path[rightestCorner]);

				// then, handle vertices between rightest and leftest
				for (int i = (rightestCorner + 1) % 4; i != leftestCorner; i = (i + 1) % 4) {
					Vector3 vecPoint = path[i];
					if (Vector3.SignedAngle(leftRightVec, vecPoint - (Vector3)path[leftestCorner], Vector3.forward) > 0) {
						Vector2 outVec = ((Vector2)vecPoint - middlePoint).normalized;
						vecPoint += new Vector3((outVec.x < 0 ? -1 : 1), (outVec.y < 0 ? -1 : 1), 0) * DIST_EPSILON;
					}
					if (brushBounds.Contains(vecPoint)) shiftedPath.Add(vecPoint);
				}

				// now add leftmost "virtual wall"
				shiftedPath.Add(path[leftestCorner]);
				shiftedPath.Add(expandedWallLeftestPoint);

				// finally, do the same handling, but for vertices between leftest and rightest
				for (int i = (leftestCorner + 1) % 4; i != rightestCorner; i = (i + 1) % 4) {
					Vector3 vecPoint = path[i];
					if (Vector3.SignedAngle(leftRightVec, vecPoint - (Vector3)path[leftestCorner], Vector3.forward) > 0) {
						Vector2 outVec = ((Vector2)vecPoint - middlePoint).normalized;
						vecPoint += new Vector3((outVec.x < 0 ? -1 : 1), (outVec.y < 0 ? -1 : 1), 0) * DIST_EPSILON;
					}
					if (brushBounds.Contains(vecPoint)) shiftedPath.Add(vecPoint);
				}

			} else {
				// complex brush - "shift" points depending on how player is aiming.

				float frac = IntersectRay(shadowShooterTarget, new Ray2D(player.position, player.up));

				for (int i = 0; i < path.Length; i++) {
					var vert0 = path[(i == 0 ? path.Length : i) - 1];
					var vert1 = path[i];
					var vert2 = path[(i + 1) % path.Length];

					//shift points by all planes that are crossing that point
					(Vector2 plane1Dir, float plane1Dist) = GetPlane(vert0, vert1);
					(Vector2 plane2Dir, float plane2Dist) = GetPlane(vert1, vert2);

					if (Vector2.Dot(player.up, plane1Dir) > 0 && frac >= 0) plane1Dir *= -1;
					if (Vector2.Dot(player.up, plane2Dir) > 0 && frac >= 0) plane2Dir *= -1;

					var shiftVec1 = (vert2 - vert1).normalized;
					shiftVec1 *= DIST_EPSILON / Vector2.Dot(shiftVec1, plane1Dir);

					var shiftVec2 = (vert0 - vert1).normalized;
					shiftVec2 *= DIST_EPSILON / Vector2.Dot(shiftVec2, plane2Dir);

					var shiftVec = shiftVec1 + shiftVec2;
					shadowInterpOffsets[i] = Vector2.Lerp(shadowInterpOffsets[i], shiftVec, shadowInterpolation * Time.deltaTime);

					shiftedPath.Add(vert1 + shadowInterpOffsets[i]);
				}
			}

			for (int i = 0; i < shiftedPath.Count; i++) {
				var vert1 = shiftedPath[i];
				var vert2 = shiftedPath[(i + 1) % shiftedPath.Count];

				verts.AddRange(new Vector3[] { vert1, vert2, middlePoint });

				var vertDir = (vert2 - vert1).normalized;
				var projPoint = vert1 + vertDir * Vector2.Dot(vertDir, middlePoint - vert1);
				var dist = (middlePoint - projPoint).magnitude;

				uvs.AddRange(new Vector2[] { Vector2.zero, Vector2.left, Vector2.up * dist });
				tris2.AddRange(new int[] { j + i * 3, j + i * 3 + 1, j + i * 3 + 2 });
			}
		}

		mesh.vertices = verts.ToArray();
		mesh.SetTriangles(tris1.ToArray(), 0);
		mesh.SetTriangles(tris2.ToArray(), 1);
		mesh.uv = uvs.ToArray();

		mesh.RecalculateNormals();
		mesh.RecalculateBounds();
		meshFilter.sharedMesh = mesh;
	}



	public Vector2 GetLocalMiddle() {
		Vector2 middle = Vector2.zero;
		var path = polygon.GetPath(0);
		foreach (var vert in path) {
			middle += vert;
        }

		return middle / path.Length;
	}

	public bool IsBox() {
		var verts = polygon.GetPath(0);
		if (verts.Length != 4) return false;
		for (int i = 0; i < 4; i++) {
			var diff = verts[i] - verts[(i + 1) % 4];
			if (diff.x != 0 && diff.y != 0) return false;
		}
		return true;
	}

	private (Vector2, float) GetPlane(Vector2 p1, Vector2 p2) {
		var p1T = transform.TransformPoint(p1);
		var p2T = transform.TransformPoint(p2);
		var midT = transform.TransformPoint(GetLocalMiddle());

		var pDir = (p2T - p1T).normalized;
		var planePoint = p1T + pDir * Vector2.Dot(pDir, midT - p1T);
		var normal = (planePoint - midT).normalized;
		var dist = Vector2.Dot(planePoint, normal);
		return (normal, dist);
	}

	// roughly similar implementation to CM_ClipBoxToBrush from the actual game
	// returns a fraction of distance where intersection of ray with brush happens.
	// returned value is 1.0 if ray didn't hit anything and -1.0 if started within brush.
	public float IntersectRay(Shooter shooter, Ray2D ray, float distance = 56755.84f, int type = 0) {
		var vertices = polygon.GetPath(0);
		if (vertices.Length < 3) return 1.0f;

		float DIST_EPSILON = shooter.distanceEpsilon;
		const float NEVER_UPDATED = -9999.0f;

        if (!solid) {
			return 1.0f;
        }

		if (IsBox() && type == 0) {
			// tldr of the simple brush logic: if intersecting with real brush,
			// find intersection with brush enlarged on each side by DIST_EPSILON
			if (IntersectRay(shooter, ray, distance, 1) < 1.0f) {
				return IntersectRay(shooter, ray, distance, 2);
			} else return 1.0f;
		}

		Vector2 p1 = ray.origin;
		Vector2 p2 = ray.origin + ray.direction * distance;

		float enterfrac = NEVER_UPDATED;
		float leavefrac = 1.0f;

		bool startout = false;

		// for each face of a brush, check intersection fraction
		// also find largest entry fraction and smallest exit fraction
		for (int i = 0; i < vertices.Length; i++) {
			(Vector2 planeNormal, float dist) = GetPlane(vertices[i], vertices[(i + 1) % vertices.Length]);

			float d1 = Vector2.Dot(p1, planeNormal) - dist;
			float d2 = Vector2.Dot(p2, planeNormal) - dist;


			if (d1 > 0.0f) {
				// starting point in front of the face, so ray definitely started outside of the brush
				startout = true;
				// both points are in front of the face, brush cannot be hit.
				if (d2 > 0.0f) return 1.0f;
			} else {
				// both points are behind the face. won't help us here.
				if (d2 <= 0.0f) continue;
			}

			// if got to this point, face was crossed
			// CUSTOM THING: using different behaviours depending on the "type" value

			// type 0 - complex brush with a bug
			if (type == 0) {
				if (d1 > d2) {
					// entering the face
					float f = (d1 - DIST_EPSILON) / (d1 - d2);
					f = Mathf.Max(0.0f, f);
					if (f > enterfrac) {
						enterfrac = f;
					}
				} else {
					// exiting the face

					// BUG!!!!
					// I imagine the original intention was to make the brush
					// virutally larger by decreasing the fraction on entry and
					// increasing the fraction on exit. HOWEVER d1 here is negative,
					// meaning that adding distance elipson here actually makes
					// fraction smaller, meaning it's backed up INTO the brush
					// rather than out of it. This is a one character typo that
					// makes seamshots possible!
					float f = (d1 + DIST_EPSILON) / (d1 - d2);
					if (f < leavefrac) {
						leavefrac = f;
					}
				}
			}

			//type 1 and 2 - no bug, used for simple brush because i'm too lazy to implement different code for simple brushes lol
			else {
				float f = (d1 - (type == 1 ? 0 : DIST_EPSILON)) / (d1 - d2);
				if (d1 > d2) {
					f = Mathf.Max(0.0f, f);
					if (f > enterfrac) {
						enterfrac = f;
					}
				} else {
					if (f < leavefrac) {
						leavefrac = f;
					}
				}
			}
			
		}

		if (!startout) {
			// original point was inside brush
			return -1.0f;
		}

		if (enterfrac < leavefrac && enterfrac > NEVER_UPDATED) {
			// "WE HIT SOMETHING!!!!" fking enthusiasm of this developer
			if (enterfrac < 0) enterfrac = 0;
			return enterfrac;
		}

		return 1.0f;
	}
}
