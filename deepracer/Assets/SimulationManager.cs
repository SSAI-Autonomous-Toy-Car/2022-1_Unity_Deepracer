using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PathCreation;
using Unity.MLAgents.Sensors;
using System.Linq;

public class SimulationManager : MonoBehaviour
{
    public PathCreator pathCreator;
	//public Camera mCamera;
    
    // returns output = [closest waypoint, next waypoint ,distance]
    public float[] distFromCenter(Vector3 carLocation)
    {
    	float[] output = new float[7];
    	output[1] = 0f;
    	output[4] = 0f;
    	float dist = 100f;
    	float currentDist;
    	int t = 0;
    	
    	float carX = carLocation.x;
    	float carZ = carLocation.z;
    	Vector3 trackPoint;
    	print(pathCreator.path.localPoints);
    	
    	for(int i=0; i<100 ;i++)
    	{
    	    trackPoint = pathCreator.path.GetPointAtTime(0.01f*i);
    	    currentDist = Mathf.Sqrt(Mathf.Pow(carX-trackPoint[0],2) + Mathf.Pow(carZ-trackPoint[2],2));
    	    if(dist > currentDist)
    	    {
    	    	dist = currentDist;
    	    	output[0] = trackPoint[0];
    	    	output[2] = trackPoint[2];
    	    	output[6] = dist;
    	    	t = i;
    	    }
    	    
    	    
    	}
    	trackPoint = pathCreator.path.GetPointAtTime(t*0.01f);
    	output[3] = trackPoint[0];
    	output[5] = trackPoint[2];
    	
    	return output;
    	
    }
    
	
    public float[] lidarSectorize(RayPerceptionSensorComponent3D rayComponent)
	{
		float maxDist = 0.5f;
		
		var rayOutputs = RayPerceptionSensor
                .Perceive(rayComponent.GetRayPerceptionInput())
                .RayOutputs;
		
		
        var lengthOfRayOutputs = RayPerceptionSensor
                    .Perceive(rayComponent.GetRayPerceptionInput())
                    .RayOutputs
                    .Length;
 
        float[] distList = new float[lengthOfRayOutputs];
		int rayNumInSector = (int)lengthOfRayOutputs / 8;
		float[] sectorLidarData = new float[8];
            
        for (int i = 0; i < lengthOfRayOutputs; i++)
        {
            //GameObject goHit = rayOutputs[i].HitGameObject;
            
            // Found some of this code to Denormalized length
            // calculation by looking trough the source code:
            // RayPerceptionSensor.cs in Unity Github. (version 2.2.1)
            var rayDirection = rayOutputs[i].EndPositionWorld - rayOutputs[i].StartPositionWorld;
            var scaledRayLength = rayDirection.magnitude;
            distList[i] = rayOutputs[i].HitFraction * scaledRayLength;   
        }

		for (int i = 0; i < 8; i++)
		{
			sectorLidarData[i] = 0f;
			for (int j = 0; j < rayNumInSector; j++)
			{
				if (distList[i*rayNumInSector+j] < maxDist)
				{
					sectorLidarData[i] = 1f;
				}
			}
		}
            
        return sectorLidarData;
	}

	/*
	public void RTImage()
    {
		int mWidth = 160;
		int mHeight = 120;
        Rect rect = new Rect(0, 0, mWidth, mHeight);
        RenderTexture renderTexture = new RenderTexture(mWidth, mHeight, 24);
        Texture2D screenShot = new Texture2D(mWidth, mHeight);
 
        mCamera.targetTexture = renderTexture;
        mCamera.Render();
 
        RenderTexture.active = renderTexture;
        screenShot.ReadPixels(rect, 0, 0);
 
        mCamera.targetTexture = null;
        RenderTexture.active = null;
 
        Destroy(renderTexture);
        renderTexture = null;
        
    }
	*/
    
    
}
