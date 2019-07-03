using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController instance;
	[Header("Map Generation")]
	public GameObject prefabBlock;
	public GameObject prefabProvince;
	Transform containerBoard;
    private void Awake() {
        instance = this;
    }

	#region Map Data Distribution
	public IEnumerator DistributeProvincesAndCells(Transform[,] grid, List<ProvinceData> provinces)	{
		containerBoard = new GameObject("Board").transform;
		float zSize = grid.GetLength(1);
        float xSize = grid.GetLength(0);
		List<Vector3> points = new List<Vector3>();
		int variation = Random.Range(-2,3);
		int newX,newY;
		newX = (int)Mathf.Sqrt(xSize)-1;
		newY = (int)Mathf.Sqrt(zSize)-1;
		for(int z = newY/2; z < zSize; z+=newY-1){
			for(int x = newX/2; x < xSize; x+=newX-1){
				variation = Random.Range(-2,3);
				if((int)variation+x<xSize &&(int)variation+z<zSize&&(int)variation+x>=0&&(int)variation+z>=0)
					if(grid[(int)variation+x,z+(int)variation])
						points.Add(new Vector3((int)variation+x,0,z+(int)variation));
			}
		}
		foreach(Vector3 v in points){
			Transform obj = Instantiate(prefabProvince,v,Quaternion.identity).transform;
			obj.parent = containerBoard;
			ProvinceData province = obj.gameObject.GetComponent<ProvinceData>();
			province.name = "Province "+ Random.Range(0,99)+""+ (int)Time.realtimeSinceStartup*10+""+ Random.Range(0,99)+province.GetInstanceID().ToString();
			province.owner = null;
			obj.name = province.name;
			provinces.Add(province);
		}

		//Foreach block in the grid
		for(int z = 0; z < zSize; z++){
			for(int x = 0; x < xSize; x++){
				if(grid[x,z])
					if(grid[x,z].GetComponent<CellData>()){
						CellData c = grid[x,z].GetComponent<CellData>();
						float closestDist = 9999;
						ProvinceData closestProvince = provinces[0];
						//Calculate closest province to the block
						foreach(ProvinceData p in provinces) {
							float distance = Vector3.Distance(grid[x,z].position,p.transform.position);
							if(distance <= closestDist){
								closestDist = distance;
								closestProvince = p;
							}
						}
						//Change cell ownership and add to lists
						//c.owner = closestProvince.owner.name;
						c.transform.parent = closestProvince.transform;
						c.province = closestProvince.name;
						closestProvince.territory.Add(c);

					}
			}
		}
		yield return null;
	}
    //GET NEIGHBOURS
	public ProvinceData[] GetNeighbours(ProvinceData prov, Transform[,] grid,List<ProvinceData> provinces) {
		//grid[0,zSize-1].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,0].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,zSize-1].transform.position+= new Vector3(0,100,0);
        int zSize = grid.GetLength(1);
        int xSize = grid.GetLength(0);
		List<ProvinceData> neighb = new List<ProvinceData>();

		//Check every cell that is territory to the drovince you are studying
		for(int i = 0 ;i < prov.territory.Count; i++) {
			//Check only the territorys that are not null
			if(prov.territory[i] != null){
			//Check neighbouring cells
			CellData checkingNow = null;
			//Analyzing the one on the right
			int aX =(int)prov.territory[i].coordinates.x+1;
			int aY =(int)prov.territory[i].coordinates.y;
			//Checking if it is valid
			if(aX < xSize)
			if(grid[aX,aY]){
				//Make a reference to the cell
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				//Check if it is owned by another drovince
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					//Comdare the name of the drovince owning the current tile you are checking
					//To all the drovinces. When they match make a reference to it as alienDrovince
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					//Check if the alien drovince is not already a neighbour
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;

					//If it is not a neighbour already, add it to the list
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x-1;
			aY =(int)prov.territory[i].coordinates.y;
			if(aX >= 0)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x;
			aY =(int)prov.territory[i].coordinates.y+1;
			if(aY < zSize)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
			aX =(int)prov.territory[i].coordinates.x;
			aY =(int)prov.territory[i].coordinates.y-1;
			if(aY >= 0)
			if(grid[aX,aY]){
				checkingNow = grid[aX,aY].GetComponent<CellData>();
				if(checkingNow.province != prov.name) {
					//print("The cell " + aX +" "+aY+" has a neighbour on " +aX+" "+aY +" called "+ checkingNow.province+" and is "+i+" "+prov.territory.Count);
					string provinceName = checkingNow.province;
					ProvinceData alienProvince=null;
					foreach(ProvinceData p in provinces) 
						if(p.name == provinceName)
							alienProvince = p;
					
					bool newS = true;
					foreach(ProvinceData p in neighb) 
						if(p.name == provinceName)
							newS = false;
					
					if(newS)
						if(alienProvince!= null)
							neighb.Add(alienProvince);

				}
			}
		}
		}

		

		ProvinceData[] endProvinces= new ProvinceData[neighb.Count];
		string debug = "";
		for(int a = 0; a < endProvinces.Length; a++){
			endProvinces[a] = neighb[a];
			debug+= neighb[a];
		}

		//Debug.Log(prov.name+" has "+ neighb.Count + ": "+ debug);
		return (endProvinces);
	}
	#endregion
	#region Map Generation
	public void PolishCells(Transform[,] grid,int minNeighbourQuantity,int xSize, int zSize) {
		print ("Polishing cells for " + minNeighbourQuantity);
		for(int z = 0; z < zSize; z++){
			for(int x = 0; x < xSize; x++){
				//Check the current x,y cell if it exists
				if(grid[x,z]) {
					int cX, cZ;
					int qnt = 0;
					//Get right neighbour
					cX = x + 1;
					cZ = z;
					if (cX < xSize) {
						if (grid [cX, cZ])
							qnt++;
					}
					//Get left neighbour
					cX = x - 1;
					cZ = z;
					if (cX >= 0) {
						if (grid [cX, cZ])
							qnt++;
					}
					//Get top neighbour
					cX = x;
					cZ = z+1;
					if (cZ < zSize) {
						if (grid [cX, cZ])
							qnt++;
					}
					//Get bot neighbour
					cX = x;
					cZ = z-1;
					if (cZ >= 0) {
						if (grid [cX, cZ])
							qnt++;
					}
					if (qnt < minNeighbourQuantity) {
						Destroy (grid [x, z].gameObject);
						grid [x, z] = null;
					}
				}
			}
		}

	}
    public void DeformTerrain (Transform[,] grid) {
		//grid[0,zSize-1].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,0].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,zSize-1].transform.position+= new Vector3(0,100,0);
        int zSize = grid.GetLength(1);
        int xSize = grid.GetLength(0);
		for(int y = 0; y < zSize;y++){
			for(int x = 0; x < xSize; x++){
				//Run code only if there is a cell there
				if(grid[x,y]) {
					int m,n;
					//Left
					m = -1; n = 0;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(x+m >= 0){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}

					m = 0; n = -1;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(y+n >= 0){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}
					m = 1; n = 0;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(x+m < xSize){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}
					m = 0; n = 1;
					//Check if the coordinates are valid, if not mark neighbour as inexistent
					if(y+n < zSize){
						if(grid[x+m,y+n] == null)	{
							grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
						}
					} else {
						grid[x,y].transform.localScale+= new Vector3(0,-0.25f,0);
							grid[x,y].transform.position += new Vector3(0,-0.25f,0);
					}

				}
				

			}
		}
	}

	//MAP GENERATION
	public Transform[,] GenerateMap(int xSize, int zSize) {
		Transform[,] grid = new Transform[xSize,zSize];
		float seed = Random.Range(1f,999f);
//		float xStretch;
		float yStretch;
		for(int z = 0; z < zSize; z++){
			yStretch = Mathf.Lerp(0,1f,z*1f/((float)zSize+1));
			//print(yStretch);
			for(int x = 0; x < xSize; x++){
				
				
				
				float perlin = Mathf.PerlinNoise(( (float)x+seed)/( (float)xSize/5 ),( (float)z+seed)/( (float)zSize/3));
				//print(z/(zSize));
				//print(perlin);
				perlin = perlin-yStretch;
				//if(perlin > 0.4f&&perlin < 0.6f){
				if(perlin > 0.05f+Random.Range(-0.04f,0.02f)){
					Transform t = Instantiate(prefabBlock, new Vector3(x*1,0,z*1), Quaternion.identity).transform;
					t.parent = containerBoard;
					t.GetComponent<CellData>().coordinates = new Vector2(x,z);
					grid[x,z] = t;
					//UpdateCellAppearance(c);
				} else {
					grid[x,z] = null;
				}
				
				
				//print(grid[x,z]+" "+c.coordinates);
			}
		}
		return(grid);
	} 
	#endregion
}
