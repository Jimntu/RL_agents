using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;
// STAGE 0

public class stage3_env1_train : Agent
{
    private List<GameObject> Targets;
    private List<GameObject> otherObject1s;
    private List<GameObject> otherObject2s;
    private List<GameObject> otherObject3s;
    private Rigidbody rBody;
    private List<Vector3> allList;
    private List<GameObject> prefabList;
    private List<Color> colorList;
    // private List<int> determiners;
    private List<int> assigned_pos;
    private List<AttributeTuple> AttributesList;
    public Material material1;
    public Material material2;
    public Material material3;
    public Material material4;
    public float objectsScaleFactor=1.0f;
    public float leftRightLimits=14f;
    public float frontBackLimits=18f;
    public float bounceBackDistance=2f;
    public AttributeTuple targetAttributes;
    private int targetAttributesIndex;
    private int target_determiner;
    private bool hasLogged;

    // define a class to hold color and gameobject tgt
    public class AttributeTuple
    {
        public int color;
        public int obj;

        public AttributeTuple(int obj,int color)
        {
            this.obj = obj;
            this.color = color;
        }
    }

    public override void Initialize()
    {
        //get the RB component so we can access in the script. refers to the agent
        rBody = GetComponent<Rigidbody>(); 

        // Load all prefabs from the "Prefabs" folder
        GameObject[] prefabs = Resources.LoadAll<GameObject>("Prefabs");
        prefabList = prefabs.OrderBy(prefab => prefab.name).ToList(); 
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere

        //initialise colours to use
        colorList = new List<Color>(new Color[] {new Color(1,0,0),new Color(0,1,0),new Color(0,0,1),new Color(1, 0.92f, 0.016f),new Color(0,0,0)});
        // 0-red,1-green,2-blue,3-yellow,4-black
        AttributesList = new List<AttributeTuple>();
        // create train and test attributesList
        for (int i = 0; i < prefabList.Count; i++)
        {
            for (int j = 0; j < colorList.Count; j++)
            {
                AttributesList.Add(new AttributeTuple(i, j));
            }
        }

        allList = new List<Vector3>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    allList.Add(new Vector3(i, j, k));
                }
            }
        }}

    public List<Vector3> Position(int pos, int det)
    {
        List<Vector3> xyzList = new List<Vector3>();
        // List<Vector3> allList = new List<Vector3>();

        // for (int i = -1; i <= 1; i++)
        // {
        //     for (int j = 1; j <= 3; j++)
        //     {
        //         for (int k = -1; k <= 1; k++)
        //         {
        //             allList.Add(new Vector3(i, j, k));
        //         }
        //     }
        // }

        System.Random rand = new System.Random();
        List<int> randomIndices = new List<int>();
        List<List<int>> nums = new List<List<int>>
        {
            new List<int> { 1 },
            new List<int> { 2, 3 },
            new List<int> { 4, 5, 6 },
            new List<int> { 7, 8, 9 },
            new List<int> { 1 },
            new List<int> { 1 },
            new List<int> { 2, 3, 4, 5, 6, 7, 8, 9},
            new List<int> { 2, 3, 4, 5, 6, 7, 8, 9}
        };
        List<int> selectedSublist = nums[det];
        int num = selectedSublist[rand.Next(selectedSublist.Count)];
        while (randomIndices.Count < num)
        {
            int randomIndex = rand.Next(0, 27);

            if (!randomIndices.Contains(randomIndex))
            {
                randomIndices.Add(randomIndex);
            }
        }
        foreach (int i in randomIndices)
        {
            xyzList.Add(allList[i]);
        }

        for (int j = 0; j < xyzList.Count; j++)
        {
            Vector3 modifiedElement = xyzList[j]; // Create a copy of the element

            if (pos == 0) modifiedElement.x -= 5.5f;
            if (pos == 1) modifiedElement.x += 5.5f;
            if (pos == 2) modifiedElement.x -= 3;
            if (pos == 3) modifiedElement.x += 3;
            modifiedElement.z += (pos < 2 ? -5 : 5);  //[-6,0,-5],[6,0,-5],[-4,0,5],[4,0,5]
            //  2   3
            //0       1
            
            xyzList[j] = modifiedElement; // Add the modified element to the new list
        }

        return xyzList;
    }
  
    public override void OnEpisodeBegin()
    {   
        hasLogged = false;
        // Destroy the old objects. This is needed because objects wont be destroyed at the end if max steps is reached. So destroy them first at the start of new Episode
        if (Targets != null)
        {
            foreach (GameObject target in Targets)
            {
                if (target != null)
                {
                    Destroy(target);
                }
            }
            Targets.Clear(); // Optionally, clear the list after destroying all objects.
        }

        if (otherObject1s != null)
        {
            foreach (GameObject obj in otherObject1s)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject1s.Clear();
        }

        if (otherObject2s != null)
        {
            foreach (GameObject obj in otherObject2s)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject2s.Clear();
        }

        if (otherObject3s != null)
        {
            foreach (GameObject obj in otherObject3s)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject3s.Clear();
        }

        // reset agent position and rotation
        this.transform.localPosition = new Vector3(0,1,-18); // move the agent back to its original location
        this.transform.localRotation = Quaternion.identity; // make sure the rotation of the agent is also reset at the beginning of each episode

        // select AttributeTuple for target
        targetAttributesIndex = Random.Range(0,AttributesList.Count);

        // // select AttributeTuple for other objects
        int otherObject1_AttributesIndex = Random.Range(0,AttributesList.Count);
        while (otherObject1_AttributesIndex==targetAttributesIndex)
        {
            otherObject1_AttributesIndex = Random.Range(0,AttributesList.Count);
        }
        int otherObject2_AttributesIndex = Random.Range(0,AttributesList.Count);
        while (otherObject2_AttributesIndex==targetAttributesIndex)
        {
            otherObject2_AttributesIndex = Random.Range(0,AttributesList.Count);
        }
        int otherObject3_AttributesIndex = Random.Range(0,AttributesList.Count);
        while (otherObject3_AttributesIndex==targetAttributesIndex)
        {
            otherObject3_AttributesIndex = Random.Range(0,AttributesList.Count);
        }

        // call the attributetuple for all 4 objects
        targetAttributes = AttributesList[targetAttributesIndex];
        AttributeTuple otherObject1_Attributes = AttributesList[otherObject1_AttributesIndex];
        AttributeTuple otherObject2_Attributes = AttributesList[otherObject2_AttributesIndex];
        AttributeTuple otherObject3_Attributes = AttributesList[otherObject3_AttributesIndex];

        // list down the positions 
        // determiners = Enumerable.Range(0, 8).OrderBy(x => Guid.NewGuid()).ToList();
        // 0 -- A, 1 -- Few, 2 -- Some, 3 -- Many, 4 -- this, 5 -- that, 6 -- these, 7 -- those
        System.Random rand = new System.Random();
        target_determiner = Random.Range(0,8);
        while(targetAttributes.obj == target_determiner || targetAttributes.obj + 3 == target_determiner)
        {
            target_determiner = Random.Range(0,8);
        }
        
               
        int otherObject2_determiner = Random.Range(0,8);
        if (target_determiner == 0)   // Target: A Others: No this/that
        {
            while (targetAttributes.obj == otherObject2_determiner || targetAttributes.obj+3 == otherObject2_determiner || target_determiner == otherObject2_determiner || otherObject2_determiner == 4 || otherObject2_determiner == 5)
            {
                otherObject2_determiner = Random.Range(0,8);
            }
        }
        else if (target_determiner == 1 || target_determiner == 2 || target_determiner == 3)  //Target: Few, Many, Some  Others: No these/those
        {
            while (targetAttributes.obj == otherObject2_determiner || targetAttributes.obj+3 == otherObject2_determiner || target_determiner == otherObject2_determiner || otherObject2_determiner == 6 || otherObject2_determiner == 7)
            {
                otherObject2_determiner = Random.Range(0,8);
            }
        }
        else
        {
            while(targetAttributes.obj == otherObject2_determiner || targetAttributes.obj+3 == otherObject2_determiner || target_determiner == otherObject2_determiner)
            {
                otherObject2_determiner = Random.Range(0,8);
            }
        }

        int otherObject3_determiner = Random.Range(0,8);
        while(otherObject3_Attributes.obj == otherObject3_determiner || otherObject3_Attributes.obj+3 == otherObject3_determiner || target_determiner == otherObject3_determiner || otherObject2_determiner == otherObject3_determiner)
        {
            otherObject3_determiner = Random.Range(0,8);
        }

        if (target_determiner == 4 || target_determiner == 6) // target -- this or these
        {
            int[] posOptions1 = { 2, 3, 0, 1 };
            int[] posOptions2 = { 3, 2, 0, 1 };
            int[] posOptions3 = { 2, 3, 1, 0 };
            int[] posOptions4 = { 3, 2, 1, 0 };
            int[][] possibleOptions = { posOptions1, posOptions2, posOptions3, posOptions4};

            assigned_pos = new List<int> (possibleOptions[rand.Next(possibleOptions.Length)].ToList());
        }
        else if (target_determiner == 5 || target_determiner == 7) // that or those
        {
            int[] posOptions1 = { 0, 1, 2, 3 };
            int[] posOptions2 = { 0, 1, 3, 2 };
            int[] posOptions3 = { 1, 0, 2, 3 };
            int[] posOptions4 = { 1, 0, 3, 2 };

            int[][] possibleOptions = { posOptions1, posOptions2, posOptions3, posOptions4 };

            assigned_pos = new List<int> (possibleOptions[rand.Next(possibleOptions.Length)].ToList());
        }
        else
        {
            assigned_pos = new List<int> { 0, 1, 2, 3 };
            assigned_pos = assigned_pos.OrderBy(x => rand.Next()).ToList();
        }

        // create list of GameObjects
        // List<GameObject> gameObjects = new List<GameObject>{Target,otherObject1,otherObject2,otherObject3};
    
        // Instantiate the prefabs at their assigned positions
            // attributes[0] gives Object, attributes[1] give Colour
        
        //--------------TARGET---------------------
        List<Vector3> targetPositions = Position(assigned_pos[0],target_determiner);
        Targets = new List<GameObject>();
        foreach (Vector3 position in targetPositions)
        {
            GameObject target = Instantiate(prefabList[targetAttributes.obj], position, Quaternion.identity);
            Targets.Add(target);
        }

        //--------------OBJECT1---------------------  Determiner is the same
        List<Vector3> otherObject1Positions = Position(assigned_pos[1],target_determiner); 
        otherObject1s = new List<GameObject>();
        foreach (Vector3 position in otherObject1Positions)
        {
            GameObject otherObject1 = Instantiate(prefabList[otherObject1_Attributes.obj], position, Quaternion.identity);
            otherObject1s.Add(otherObject1);
        }

        //--------------OBJECT2---------------------  C&S is the same
        List<Vector3> otherObject2Positions = Position(assigned_pos[2],otherObject2_determiner); 
        otherObject2s = new List<GameObject>();
        foreach (Vector3 position in otherObject2Positions)
        {
            GameObject otherObject2 = Instantiate(prefabList[targetAttributes.obj], position, Quaternion.identity);
            otherObject2s.Add(otherObject2);
        }

        //--------------OBJECT3---------------------
        List<Vector3> otherObject3Positions = Position(assigned_pos[3],otherObject3_determiner);
        otherObject3s = new List<GameObject>();
        foreach (Vector3 position in otherObject3Positions)
        {
            GameObject otherObject3 = Instantiate(prefabList[otherObject3_Attributes.obj], position, Quaternion.identity);
            otherObject3s.Add(otherObject3);
        }

        // Target = Instantiate(targetAttributes.obj, positions[assigned_pos[0]][numbers[0]], Quaternion.identity);
        // otherObject1 = Instantiate(otherObject1_Attributes.obj, positions[assigned_pos[1]][numbers[1]], Quaternion.identity);
        // otherObject2 = Instantiate(otherObject2_Attributes.obj, positions[assigned_pos[2]][numbers[2]], Quaternion.identity);
        // otherObject3 = Instantiate(otherObject3_Attributes.obj, positions[assigned_pos[3]][numbers[3]], Quaternion.identity);

        // Iterate through the Target list
        foreach (GameObject obj in Targets)
        {
            // Scale the object
    
            obj.transform.localScale = Vector3.one * objectsScaleFactor;

            // Get the Renderer component and set its material
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material1;

                // Set the color based on targetAttributes
                material1.color = colorList[targetAttributes.color];
            }
        }

        // Repeat the same process for otherObject1, otherObject2, and otherObject3
        foreach (GameObject obj in otherObject1s)
        {   
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material2;

                // Set the color based on otherObject1_Attributes
                material2.color = colorList[otherObject1_Attributes.color];
            }
        }

        foreach (GameObject obj in otherObject2s)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material3;

                // Set the color based on otherObject2_Attributes
                material3.color = colorList[targetAttributes.color];
            }
        }

        foreach (GameObject obj in otherObject3s)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material4;

                // Set the color based on otherObject3_Attributes
                material4.color = colorList[otherObject3_Attributes.color];
            }
        }
    }
        // Target.transform.localScale = Vector3.one * objectsScaleFactor;
        // otherObject1.transform.localScale = Vector3.one * objectsScaleFactor;
        // otherObject2.transform.localScale = Vector3.one * objectsScaleFactor;
        // otherObject3.transform.localScale = Vector3.one * objectsScaleFactor;

        // // get the materials for objects 
        // Renderer renderer1 = Target.GetComponent<Renderer>();
        // renderer1.material = material1;
        // Renderer renderer2 = otherObject1.GetComponent<Renderer>();
        // renderer2.material = material2;
        // Renderer renderer3 = otherObject2.GetComponent<Renderer>();
        // renderer3.material = material3;
        // Renderer renderer4 = otherObject3.GetComponent<Renderer>();
        // renderer4.material = material4;

        // // set colours
        // material1.color=targetAttributes.color;
        // material2.color=otherObject1_Attributes.color;
        // material3.color=otherObject2_Attributes.color;
        // material4.color=otherObject3_Attributes.color;

        // Debug.Log("Number is "+ determiners[0] + ", target color is " + material1.color + ", target prefab is " + Targets[0].name); // print out the name of the target_prefab, might need to save to a json file

        // move up the capsule and cylinder by the objectsScaleFactor to avoid them being stuck in the floor
        // Get all 4 objects in the scene
        // GameObject[] prefabs_ =  { Targets, otherObject1s, otherObject2s, otherObject3s };
        // foreach (GameObject obj in prefabs_)
        // {
        //     // Check if the object name starts with "Capsule" or "Cylinder"
        //     if (obj.name.StartsWith("Capsule") || obj.name.StartsWith("Cylinder"))
        //     {
        //         // Set the y coordinate to 3
        //         Vector3 newPosition = new Vector3(obj.transform.localPosition.x, objectsScaleFactor, obj.transform.localPosition.z);
        //         obj.transform.localPosition = newPosition;
        //     }
        // }

        // set the target_prefab_index and target_color_index to be output in CollectObservations
    //     for (int i=0;i<prefabList.Count;i++)
    //     {
    //         if (targetAttributes.obj==prefabList[i])
    //         {
    //             target_prefab_index=i;
    //         }
    //     }

    //     for (int i=0;i<colorList.Count;i++)
    //     {
    //         if (targetAttributes.color==colorList[i])
    //         {
    //             target_color_index=i;
    //         }        
    //     }
    // }

    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target_determiner);
        sensor.AddObservation(targetAttributes.color);
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere
        sensor.AddObservation(targetAttributes.obj);
        // 0-red,1-green,2-blue,3-yellow,4-black
        Dictionary<int, string> determiner_Dic = new Dictionary<int, string>
        {
            { 0, "A" },
            { 1, "Few (2~3)" },
            { 2, "Some (4~6)" },
            { 3, "Many (7~9)" },
            { 4, "This" },
            { 5, "That" },
            { 6, "These" },
            { 7, "Those" }
        };
        Dictionary<int, string> Color_Dic = new Dictionary<int, string>
        {
            { 0, "Red" },
            { 1, "Green" },
            { 2, "Blue" },
            { 3, "Yellow" },
            { 4, "Black" },
        };
        Dictionary<int, string> Prefab_Dic = new Dictionary<int, string>
        {
            { 0, "Capsule" },
            { 1, "Cube" },
            { 2, "Cylinder" },
            { 3, "Prism" },
            { 4, "Sphere" },
        };
        if (!hasLogged) // Check if the log hasn't been printed yet
        {
            Debug.Log(determiner_Dic[target_determiner] + ' '+ Color_Dic[targetAttributes.color] + ' ' +  Prefab_Dic[targetAttributes.obj]);
            hasLogged = true; // Set the flag to indicate that the log has been printed
        }
    }

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
        Vector3[] positions = {new Vector3(-6,0,-5),new Vector3(6,0,-5),new Vector3(-4,0,5),new Vector3(4,0,5)};
        

        Vector3 movement = rBody.transform.forward * move;
        Vector3 current_position = rBody.transform.localPosition;
        Vector3 new_position = current_position + movement;
        float distance_ref = Vector3.Distance(new_position,new Vector3(0,0,19));
        float distance_target = Vector3.Distance(new_position,positions[assigned_pos[0]]);
        float distance_object1 = Vector3.Distance(new_position,positions[assigned_pos[1]]);
        float distance_object2 = Vector3.Distance(new_position,positions[assigned_pos[2]]);
        float distance_object3 = Vector3.Distance(new_position,positions[assigned_pos[3]]);
        if (distance_target < 2)
        {
            SetReward(10f); 
            if (Targets != null)
            {
                foreach (GameObject target in Targets)
                {
                    if (target != null)
                    {
                        GameObject.Destroy(target);
                    }
                }
                Targets.Clear(); // Optionally, clear the list after destroying all objects.
            }

            if (otherObject1s != null)
            {
                foreach (GameObject obj in otherObject1s)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject1s.Clear();
            }

            if (otherObject2s != null)
            {
                foreach (GameObject obj in otherObject2s)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject2s.Clear();
            }

            if (otherObject3s != null)
            {
                foreach (GameObject obj in otherObject3s)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject3s.Clear();
            }
            EndEpisode(); // end of task    
        }

        if(distance_object1<2 || distance_object2<2 || distance_object3<2 )
        {
            SetReward(-3f);
        }
        if (distance_ref < 2) // if next action will enter the radius of ref man, then dont take action, rotation is fine
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
}

    // reward function for when agent collides with reference man, wall or target
