using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gamemanger : MonoBehaviour {

	public GameObject cubeGenerator;
	public GameObject Effect;
	public GameObject originalCube;

	public float squaresize = 1;
	public GameObject maincamera;

	public Vector3 xoffset = new Vector3 (2.3f, 0, 0);
	public Vector3 zoffset = new Vector3 (0, 0, 2.3f);
	public float superposition;//默认值设为0.05f
	public float EffectColorChange;//默认为0.06f

	public float length;
	public float width;//x dir -> length,z dir -> width


	int[] cubeCount;//cubecount的0代表总数，cubecount的1计数，每到10改变camera的背景色
	int[] comboCount;
	int directionIndex;//运动方向的判定，初始是从z轴向负方向运动即值为1时，否则则从x轴向负方向运动

	Camera camera;
	GameObject cube;
	cubemeshGenerator generator;
	CameraControl cameraControl;

	Color originalColor = new Color();

	Vector3 offset = Vector3.zero;
	Vector3 cameraMove = Vector3.zero;

	List<Vector2> PreNodes;

	void Start () {//初始化时生成方块并调用其GenerateCube函数

		PreNodes = new List<Vector2> ();

		cubeCount = new int[2];
		comboCount = new int[1];

		directionIndex = 1;

		camera = maincamera.GetComponentInChildren<Camera> ();
		cameraControl = maincamera.GetComponent<CameraControl> ();

		offset += xoffset;

		cube =  Instantiate (cubeGenerator, offset , Quaternion.identity) as GameObject;
		generator = cube.GetComponent<cubemeshGenerator> ();
		Renderer renderer = cube.GetComponent<Renderer> ();
		originalColor = Random.ColorHSV (0f, 0.9f, 0.2f, 0.7f, 0.6f, 0.9f);//初始的随机颜色，v的值不能过小否则会太暗
		renderer.material.color = originalColor;

		float H, S, V;
		Color.RGBToHSV (originalColor, out H, out S, out V);

		Renderer originalRenderer = originalCube.GetComponent<Renderer> ();
		originalRenderer.material.color = Color.HSVToRGB (H - 0.03f, S, V);

		generator.GenerateCube(length, width);
		generator.caculateMoveDir (directionIndex, true);

		PreNodeGenerate (new Vector3(0,0,0), length, width);

		cubeCount[1]++;
	}

	void Update () {//每次触发时，生成新的方块，调用PerStepAddColor来获取新方块的颜色

		if (length >= 0.02f && width >= 0.02f) {
			if (Input.GetMouseButtonDown (0)) {

				generator.stopMove ();

				caculateBreakPos ();

				offset = CaculateOffset ();

				cube = Instantiate (cubeGenerator, offset, Quaternion.identity) as GameObject;

				generator = cube.GetComponent<cubemeshGenerator> ();
				Renderer renderer = cube.GetComponent<Renderer> ();//获取组件
				originalColor = PerStepAddcubeColor (originalColor);
				renderer.material.color = originalColor;//计算每个新的方块的颜色
				generator.GenerateCube (length, width);
				generator.caculateMoveDir (directionIndex, true);

				cameraMove += new Vector3 (0, squaresize / 4f, 0);
				cameraControl.SetMovement (cameraMove);

				cubeCount [1]++;
				cubeCount [0]++;

				if (cubeCount [1] >= 15) {
					cameraControl.ChangeCameraBColor ();

					cubeCount [1] = 0;
				}
			}
		} else {
			Destroy (cube);
//			Debug.Break ();
			Debug.Log(cubeCount[0]);
		}
	}

	Vector3 CaculateOffset(){
		if (directionIndex == 1) {
			offset = cube.transform.position + zoffset + new Vector3 (0, squaresize / 4f, 0);;
			directionIndex--;
		} else {
			offset = cube.transform.position + xoffset + new Vector3 (0, squaresize / 4f, 0);;
			directionIndex++;
		}

		return offset;
	}

	Color PerStepAddcubeColor(Color originalcolor){
		float H, S, V;
		Color.RGBToHSV (originalColor, out H, out S, out V);
		H += 0.009f;
		return Color.HSVToRGB (H, S, V);
	}

	void PreNodeGenerate(Vector3 centre,float length, float width){
		Vector2 V2centre = new Vector2 (centre.x, centre.z);
		if (PreNodes.Count == 0) {
			PreNodes.Add (new Vector2 (-length / 2f, width / 2f) + V2centre);//tf
			PreNodes.Add (new Vector2 (length / 2f, width / 2f) + V2centre);//tr
			PreNodes.Add (new Vector2 (length / 2f, -width / 2f) + V2centre);//br
			PreNodes.Add (new Vector2 (-length / 2f, -width / 2f) + V2centre);//bl
		} else {
			PreNodes [0] = new Vector2 (-length / 2f, width / 2f) + V2centre;
			PreNodes [1] = new Vector2 (length / 2f, width / 2f) + V2centre;
			PreNodes [2] = new Vector2 (length / 2f, -width / 2f) + V2centre;
			PreNodes [3] = new Vector2 (-length / 2f, -width / 2f) + V2centre;
		}
	}

	void CreatEffect(Vector3 Pos,float elength,float ewidth){
		GameObject effects;
		effects = Instantiate (Effect, Pos, Quaternion.identity) as GameObject;
		effects.GetComponent<EffectGenerator> ().GenerateEffect (elength, ewidth);
	}

	GameObject recreateCube(Vector3 pos,float length,float width,int index,Color color,bool kinematic,int type){
		GameObject Recube;

		Recube = Instantiate(cubeGenerator, pos, Quaternion.identity) as GameObject;
		Recube.GetComponent<cubemeshGenerator> ().GenerateCube (length, width);
		Recube.GetComponent<cubemeshGenerator> ().caculateMoveDir (index, false);
		Recube.GetComponent<cubemeshGenerator> ().stopMove ();
		Recube.GetComponent<cubemeshGenerator> ().SetType (type);
		Recube.GetComponent<Renderer> ().material.color = color;

		Recube.GetComponent<Rigidbody> ().isKinematic = kinematic;

		return Recube;
	}

	void caculateBreakPos(){

		GameObject StayCube, DropCube;
		Vector3 StayCubePos, DropCubePos;
		float Staylength, Droplength;
		float StayWidth, DropWidth;
		Color PreColor;

		if (directionIndex == 1) {
			Vector3 PreCentre = cube.transform.position;
			PreColor = cube.GetComponent<Renderer> ().material.color;

			float finalPosTL = PreCentre.x - length / 2f;//停下之后tl的点的x
			float finalPosTR = PreCentre.x + length / 2f;//停下之后tr的点的x

			if (((finalPosTL - PreNodes [0].x) > superposition) && ((finalPosTL - PreNodes [0].x) < length)) {
				Staylength = PreNodes [1].x - finalPosTL;
				Droplength = length - Staylength;

				StayCubePos = new Vector3 (finalPosTL + Staylength / 2f, PreCentre.y, PreCentre.z);
				DropCubePos = new Vector3 (finalPosTR - Droplength / 2f, PreCentre.y, PreCentre.z);

				length = Staylength;

				Destroy (cube.gameObject);

				StayCube = recreateCube (StayCubePos, length, width, directionIndex, PreColor, true,1);

				cube = StayCube;
				PreNodeGenerate (StayCubePos, length, width);

				DropCube = recreateCube (DropCubePos, Droplength, width, directionIndex, PreColor, false,0);

				comboCount [0] = 0;


			} else if (((PreNodes [1].x - finalPosTR) > superposition) && ((PreNodes [1].x - finalPosTR) < length)) {
				Staylength = finalPosTR - PreNodes [0].x;
				Droplength = length - Staylength;

				StayCubePos = new Vector3 (finalPosTR - Staylength / 2f, PreCentre.y, PreCentre.z);
				DropCubePos = new Vector3 (finalPosTL + Droplength / 2f, PreCentre.y, PreCentre.z);

				length = Staylength;
				//Debug.Log (length);

				Destroy (cube.gameObject);

				StayCube = recreateCube (StayCubePos, length, width, directionIndex, PreColor, true,1);

				cube = StayCube;
				PreNodeGenerate (StayCubePos, length, width);

				DropCube = recreateCube (DropCubePos, Droplength, width, directionIndex, PreColor, false,0);

				comboCount [0] = 0;

			}
			else if (Mathf.Abs(finalPosTL - PreNodes [0].x) <= superposition) {
				comboCount [0]++;

				float moveAdjustment = finalPosTL - PreNodes [0].x;
				if ((finalPosTL - PreNodes [0].x)>0) {
					cube.transform.Translate (new Vector3 (-moveAdjustment, 0, 0));
				} else if((finalPosTL - PreNodes [0].x)<0){
					cube.transform.Translate (new Vector3 (-moveAdjustment, 0, 0));
				}

				float elength = length;
				float ewidth = width;

				for (int x = 0; x < (comboCount [0]>4?4:comboCount[0]); x++) {
					CreatEffect (cube.transform.position, elength, ewidth);

					elength += 0.14f;
					ewidth += 0.14f;
				}
			}
			else {
				cube.GetComponent<Rigidbody> ().isKinematic = false;
				length = 0.001f;
				width = 0.001f;
			}
		} else {
			Vector3 PreCentre = cube.transform.position;
			PreColor = cube.GetComponent<Renderer> ().material.color;

			float finalPosBL = PreCentre.z - width / 2f;
			float finalPosTL = PreCentre.z + width / 2f;

			if(((finalPosBL - PreNodes[3].y) > superposition) && ((finalPosBL - PreNodes[3].y) < width)){
				StayWidth = PreNodes [0].y - finalPosBL;
				DropWidth = width - StayWidth;

				width = StayWidth;

				Destroy (cube.gameObject);

				StayCubePos = new Vector3 (PreCentre.x, PreCentre.y, finalPosBL + width / 2f);
				DropCubePos = new Vector3 (PreCentre.x, PreCentre.y, finalPosTL - DropWidth / 2f);

				StayCube = recreateCube (StayCubePos, length, width, directionIndex, PreColor, true,1);

				cube = StayCube;
				PreNodeGenerate (StayCubePos, length, width);

				DropCube = recreateCube (DropCubePos, length, DropWidth, directionIndex, PreColor, false,0);
				comboCount [0] = 0;

			}
			else if(((PreNodes[0].y - finalPosTL) > superposition) && ((PreNodes[0].y - finalPosTL) < width)){
				StayWidth = finalPosTL - PreNodes[3].y;
				DropWidth = width - StayWidth;

				width = StayWidth;

				Destroy (cube.gameObject);

				StayCubePos = new Vector3 (PreCentre.x, PreCentre.y, finalPosTL - width / 2f);
				DropCubePos = new Vector3 (PreCentre.x, PreCentre.y, finalPosBL + DropWidth / 2f);

				StayCube = recreateCube (StayCubePos, length, width, directionIndex, PreColor, true,1);

				cube = StayCube;
				PreNodeGenerate (StayCubePos, length, width);

				DropCube = recreateCube (DropCubePos, length, DropWidth, directionIndex, PreColor, false,0);
				comboCount [0] = 0;

			}
			else if(Mathf.Abs(finalPosTL - PreNodes[0].y) <= superposition ){
				comboCount [0]++;

				float moveAjustment = finalPosTL - PreNodes [0].y;
				if ((finalPosTL - PreNodes [0].y)> 0) {
					
					cube.transform.Translate (new Vector3 (0, 0, -moveAjustment));
				} else if((finalPosTL - PreNodes [0].y)< 0){
					
					cube.transform.Translate (new Vector3 (0, 0, -moveAjustment));
				}

				float elength = length;
				float ewidth = width;

				for (int x = 0; x < (comboCount [0]>4?4:comboCount[0]); x++) {
					CreatEffect (cube.transform.position, elength, ewidth);

					elength += 0.14f;
					ewidth += 0.14f;
				}

			} else{
				cube.GetComponent<Rigidbody> ().isKinematic = false;
				length = 0.001f;
				width = 0.001f;
			}
		}
	}
}
