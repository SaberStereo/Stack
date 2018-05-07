using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class cubemeshGenerator : MonoBehaviour {

	List<Vector3> vertices;
	List<int> triangles;
	List<Vector2> uv;

	List<Square> squares = new List<Square> ();

	Vector3 moveDir;
	float movespeed = 2.5f;
	bool moveCcompleted = true;
	bool movedir = true;
	int dirindex;

	int CubeType = 1; //1-> stay 0 -> drop

	public void caculateMoveDir(int dirIndex,bool moveState){
		if (moveState) {
			dirindex = dirIndex;
			if (dirIndex == 1) {
				moveDir = -Vector3.right * movespeed;
			} else {
				moveDir = -Vector3.forward * movespeed;
			}
		}
	}

	public void stopMove(){
		moveCcompleted = false;
	}

	public void SetType(int type){
		CubeType = type;
	}

	void moveInRange(float dirValue){
		if (dirValue >= 2.3f) {
			movedir = true;
		}
		if (dirValue <= -2.3f) {
			movedir = false;
		}
	}

	void Update(){
		if (CubeType == 0) {
			Destroy (this.gameObject, 2f);
		}

		if (moveCcompleted) {
			if (dirindex == 1) {
				moveInRange (this.transform.position.x);
				if (movedir) {
					this.transform.position += moveDir * Time.deltaTime;
				} else {
					this.transform.position -= moveDir * Time.deltaTime;
				}
			} else {
				moveInRange (this.transform.position.z);
				if (movedir) {
					this.transform.position += moveDir * Time.deltaTime;
				} else {
					this.transform.position -= moveDir * Time.deltaTime;
				}
			}
		}
	}

	public void GenerateCube(float length,float width){//x dir -> length,z dir -> width
		if (length >= width) {
			movespeed = 6f / length >= 3.8f ? 3.8f : 6f / length;
		} else {
			movespeed = 6f / width >= 3.8f ? 3.8f : 6f / width;
		}


		vertices = new List<Vector3>();
		triangles = new List<int>();
		uv = new List<Vector2> ();

		createCube (length, width);

		for (int x = 0; x < squares.Count; x++) {
			//Debug.Log (squares [x].topLeft.position);
			caculateUV(squares[x]);
			TriangluateSquare (squares [x]);
		}

		Mesh mesh = new Mesh();
		GetComponent<MeshFilter>().mesh = mesh;
		GetComponent<MeshCollider> ().sharedMesh = mesh;

		mesh.vertices = vertices.ToArray();
		mesh.triangles = triangles.ToArray();
		mesh.uv = uv.ToArray();
		mesh.RecalculateNormals ();

		this.gameObject.AddComponent<Rigidbody> ();
		GetComponent<Rigidbody> ().isKinematic = true;//在计算mesh之后加入rigidbody，否则rigidbody读取不到mesh，没有碰撞体积

		//Debug.Log (vertices.Count);
		//Debug.Log (uv.Count); 2者count必须一样，检错
	}

	void createCube(float length, float width){
		Vector3 posleft = new Vector3 (-length/2f, 1/4f, width/2f);
		Vector3 posright = new Vector3 (length/2f, 1/4f, width/2f);
		Vector3 posbleft = new Vector3 (-length/2f, 1/4f, -width/2f);
		Vector3 posbright = new Vector3 (length/2f, 1/4f, -width/2f);

		controlNode topleft = new controlNode (posleft,1f);
		controlNode topright = new controlNode (posright,1f);
		controlNode bottomleft = new controlNode (posbleft,1f);
		controlNode bottomright = new controlNode (posbright,1f);//控制节点，对应着上顶，侧面和底面使用controlnode的bottom来构造Square，在断裂的时候将传入新的position来分块

		Square top = new Square (topleft, topright, bottomright, bottomleft);
		Square bottom = new Square (topright.bottom, topleft.bottom, bottomleft.bottom, bottomright.bottom);
		Square left = new Square (topleft, bottomleft, bottomleft.bottom, topleft.bottom);
		Square right = new Square (bottomright, topright, topright.bottom, bottomright.bottom);
		Square forward = new Square (topright, topleft, topleft.bottom, topright.bottom);
		Square back = new Square (bottomleft, bottomright, bottomright.bottom, bottomleft.bottom);
		//每个面初始化的4个顶点，按照tl,tr,bl,br的顺序构造

		squares.Add (top);
		squares.Add (left);
		squares.Add (right);
		squares.Add (forward);
		squares.Add (back);
		squares.Add (bottom);//将每个面加入队列，在Generate中遍历队列，逐个面计算顶点和三角形
	}

	public class Node{
		public Vector3 position;
		public int vertexindex = -1;

		public Node(Vector3 _pos){
			position = _pos;
		}
	}

	public class controlNode:Node{
		public Node bottom;

		public controlNode(Vector3 _pos,float size):base(_pos){
			bottom = new Node(position - Vector3.up * size/4f);
		}
	}

	public class Square{//面包含4个顶点
		public Node topLeft,topRight,bottomLeft,bottomRight;

		public Square(Node _topLeft, Node _topRight, Node _bottomRight, Node _bottomLeft)
		{
			topLeft = _topLeft;
			topRight = _topRight;
			bottomRight = _bottomRight;
			bottomLeft = _bottomLeft;
		}
	}

	void caculateUV(Square square){
		uv.Add (new Vector2 (square.topLeft.position.x, square.topLeft.position.z));
		uv.Add (new Vector2 (square.topRight.position.x, square.topRight.position.z));
		uv.Add (new Vector2 (square.bottomLeft.position.x, square.bottomLeft.position.z));
		uv.Add (new Vector2 (square.bottomRight.position.x, square.bottomRight.position.z));
	}

	struct Triangle{
		int vertexA;
		int vertexB;
		int vertexC;

		public Triangle(int a,int b,int c){
			vertexA = a;
			vertexB = b;
			vertexC = c;
		}
	}

	/*void Add2TriangleDictionary(int vertexIndexKey,Triangle triangle){
		if (TriangleDictionary.ContainsKey (vertexIndexKey)) {
			TriangleDictionary [vertexIndexKey].Add (triangle);
		} else {
			List<Triangle> tList = new List<Triangle> ();
			tList.Add (triangle);
			TriangleDictionary.Add (vertexIndexKey, tList); 
		}
	}*/

	void CreateTriangles(Node a,Node b,Node c){
		triangles.Add (a.vertexindex);
		triangles.Add (b.vertexindex);
		triangles.Add (c.vertexindex);

		//Triangle triangle = new Triangle (a.vertexindex, b.vertexindex, c.vertexindex);
		//Add2TriangleDictionary (a.vertexindex, triangle);
		//Add2TriangleDictionary (b.vertexindex, triangle);
		//Add2TriangleDictionary (c.vertexindex, triangle);
	}

	void MeshFromPoints(params Node[] points){
		for (int x = 0; x < points.Length; x++) {
			//if (points [x].vertexindex == -1) {去掉的原因是保证vertices和uv的count相等，实际只有8个顶点，但是包含重复的顶点一共24个（？如何只保留8个顶点？）
				points [x].vertexindex = vertices.Count;
				//Debug.Log (points [x].vertexindex);
				vertices.Add (points [x].position); 
			//}
		}

		if (points.Length >= 3)
			CreateTriangles (points [0], points [1], points [2]);
		if (points.Length >= 4)
			CreateTriangles (points [0], points [2], points [3]);//因为是矩形所以不存在一个面5和6个顶点，故只有2个triangles
		//if (points.Length >= 5)
		//	CreateTriangles (points [0], points [3], points [4]);
		//if (points.Length >= 6)
		//	CreateTriangles (points [0], points [4], points [5]);
	}

	void TriangluateSquare(Square square){
		MeshFromPoints (square.topLeft, square.topRight, square.bottomRight, square.bottomLeft);//计算每个面的三角形
	}
}
