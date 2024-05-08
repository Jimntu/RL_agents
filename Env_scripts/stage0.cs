using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

// STAGE 0

public class stage0 : Agent
{
    private GameObject Target;
    private Rigidbody rBody;
    public Material target_material;
    // private string target_name;
    // private int currentPrefabIndex;
    private List<GameObject> prefabList;
    private List<string> prefabNames;
    private List<Color> colorList;
    public float objectsScaleFactor=1.0f;
    public GameObject referenceMan;
    public float leftRightLimits=14f;
    public float frontBackLimits=18f;
    public float bounceBackDistance=2f;


    public override void Initialize()
    {
        // Load all prefabs from the "Prefabs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        // Convert the array to a list
        prefabList = new List<GameObject>(prefabs);
        // initialise an array and add names of the prefabs to it
        prefabNames = new List<string>();
        for (int i=0; i<prefabList.Count;i++)
        {
            prefabNames.Add(prefabList[i].name);
        }

        //get the RB component so we can access in the script. refers to the agent
        rBody = GetComponent<Rigidbody>(); 

        //initialise colours to use
        colorList = new List<Color>(new Color[] {new Color(1,0,0),new Color(0,1,0),new Color(0,0,1),new Color(1, 0.92f, 0.016f),new Color(0,0,0)});
        // colorList = new List<Color>(new Color[] {new Color(1,0,0),new Color(0,1,0),new Color(0,0,1),new Color(1, 1, 1),new Color(0,0,0)});
    }

    public override void OnEpisodeBegin()
    {
        if (Target != null) // Destroy the old target, if any, need this because i am setting a max step in the scene
        {
            Destroy(Target);
        }
        // this script is directly attached to the agent gameobject, so this refers to the agent directly
        this.transform.localPosition = new Vector3(0,0.5f,-15); // move the agent back to its original location
        this.transform.localRotation = Quaternion.identity; // reset the rotation of the agent
        rBody.velocity = Vector3.zero; // reset velocity of agent to zero, this step may not be necessary
        // this.transform.localScale = Vector3.one * objectsScaleFactor;

        int prefab_index = Random.Range(0, prefabList.Count); // get random int 
        GameObject prefab = prefabList[prefab_index]; // randomly get a prefab from prefabsList by using the previous random int
        // Vector3[] positions = {new Vector3(-5,1*objectsScaleFactor/2,5),new Vector3(5,1*objectsScaleFactor/2,5),new Vector3(-5,1*objectsScaleFactor/2,-5),new Vector3(5,1*objectsScaleFactor/2,-5)};  // initialise the 4 fixed positions
        Vector3[] positions = {new Vector3(-4,1*objectsScaleFactor/2,10),new Vector3(4,1*objectsScaleFactor/2,10),new Vector3(-8,1*objectsScaleFactor/2,1),new Vector3(8,1*objectsScaleFactor/2,1)};  // initialise the 4 fixed positions

        int random_pos = Random.Range(0,4); // get random int
        Target = Instantiate(prefab, positions[random_pos], Quaternion.identity); // instantiate the Target GO by using the selected prefab and randomly spawn a position using previous random int
        Target.transform.localScale = Vector3.one * objectsScaleFactor;
        Renderer renderer = Target.GetComponent<Renderer>(); // access renderer to get material component
        renderer.material = target_material; // set the material component of target to the target_material variable

        // referenceMan.transform.localScale *= objectsScaleFactor;


        int random_color = Random.Range(0,5);
        target_material.color = colorList[random_color];
        // float r = Random.Range(0f, 1f); // get random float
        // float g = Random.Range(0f, 1f); // get random float
        // float b = Random.Range(0f, 1f); // get random float
        // target_material.color = new Color(r, g, b); // randomise the colours of the cubes at start of every episode
        // target_name = Target.name.Substring(0,Target.name.Length-7);
        // currentPrefabIndex = prefabNames.IndexOf(target_name);
        // // index printed 0-4: capsule,cube,cylinder,prism,sphere
        // //                      0 1 2 3 4

        // move up the capsule and cylinder by the objectsScaleFactor to avoid them being stuck in the floor
        // Get all 4 objects in the scene
        GameObject[] prefabs_ =  { Target };

        foreach (GameObject obj in prefabs_)
        {
            // Check if the object name starts with "Capsule" or "Cylinder"
            if (obj.name.StartsWith("Capsule") || obj.name.StartsWith("Cylinder"))
            {
                // Set the y coordinate to 3
                Vector3 newPosition = new Vector3(obj.transform.localPosition.x, objectsScaleFactor, obj.transform.localPosition.z);
                obj.transform.localPosition = newPosition;
            }
        }
    }

