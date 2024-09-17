using UnityEngine;
using System.Collections.Generic;
using UnityEditor;

public class PolyDrawer : MonoBehaviour {
	
	public static List<Vector2> RawPoints = new List<Vector2>();
	
	private struct PolyPoint{

		public int NextP;
		public int PrevP;

		public int NextEar;
		public int PrevEar;

		public int NextRefl;
		public int PrevRefl;

		public bool isEar;

	}

	private static List<Vector3> m_TriPointList;
	private static int Pointcount;
	private static PolyPoint[] PolyPointList;

	public Transform parent;
	
	private static Mesh 						m_Mesh;

    public static Mesh GetMesh(Mesh mesh, Vector3[] points) {

		m_Mesh = mesh;
		mesh.Clear();

        var flatPoints = new Vector2[points.Length];
        RawPoints.Clear();
        for (int i = 0; i < flatPoints.Length; i++) {
            flatPoints[i] = new Vector2(points[i].x, points[i].z);
			RawPoints.Add(flatPoints[i]);
        }

        Pointcount = RawPoints.Count;
        PolyPointList = new PolyPoint[Pointcount + 1];
        m_TriPointList = new List<Vector3>();

        FillLists();

        Triangulate();

        DrawMesh(points);

		return m_Mesh;
    }

    static void FillLists(){

		/*
		 * three doubly linked lists (points list,reflective points list, ears list) are
		 * maintained in the "PolyPointList" arry.
		 * points list is a cyclic list while other two arent.
		 * 0 index of the Point list is kept only for entering the lists
		 * -1 means undefined link
		 */
		PolyPoint p = new PolyPoint();

		PolyPointList[0] = p;
		PolyPointList[0].NextP = 1;
		PolyPointList[0].PrevP = -1;
		PolyPointList[0].NextEar = -1;
		PolyPointList[0].PrevEar = -1;
		PolyPointList[0].NextRefl = -1;
		PolyPointList[0].PrevRefl = -1;
		PolyPointList[0].isEar = false;

		int T_Reflective = -1;
		int T_Convex = -1;

		for(int i=1;i<=Pointcount;i++){
			
			PolyPointList[i]=p;

			if(i==1)
				PolyPointList[i].PrevP = Pointcount;
			else
				PolyPointList[i].PrevP = i-1;

			PolyPointList[i].NextP = (i%Pointcount)+1;

			if(isReflective(i)){

				PolyPointList[i].PrevRefl = T_Reflective;

				if(T_Reflective==-1){
					PolyPointList[0].NextRefl =i;
				}
				else
					PolyPointList[T_Reflective].NextRefl=i;

				T_Reflective = i;
				PolyPointList[i].NextRefl = -1;

				PolyPointList[i].PrevEar = -1;
				PolyPointList[i].NextEar = -1;

			}
			else{

				PolyPointList[i].PrevRefl = -1;
				PolyPointList[i].NextRefl = -1;
				PolyPointList[i].isEar = true;

				PolyPointList[i].PrevEar = T_Convex;

				if(T_Convex==-1){
					PolyPointList[0].NextEar = i;
				}
				else
					PolyPointList[T_Convex].NextEar=i;

				T_Convex = i;

				PolyPointList[i].NextEar = -1;
			}

		}


		int Con = PolyPointList[0].NextEar;

		while(Con!=-1){

			if(!isCleanEar(Con)){
				RemoveEar(Con);
			}
				Con = PolyPointList[Con].NextEar;

		}


	}


	/*
	 * "Ear Clipping" is used for
	 * Polygon triangulation
	 */
	private static void Triangulate(){

		int i;
		
		while(Pointcount>3){

			/*
			 * The Two-Ears Theorem: "Except for triangles every 
			 * simple ploygon has at least two non-overlapping ears"
			 * so there i will always have a value
			 */
			i= PolyPointList[0].NextEar;
			
			int PrevP = PolyPointList[i].PrevP;
			int NextP = PolyPointList[i].NextP;
			
			m_TriPointList.Add(new Vector3(PrevP,i,NextP));
			
			RemoveEar(i);
			RemoveP(i);

			if(!isReflective(PrevP)){
				
				if(isCleanEar(PrevP)){ 
					
					if(!PolyPointList[PrevP].isEar){
						
						AddEar(PrevP);
					}
					
				}
				else{
					
					if(PolyPointList[PrevP].isEar){
						
						RemoveEar(PrevP);
					}  
					
				}
				
			}

			if(!isReflective(NextP)){
				
				if(isCleanEar(NextP)){ 
					
					if(!PolyPointList[NextP].isEar){
						
						AddEar(NextP);
					}
					
				}
				else{
					
					if(PolyPointList[NextP].isEar){
						
						RemoveEar(NextP);
					}  
					
				}
				
			}
			
			
		}

		int y = PolyPointList[0].NextP;
		int x = PolyPointList[y].PrevP;
		int z = PolyPointList[y].NextP;
		
		m_TriPointList.Add(new Vector3(x , y , z));

	}

	

