using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController instance;
    private void Awake() {
        instance = this;
    }
    //GET NEIGHBOURS
	ProvinceData[]	GetNeighbours(ProvinceData prov, Transform[,] grid) {
		//grid[0,zSize-1].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,0].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,zSize-1].transform.position+= new Vector3(0,100,0);
        float zSize = grid.GetLength(1);
        float xSize = grid.GetLength(0);
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
    public void DeformTerrain (Transform[,] grid) {
		//grid[0,zSize-1].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,0].transform.position+= new Vector3(0,100,0);
		//grid[xSize-1,zSize-1].transform.position+= new Vector3(0,100,0);
        float zSize = grid.GetLength(1);
        float xSize = grid.GetLength(0);
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
}
