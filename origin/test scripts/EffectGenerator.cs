using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EffectGenerator : MonoBehaviour {

	List<Vector3> vertices;
	List<int> Triangles;
	Color ColorChange;
	Color OriginalColor;

	float AlphaValue = 1f;

	float effectwidth = 0.05f;

	void Start(){
		OriginalColor = this.GetComponent<Renderer> ().material.color;
		//ColorChange = Color.HSVToRGB (0,0,0.2f);
		Destroy (this.gameObject, 2f);
	}

	void Update(){
//		GetComponent<Renderer> ().material.color = Color.Lerp (GetComponent<Renderer> ().material.color, new Color(255,255,255,0), Time.deltaTime);
		AlphaValue -= 0.03f;

		this.GetComponent<Renderer>().material.color = new Color(OriginalColor.r,OriginalColor.g,OriginalColor.b, AlphaValue);
	}

	public class Node{
		public Vector3 position;
		public int vertexindex = -1;

		public Node(Vector3 _pos){
			position = _pos;
		}
	}

	public void GenerateEffect(float length,float width){
		vertices = new List<Vector3> ();
		Triangles = new List<int> ();
		CreateEffect (length, width);

		Mesh mesh = new Mesh ();

		GetComponent<MeshFilter> ().mesh = mesh;

		mesh.vertices = vertices.ToArray ();
		mesh.triangles = Triangles.ToArray ();
		mesh.RecalculateNormals ();
	}

	void CreateEffect(float length,float width){
		Node Outtl, Outtr, Outbr, Outbl, Intl, Intr, Inbr, Inbl;
		Intl = new Node (new Vector3 (-length / 2f, 0, width / 2f));
		Intr = new Node (new Vector3 (length / 2f, 0, width / 2f));
		Inbr = new Node (new Vector3 (length / 2f, 0, -width / 2f));
		Inbl = new Node (new Vector3 (-length / 2f, 0, -width / 2f));

		Outtl = new Node (new Vector3 (-length / 2f - effectwidth, 0, width / 2f +effectwidth));
		Outtr = new Node (new Vector3 (length / 2f + effectwidth, 0, width / 2f +effectwidth));
		Outbr = new Node (new Vector3 (length / 2f + effectwidth, 0, -width / 2f -effectwidth));
		Outbl = new Node (new Vector3 (-length / 2f - effectwidth, 0, -width / 2f -effectwidth));

		Add2Vertices (Outtl, Outtr, Outbr, Outbl, Intl, Intr, Inbr, Inbl);
	}

	void Add2Vertices(params Node[] points){
		for (int x = 0; x < points.Length; x++) {
			points [x].vertexindex = vertices.Count;

			vertices.Add (points [x].position);
		}
		Add2Triangles (points [0], points [1], points [4]);
		Add2Triangles (points [4], points [1], points [5]);
		Add2Triangles (points [5], points [1], points [2]);
		Add2Triangles (points [5], points [2], points [6]);
		Add2Triangles (points [7], points [6], points [2]);
		Add2Triangles (points [7], points [2], points [3]);
		Add2Triangles (points [4], points [7], points [3]);
		Add2Triangles (points [0], points [4], points [3]);
	}

	void Add2Triangles(Node a,Node b,Node c){
		Triangles.Add (a.vertexindex);
		Triangles.Add (b.vertexindex);
		Triangles.Add (c.vertexindex);
	}
}
