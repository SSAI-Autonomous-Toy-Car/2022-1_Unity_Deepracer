using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using PathCreation;
using System;

[RequireComponent(typeof(Controller))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(SimulationManager))]

public class DeepracerAgent : Agent
{
    public PathCreator pathCreator;
    public RayPerceptionSensorComponent3D _lidar;
    public Camera leftCamera;
    public Camera rightCamera;

    private Rigidbody _rigidBody;
    private Controller _controller;
    private SimulationManager _simulationManager;
    private float[] _lastActions;
    public int fileName = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        _rigidBody = GetComponent<Rigidbody>();
        _controller = GetComponent<Controller>();
        _simulationManager = GetComponent<SimulationManager>();
    }

    // Update is called once per frame
    public override void OnEpisodeBegin()
    {
        Vector3 deepracerPos = this.transform.localPosition;
        float t = pathCreator.path.GetClosestTimeOnPath(deepracerPos);
        //t = t-0.1f;
        this.transform.localPosition = pathCreator.path.GetPointAtTime(t);
        this.transform.rotation = Quaternion.LookRotation(pathCreator.path.GetDirection(t), Vector3.up);

        _controller.CurrentSteeringAngle = 0f;
        _controller.CurrentAcceleration = 0f;
    }
    
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        _controller.CurrentSteeringAngle = actionBuffers.ContinuousActions[0];
        _controller.CurrentAcceleration = actionBuffers.ContinuousActions[1];
        // _controller.CurrentAcceleration shows Mathf.clamp of actionBuffers.ContinuousActions[1]. 
        // that is, _controller.CurrentAcceleration: [0.1, 1]   whereas actionBuffers.ContinuousActions[1]: [-1, 1]
        
        float[] waypointsDist = _simulationManager.distFromCenter(this.transform.localPosition);
        //float dist = pathCreator.path.GetClosestDistanceAlongPath(this.transform.localPosition);
        
        if(actionBuffers.ContinuousActions[1] > 0.5f)
        {
            AddReward(0.1f);
        }

        if(actionBuffers.ContinuousActions[1] < 0.2f)
        {
            AddReward(-0.1f);
        }

        if(waypointsDist[6] > 0.3f)
        {
            EndEpisode();
        }
        
        if(waypointsDist[6] < 0.1f)
        {
            AddReward(0.1f);
        }

        if((actionBuffers.ContinuousActions[0] > 0.8f && actionBuffers.ContinuousActions[1] < 0.2f) || (actionBuffers.ContinuousActions[0] < -0.8f && actionBuffers.ContinuousActions[1] < 0.2f))
        {
            AddReward(0.05f);
        }

        Debug.Log("Steering: " + actionBuffers.ContinuousActions[0] + "    Accelerate: " + _controller.CurrentAcceleration);
        Debug.Log("Lidar = " + String.Join("", new List<float>(_simulationManager.lidarSectorize(_lidar)).ConvertAll(i => i.ToString())));

        RayCastInfo(_lidar);    

        // save camera image png file
        if(fileName <= 20)
        {
            GetCameraImage(leftCamera, 0, fileName);
            GetCameraImage(rightCamera, 1, fileName);
            Debug.Log(fileName + "Steering: " + actionBuffers.ContinuousActions[0] + "    Accelerate: " + _controller.CurrentAcceleration);

            fileName++;
        }
    }
    
    public override void CollectObservations(VectorSensor sensor)
    {
    	sensor.AddObservation(_simulationManager.lidarSectorize(_lidar));
    	
    }
    
    
    public override void Heuristic(in ActionBuffers actionsOut)
    {
    	var continuousActionsOut = actionsOut.ContinuousActions;
    	continuousActionsOut[0] = Input.GetAxis("Horizontal");
    	continuousActionsOut[1] = Input.GetAxis("Vertical");
    }
    
    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("car") || other.gameObject.CompareTag("wall"))
        {
            AddReward(-0.1f);
            EndEpisode();
        }
    }

    private void RayCastInfo(RayPerceptionSensorComponent3D rayComponent)
    {
        var rayOutputs = RayPerceptionSensor
                .Perceive(rayComponent.GetRayPerceptionInput())
                .RayOutputs;
 
        
        var lengthOfRayOutputs = RayPerceptionSensor
                    .Perceive(rayComponent.GetRayPerceptionInput())
                    .RayOutputs
                    .Length;
 
        Debug.Log(lengthOfRayOutputs);

        float[] distList = new float[lengthOfRayOutputs];
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
        
        Debug.Log("LidarDist = " + String.Join("", new List<float>(distList).ConvertAll(i => i.ToString())));

    }



    public void GetCameraImage(Camera mCamera, int rightOrLeft, int fileName)
    {
        Texture2D screenShot = new Texture2D(160, 120);
        CameraSensor.ObservationToTexture(mCamera, screenShot, 160, 120);
        byte[] screenBytes = screenShot.EncodeToPNG();
        string directoryPath = "/home/deepracer/UnityData/";
        string filePath = "";
        if(rightOrLeft==0)
        {
            filePath = directoryPath + "left" + fileName.ToString() + ".png";
        }
        else
        {
            filePath = directoryPath + "right" + fileName.ToString() + ".png";
        }
        
        UnityEngine.Windows.File.WriteAllBytes(filePath, screenBytes);
    }
}