	private static void DrawMesh(Vector3[] points){
		
		int vertex_count = RawPoints.Count;
		int triangle_count = m_TriPointList.Count;

		/*
		 * Mesh vertices
		 */
		Vector3 [] vertices = new Vector3 [vertex_count * 2]; 

		for(int i = 0; i < vertex_count; i++) {
            vertices[i] = new Vector3(RawPoints[i].x, points[i].y, RawPoints[i].y);
            vertices[vertex_count + i] = new Vector3(RawPoints[i].x, points[i].y - GlobalRoomData.Get.bridgeHeight, RawPoints[i].y);
        }

        RawPoints.Clear();

		m_Mesh.vertices = vertices;

		/*
		 * Mesh trangles
		 */
		int [] tri = new int [(triangle_count * 6)+(vertex_count*6)];
		
		// faces
		for(int i=0,j=0;i<triangle_count;i++,j+=6){
			int a = (int)(m_TriPointList[i].x - 1);
			int b = (int)(m_TriPointList[i].y-1);
            int c = (int)(m_TriPointList[i].z - 1);

			// upper face
            tri[j] = a;
            tri[j + 1] = b;
            tri[j + 2] = c;

			// bottom face
            tri[j+3]= vertex_count+ c;
			tri[j+4]= vertex_count + b;
			tri[j+5]= vertex_count + a;
		}
		// sides
		for (int i = 0, j = (triangle_count * 6); i < vertex_count; i++, j+=6) {
			var n = (i + 1) % vertex_count;

            tri[j] = i;
			tri[j+1] = vertex_count+i;
			tri[j+2] = n;

            tri[j + 3] = n;
            tri[j + 4] = vertex_count+i;
            tri[j + 5] = vertex_count+n;

        }

        m_Mesh.triangles = tri;
		// normals & uvs 
		/*Vector3[] normals= new Vector3[vertex_count*2];
		for(int i=0;i<vertex_count*2;i++)
			normals[i] = -Vector3.forward;
		m_Mesh.normals = normals;
		m_Uv    = new Vector2[vertex_count * 2];
		for(int i=0;i<m_Uv.Length;i++)
			m_Uv[i] = new Vector2(0, 0);
		m_Mesh.uv = m_Uv;*/

	}

	/*
	 * Utility Methods
	 */

	private static bool isCleanEar(int Ear){

		/*
		 * Barycentric Technique is used to test
		 * if the reflective vertices are in selected ears
		 */

		float dot00;
		float dot01;
		float dot02;
		float dot11;
		float dot12;

		float invDenom;
		float U;
		float V;

		Vector2 v0 = RawPoints[PolyPointList[Ear].PrevP-1]-RawPoints[Ear-1];
		Vector2 v1 = RawPoints[PolyPointList[Ear].NextP-1]-RawPoints[Ear-1];
		Vector2 v2;

		int i = PolyPointList[0].NextRefl;

		while(i!=-1){

			v2 = RawPoints[i-1]-RawPoints[Ear-1];

			dot00=Vector2.Dot(v0,v0);
			dot01=Vector2.Dot(v0,v1);
			dot02=Vector2.Dot(v0,v2);
			dot11=Vector2.Dot(v1,v1);
			dot12=Vector2.Dot(v1,v2);

			invDenom = 1 / (dot00 * dot11 - dot01 * dot01);
			U = (dot11 * dot02 - dot01 * dot12) * invDenom;
			V = (dot00 * dot12 - dot01 * dot02) * invDenom;

			if((U > 0) && (V > 0) && (U + V < 1))
			return false;

			i = PolyPointList[i].NextRefl;
		}

		return true;
	}

	private static bool isReflective(int P){

		/*
		 * vector cross product is used to determin the reflectiveness of vertices
		 * because "Sin" values of angles are always - if the angle > 180 
		 */

		Vector2 v0 = RawPoints[PolyPointList[P].PrevP-1]- RawPoints[P-1];
		Vector2 v1 = RawPoints[PolyPointList[P].NextP-1]- RawPoints[P-1];

		Vector3 A = Vector3.Cross(v0,v1);

		if(A.z<0)
			return true;
	
		return false;
	}
	
	private static void RemoveEar(int Ear){

		int PrevEar = PolyPointList[Ear].PrevEar;
		int NextEar = PolyPointList[Ear].NextEar;

		PolyPointList[Ear].isEar = false;

		if(PrevEar==-1){
			PolyPointList[0].NextEar = NextEar;
		}
		else{
			PolyPointList[PrevEar].NextEar = NextEar;
		}
		
		if(NextEar!=-1){
			PolyPointList[NextEar].PrevEar = PrevEar;
		}
	}

	private static void AddEar(int Ear){

		int NextEar=PolyPointList[0].NextEar;

		PolyPointList[0].NextEar = Ear;
		
		PolyPointList[Ear].PrevEar = -1;
		PolyPointList[Ear].NextEar = NextEar;

		PolyPointList[Ear].isEar = true;

		if(NextEar!=-1){

			PolyPointList[NextEar].PrevEar = Ear;

		}
	
	}

	private static void RemoverReflective(int P){

		int PrevRefl = PolyPointList[P].PrevRefl;
		int NextRefl = PolyPointList[P].NextRefl;
		
		if(PrevRefl==-1){
			PolyPointList[0].NextRefl = NextRefl;
		}
		else{
			PolyPointList[PrevRefl].NextRefl = NextRefl;
		}
		
		if(NextRefl!=-1){
			PolyPointList[NextRefl].PrevRefl = PrevRefl;
		}

	}

	private static void AddReflective(int P){

		int NextRefl=PolyPointList[0].NextRefl;
		
		PolyPointList[0].NextRefl = P;
		
		PolyPointList[P].PrevRefl = -1;
		PolyPointList[P].NextRefl = NextRefl;
		
		if(NextRefl!=-1){
			
			PolyPointList[NextRefl].PrevRefl = P;
			
		}

	}
	
	private static void RemoveP(int P){

		int NextP = PolyPointList[P].NextP;
		int PrevP = PolyPointList[P].PrevP;

		PolyPointList[PrevP].NextP=NextP;
		PolyPointList[NextP].PrevP=PrevP;

		if(PolyPointList[0].NextP==P)
			PolyPointList[0].NextP=NextP;

		--Pointcount;
	}


}