    // public override void CollectObservations(VectorSensor sensor)
    // {
    //     // sensor.AddOneHotObservation((int)currentPrefabIndex,prefabNames.Count);
    //     sensor.AddObservation(currentPrefabIndex);
    // }

    public float movementSpeed = 150;
    public float rotationSpeed = 200;
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {

        float moveForward = actionBuffers.DiscreteActions[0]; 
        float moveBackward = actionBuffers.DiscreteActions[1];
        float rotateRight = actionBuffers.DiscreteActions[2];
        float rotateLeft = actionBuffers.DiscreteActions[3];
        float move = (moveForward-moveBackward) * movementSpeed * Time.deltaTime;
        float rotate = (rotateRight-rotateLeft) * rotationSpeed * Time.deltaTime;

        Vector3 movement = rBody.transform.forward * move;
        Vector3 current_position = rBody.transform.localPosition;
        Vector3 new_position = current_position + movement;
        float distance = Vector3.Distance(new_position,new Vector3(0,0,19));

        if (distance < 2) // if next action will enter the radius of ref man, then dont take action, rotation is fine
        {
            SetReward(-1.0f);
            Vector3 backwardVector = -rBody.transform.forward;
            backwardVector.Normalize();
            Vector3 newPosition = current_position + (backwardVector * bounceBackDistance);
            rBody.MovePosition(newPosition);
        }

        else
        {
            rBody.MovePosition(rBody.position + movement);
            rBody.transform.Rotate(0, rotate, 0);
        }

        // this entire block is activated when the OnCollisionEnter block fails, and the agent can still somehow  break through the wall
        if (current_position.x <-leftRightLimits) // hit the left wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x+bounceBackDistance,current_position.y,current_position.z);
            SetReward(-1.0f);
        }
        else if (current_position.x > leftRightLimits) // hit the right wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x-bounceBackDistance,current_position.y,current_position.z);
            SetReward(-1.0f);
        }
        else if (current_position.z < -frontBackLimits) // hit the back wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z+bounceBackDistance);
            SetReward(-1.0f);
        }
        else if (current_position.z > frontBackLimits) // hit the front wall
        {
            rBody.transform.localPosition = new Vector3(current_position.x,current_position.y,current_position.z-bounceBackDistance);
            SetReward(-1.0f);
        }
    }


    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = Mathf.RoundToInt(Input.GetAxisRaw("move forward"));
        discreteActionsOut[1] = Mathf.RoundToInt(Input.GetAxisRaw("move backward"));
        discreteActionsOut[2] = Mathf.RoundToInt(Input.GetAxisRaw("rotate right"));
        discreteActionsOut[3] = Mathf.RoundToInt(Input.GetAxisRaw("rotate left"));
    }


    // reward function for when agent collides with reference man, wall or target
    void OnCollisionEnter(Collision collided_object)
    {
        if(collided_object.gameObject.name == "wall")
        // if(collided_object.gameObject.name == "reference man")
        {
            SetReward(-1.0f);
        }

        else if (collided_object.gameObject.name == Target.name)
        {
            SetReward(10.0f);
            // GameObject.Destroy(Target);
            EndEpisode();
        }
    }
}