//     void OnCollisionEnter(Collision collided_object)
//     {        
//         // get color of collided object to check for collision and give appropriate reward and endepisode accordingly
//         Renderer renderer = collided_object.gameObject.GetComponent<Renderer>();
//         Material collided_object_material = renderer.material;
//         Color collided_object_color = collided_object_material.color;
//         Debug.Log("collide object:" + collided_object.gameObject.name + "target object:" + Targets[0].name + "collide color:" + collided_object_color + "target color:" + material1.color);
//         if (collided_object_color==material1.color && collided_object.gameObject.name == Targets[0].name) // this refers to the target object specifically since only the target is of the target object color
//         {
//             SetReward(10f); 
//             if (Targets != null)
//             {
//                 foreach (GameObject target in Targets)
//                 {
//                     if (target != null)
//                     {
//                         GameObject.Destroy(target);
//                     }
//                 }
//                 Targets.Clear(); // Optionally, clear the list after destroying all objects.
//             }

//             if (otherObject1s != null)
//             {
//                 foreach (GameObject obj in otherObject1s)
//                 {
//                     if (obj != null)
//                     {
//                         GameObject.Destroy(obj);
//                     }
//                 }
//                 otherObject1s.Clear();
//             }

//             if (otherObject2s != null)
//             {
//                 foreach (GameObject obj in otherObject2s)
//                 {
//                     if (obj != null)
//                     {
//                         GameObject.Destroy(obj);
//                     }
//                 }
//                 otherObject2s.Clear();
//             }

//             if (otherObject3s != null)
//             {
//                 foreach (GameObject obj in otherObject3s)
//                 {
//                     if (obj != null)
//                     {
//                         GameObject.Destroy(obj);
//                     }
//                 }
//                 otherObject3s.Clear();
//             }
//             EndEpisode(); // end of task     
//         }

//         if (collided_object_color!=material1.color && collided_object.gameObject.name != Targets[0].name)
//         // this is for the other objects, need the second part of checking not equal wall name because the wall is also not same color as target, so the first condition alone wont suffice
//         {
//             SetReward(-3f);
//             // Debug.Log(collided_object.gameObject.name);
//         }
//     }
// }