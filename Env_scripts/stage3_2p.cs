using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Random = UnityEngine.Random;

public class stage3_env2_train_5c5s2p : Agent
{
    private List<GameObject> Targets_1;
    private List<GameObject> Targets_2;
    private List<GameObject> otherObject1s_1;
    private List<GameObject> otherObject1s_2;
    private List<GameObject> otherObject2s_1;
    private List<GameObject> otherObject2s_2;
    private List<GameObject> otherObject3s_1;
    private List<GameObject> otherObject3s_2;
    private Rigidbody rBody;
    private List<Vector3> allList;
    private List<Vector3> left;
    private List<Vector3> right;
    private List<Vector3> middle_1;
    private List<Vector3> up;
    private List<Vector3> down;
    private List<Vector3> middle_2;
    private List<Vector3> front;
    private List<Vector3> middle_3;
    private List<Vector3> back;
    private List<GameObject> prefabList;
    private List<Color> colorList;
    private List<int> prepositions;
    private List<int> assigned_pos;
    private List<AttributeTuple> AttributesList;
    public Material material1_1;
    public Material material1_2;
    public Material material2_1;
    public Material material2_2;
    public Material material3_1;
    public Material material3_2;
    public Material material4_1;
    public Material material4_2;
    public float objectsScaleFactor=1.0f;
    public float leftRightLimits=14f;
    public float frontBackLimits=18f;
    public float bounceBackDistance=2f;
    private int targetAttributesIndex_1;
    public AttributeTuple targetAttributes_1;
    public AttributeTuple targetAttributes_2;
    private int target_preposition;
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
    
        // create train and test attributesList
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
        left = new List<Vector3>();
        middle_1 = new List<Vector3>();
        right = new List<Vector3>();
        up = new List<Vector3>();
        middle_2 = new List<Vector3>();
        down = new List<Vector3>();
        front = new List<Vector3>();
        middle_3 = new List<Vector3>();
        back = new List<Vector3>();

        for (int i = -1; i <= 1; i++)
        {
            for (int j = 1; j <= 3; j++)
            {
                for (int k = -1; k <= 1; k++)
                {
                    allList.Add(new Vector3(i, j, k));
                }
            }
        }
        left = allList.GetRange(0, 9);
        middle_1 = allList.GetRange(9, 9);
        right = allList.GetRange(18, 9);

        
        foreach (Vector3 element in allList)
        {
            if (element.y == 3)
            {
                up.Add(element);
            }
            if (element.y == 2)
            {
                middle_2.Add(element);
            }
            if (element.y == 1)
            {
                down.Add(element);
            }
        }

        foreach (Vector3 element in allList)
        {
            if (element.z == 1)
            {
                back.Add(element);
            }
            if (element.z == 0)
            {
                middle_3.Add(element);
            }
            if (element.z == -1)
            {
                front.Add(element);
            }
        }
        
        }
    // static Random rand = new Random();

    // Define a method to select a random element from a list
    public static Vector3 choose(List<Vector3> list)
    {   
        System.Random rand = new System.Random();
        int randomIndex = rand.Next(list.Count); // Generate a random index within the range of valid indices
        return list[randomIndex]; // Get the random element using the random index
    }
    public List<List<Vector3>> Position(int pos, int pre)
    {
        List<Vector3> xyzList_1 = new List<Vector3>();
        List<Vector3> xyzList_2 = new List<Vector3>();
        List<List<Vector3>> xyzList = new List<List<Vector3>>();
        System.Random rand = new System.Random();
        List<int> randomIndices = new List<int>();
        // 0 -- Above, 1 -- Below, 2 -- In front of, 3 -- Behind, 4 -- Beside, 5 -- On, 6 -- Between, 7 -- Among
        
        if (pre == 0) // Above
        {   
            Vector3 element_1 = choose(up);
            Vector3 element_2 = choose(down);
            while (Math.Abs(element_2.x - element_1.x) > 1 || Math.Abs(element_2.z - element_1.z) > 1)
            {
                element_2 = choose(down);
            }
            xyzList_1.Add(element_1);
            xyzList_2.Add(element_2);
        }
        else if (pre == 1) // On
        {
            Vector3 element_1 = choose(up.Concat(middle_2).ToList());
            Vector3 temp = element_1;
            temp.y -= 1;
            Vector3 element_2 = temp;
            xyzList_1.Add(element_1);
            xyzList_2.Add(element_2);
        }
        else if (pre == 2) // Between
        {
            Vector3 element_1 = choose(middle_1);
            Vector3 temp = element_1;
            temp.x += 1;
            Vector3 element_2 = temp;
            temp.x -= 2;
            Vector3 element_3 = temp;
            xyzList_1.Add(element_1);
            xyzList_2.Add(element_2);
            xyzList_2.Add(element_3);
        }
        else if (pre == 3) // Among
        {
            Vector3 element_1 = choose(middle_2);
            int num = Random.Range(3,5);
            List<int> randomIndices_1 = new List<int>();
            List<int> randomIndices_2 = new List<int>();

            while (randomIndices_1.Count < num)
            {
                int randomIndex = rand.Next(up.Count);

                if (!randomIndices_1.Contains(randomIndex))
                {
                    randomIndices_1.Add(randomIndex);
                }
            }

            while (randomIndices_2.Count < num)
            {
                int randomIndex = rand.Next(down.Count);

                if (!randomIndices_2.Contains(randomIndex))
                {
                    randomIndices_2.Add(randomIndex);
                }
            }

            xyzList_1.Add(element_1);
            foreach (int i in randomIndices_1)
            {
                xyzList_2.Add(up[i]);
            }
            foreach (int i in randomIndices_2)
            {
                xyzList_2.Add(down[i]);
            }
        }
                
        

        for (int j = 0; j < xyzList_1.Count; j++)
        {
            Vector3 modifiedElement = xyzList_1[j]; // Create a copy of the element

            if (pos == 0) modifiedElement.x -= 5.5f;
            if (pos == 1) modifiedElement.x += 5.5f;
            if (pos == 2) modifiedElement.x -= 3;
            if (pos == 3) modifiedElement.x += 3;
            modifiedElement.z += (pos < 2 ? -5 : 5);  //[-6,0,-5],[6,0,-5],[-4,0,5],[4,0,5]
            //  2   3
            //0       1
            
            xyzList_1[j] = modifiedElement; // Add the modified element to the new list
        }

        for (int j = 0; j < xyzList_2.Count; j++)
        {
            Vector3 modifiedElement = xyzList_2[j]; // Create a copy of the element

            if (pos == 0) modifiedElement.x -= 5.5f;
            if (pos == 1) modifiedElement.x += 5.5f;
            if (pos == 2) modifiedElement.x -= 3;
            if (pos == 3) modifiedElement.x += 3;
            modifiedElement.z += (pos < 2 ? -5 : 5);  //[-6,0,-5],[6,0,-5],[-4,0,5],[4,0,5]
            //  2   3
            //0       1
            
            xyzList_2[j] = modifiedElement; // Add the modified element to the new list
        }

        xyzList.Add(xyzList_1);
        xyzList.Add(xyzList_2);
        return xyzList;
    }
  
    public override void OnEpisodeBegin()
    {   
        hasLogged = false;
        // Destroy the old objects. This is needed because objects wont be destroyed at the end if max steps is reached. So destroy them first at the start of new Episode
        if (Targets_1 != null)
        {
            foreach (GameObject target in Targets_1)
            {
                if (target != null)
                {
                    Destroy(target);
                }
            }
            Targets_1.Clear(); // Optionally, clear the list after destroying all objects.
        }

        if (Targets_2 != null)
        {
            foreach (GameObject target in Targets_2)
            {
                if (target != null)
                {
                    Destroy(target);
                }
            }
            Targets_2.Clear(); // Optionally, clear the list after destroying all objects.
        }

        if (otherObject1s_1 != null)
        {
            foreach (GameObject obj in otherObject1s_1)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject1s_1.Clear();
        }

        if (otherObject1s_2 != null)
        {
            foreach (GameObject obj in otherObject1s_2)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject1s_2.Clear();
        }

        if (otherObject2s_1 != null)
        {
            foreach (GameObject obj in otherObject2s_1)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject2s_1.Clear();
        }

        if (otherObject2s_2 != null)
        {
            foreach (GameObject obj in otherObject2s_2)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject2s_2.Clear();
        }

        if (otherObject3s_1 != null)
        {
            foreach (GameObject obj in otherObject3s_1)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject3s_1.Clear();
        }

        if (otherObject3s_2 != null)
        {
            foreach (GameObject obj in otherObject3s_2)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            otherObject3s_2.Clear();
        }

        // reset agent position and rotation
        this.transform.localPosition = new Vector3(0,1,-18); // move the agent back to its original location
        this.transform.localRotation = Quaternion.identity; // make sure the rotation of the agent is also reset at the beginning of each episode

        // select AttributeTuple for target
        targetAttributesIndex_1 = Random.Range(0,AttributesList.Count);

        // // select AttributeTuple for other objects
        int targetAttributesIndex_2 = Random.Range(0,AttributesList.Count);
        while (targetAttributesIndex_1==targetAttributesIndex_2)
        {
            targetAttributesIndex_2 = Random.Range(0,AttributesList.Count);
        }
        int otherObject1_AttributesIndex_1 = Random.Range(0,AttributesList.Count);
        while (otherObject1_AttributesIndex_1==targetAttributesIndex_1)
        {
            otherObject1_AttributesIndex_1 = Random.Range(0,AttributesList.Count);
        }
        int otherObject1_AttributesIndex_2 = Random.Range(0,AttributesList.Count);
        while (otherObject1_AttributesIndex_2==targetAttributesIndex_2 || otherObject1_AttributesIndex_2 == otherObject1_AttributesIndex_1)
        {
            otherObject1_AttributesIndex_2 = Random.Range(0,AttributesList.Count);
        }
        int otherObject3_AttributesIndex_1 = Random.Range(0,AttributesList.Count);
        while (otherObject3_AttributesIndex_1==targetAttributesIndex_1)
        {
            otherObject3_AttributesIndex_1 = Random.Range(0,AttributesList.Count);
        }
        int otherObject3_AttributesIndex_2 = Random.Range(0,AttributesList.Count);
        while (otherObject3_AttributesIndex_2==targetAttributesIndex_2 || otherObject3_AttributesIndex_2 == otherObject3_AttributesIndex_1)
        {
            otherObject3_AttributesIndex_2 = Random.Range(0,AttributesList.Count);
        }

        // call the attributetuple for all 4 objects
        targetAttributes_1 = AttributesList[targetAttributesIndex_1];
        targetAttributes_2 = AttributesList[targetAttributesIndex_2];
        AttributeTuple otherObject1_Attributes_1 = AttributesList[otherObject1_AttributesIndex_1];
        AttributeTuple otherObject1_Attributes_2 = AttributesList[otherObject1_AttributesIndex_2];
        AttributeTuple otherObject3_Attributes_1 = AttributesList[otherObject3_AttributesIndex_1];
        AttributeTuple otherObject3_Attributes_2 = AttributesList[otherObject3_AttributesIndex_2];

        // list down the positions 
        // prepositions = Enumerable.Range(0, 8).OrderBy(x => Guid.NewGuid()).ToList();
        // 0 -- Above, 1 -- Below, 2 -- In front of, 3 -- Behind, 4 -- Beside, 5 -- On, 6 -- Between, 7 -- Among
        System.Random rand = new System.Random();
        target_preposition = Random.Range(0,2)+2;

        while(targetAttributes_1.obj + 1 == target_preposition)
        {
            target_preposition = Random.Range(0,2)+2;
        }
        int otherObject2_preposition = Random.Range(0,4);
        while(targetAttributes_1.obj + 1 == otherObject2_preposition || target_preposition == otherObject2_preposition)
        {
            otherObject2_preposition = Random.Range(0,4);
        }
        int otherObject3_preposition = Random.Range(0,4);
        while(otherObject3_Attributes_1.obj + 1 == otherObject3_preposition || target_preposition == otherObject3_preposition || otherObject2_preposition == otherObject3_preposition)
        {
            otherObject3_preposition = Random.Range(0,4);
        }
        assigned_pos = new List<int> { 0, 1, 2, 3 };
        assigned_pos = assigned_pos.OrderBy(x => rand.Next()).ToList();
        
        //--------------TARGET---------------------
        List<List<Vector3>> targetPositions = Position(assigned_pos[0],target_preposition);
        Targets_1 = new List<GameObject>();
        Targets_2 = new List<GameObject>();
        foreach (Vector3 position in targetPositions[0])
        {
            GameObject target = Instantiate(prefabList[targetAttributes_1.obj], position, Quaternion.identity);
            Targets_1.Add(target);
        }
        foreach (Vector3 position in targetPositions[1])
        {
            GameObject target = Instantiate(prefabList[targetAttributes_2.obj], position, Quaternion.identity);
            Targets_2.Add(target);
        }

        //--------------OBJECT1---------------------  Preposition is the same
        List<List<Vector3>> otherObject1Positions = Position(assigned_pos[1],target_preposition);
        otherObject1s_1 = new List<GameObject>();
        otherObject1s_2 = new List<GameObject>();
        foreach (Vector3 position in otherObject1Positions[0])
        {
            GameObject otherObject1 = Instantiate(prefabList[otherObject1_Attributes_1.obj], position, Quaternion.identity);
            otherObject1s_1.Add(otherObject1);
        }
        foreach (Vector3 position in otherObject1Positions[1])
        {
            GameObject otherObject1 = Instantiate(prefabList[otherObject1_Attributes_2.obj], position, Quaternion.identity);
            otherObject1s_2.Add(otherObject1);
        }

        //--------------OBJECT2---------------------  C&S is the same

        List<List<Vector3>> otherObject2Positions = Position(assigned_pos[2],otherObject2_preposition);
        otherObject2s_1 = new List<GameObject>();
        otherObject2s_2 = new List<GameObject>();
        foreach (Vector3 position in otherObject2Positions[0])
        {
            GameObject otherObject2 = Instantiate(prefabList[targetAttributes_1.obj], position, Quaternion.identity);
            otherObject2s_1.Add(otherObject2);
        }
        foreach (Vector3 position in otherObject2Positions[1])
        {
            GameObject otherObject2 = Instantiate(prefabList[targetAttributes_2.obj], position, Quaternion.identity);
            otherObject2s_2.Add(otherObject2);
        }


        //--------------OBJECT3---------------------
        List<List<Vector3>> otherObject3Positions = Position(assigned_pos[3],otherObject3_preposition);
        otherObject3s_1 = new List<GameObject>();
        otherObject3s_2 = new List<GameObject>();
        foreach (Vector3 position in otherObject3Positions[0])
        {
            GameObject otherObject3 = Instantiate(prefabList[otherObject3_Attributes_1.obj], position, Quaternion.identity);
            otherObject3s_1.Add(otherObject3);
        }
        foreach (Vector3 position in otherObject3Positions[1])
        {
            GameObject otherObject3 = Instantiate(prefabList[otherObject3_Attributes_2.obj], position, Quaternion.identity);
            otherObject3s_2.Add(otherObject3);
        }

        // Iterate through the Target list
        foreach (GameObject obj in Targets_1)
        {
            // Scale the object
    
            obj.transform.localScale = Vector3.one * objectsScaleFactor;

            // Get the Renderer component and set its material
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material1_1;

                // Set the color based on targetAttributes
                material1_1.color = colorList[targetAttributes_1.color];
            }
        }

        foreach (GameObject obj in Targets_2)
        {
            // Scale the object
    
            obj.transform.localScale = Vector3.one * objectsScaleFactor;

            // Get the Renderer component and set its material
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material1_2;

                // Set the color based on targetAttributes
                material1_2.color = colorList[targetAttributes_2.color];
            }
        }

        // Repeat the same process for otherObject1, otherObject2, and otherObject3
        foreach (GameObject obj in otherObject1s_1)
        {   
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material2_1;

                // Set the color based on otherObject1_Attributes
                material2_1.color = colorList[otherObject1_Attributes_1.color];
            }
        }

        foreach (GameObject obj in otherObject1s_2)
        {   
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material2_2;

                // Set the color based on otherObject1_Attributes
                material2_2.color = colorList[otherObject1_Attributes_2.color];
            }
        }

        foreach (GameObject obj in otherObject2s_1)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material3_1;

                // Set the color based on otherObject2_Attributes
                material3_1.color = colorList[targetAttributes_1.color];
            }
        }

        foreach (GameObject obj in otherObject2s_2)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material3_2;

                // Set the color based on otherObject2_Attributes
                material3_2.color = colorList[targetAttributes_2.color];
            }
        }


        foreach (GameObject obj in otherObject3s_1)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material4_1;

                // Set the color based on otherObject3_Attributes
                material4_1.color = colorList[otherObject3_Attributes_1.color];
            }
        }

        foreach (GameObject obj in otherObject3s_2)
        {
            
            obj.transform.localScale = Vector3.one * objectsScaleFactor;
            Renderer renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material = material4_2;

                // Set the color based on otherObject3_Attributes
                material4_2.color = colorList[otherObject3_Attributes_2.color];
            }
        }
        
        }


        // Debug.Log("Number is "+ determiners[0] + ", target color is " + material1.color + ", target prefab is " + Targets[0].name); // print out the name of the target_prefab, might need to save to a json file
    
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(targetAttributes_1.color);
        // 0-capsule,1-cube,2-cylinder,3-prism,4-sphere
        sensor.AddObservation(targetAttributes_1.obj);
        // 0-red,1-green,2-blue,3-yellow,4-black
        sensor.AddObservation(target_preposition);
        sensor.AddObservation(targetAttributes_2.color);
        sensor.AddObservation(targetAttributes_2.obj);
        Dictionary<int, string> preposition_Dic = new Dictionary<int, string>
        {
            { 0, "Above" },
            { 1, "On" },
            { 2, "Between" },
            { 3, "Among" }
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
            Debug.Log(Color_Dic[targetAttributes_1.color] + ' ' +  Prefab_Dic[targetAttributes_1.obj] + ' '+ preposition_Dic[target_preposition] + ' '+ Color_Dic[targetAttributes_2.color] + ' ' +  Prefab_Dic[targetAttributes_2.obj]);
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
            if (Targets_1 != null)
            {
                foreach (GameObject target in Targets_1)
                {
                    if (target != null)
                    {
                        GameObject.Destroy(target);
                    }
                }
                Targets_1.Clear(); // Optionally, clear the list after destroying all objects.
            }

            if (Targets_2 != null)
            {
                foreach (GameObject target in Targets_2)
                {
                    if (target != null)
                    {
                        GameObject.Destroy(target);
                    }
                }
                Targets_2.Clear(); // Optionally, clear the list after destroying all objects.
            }

            if (otherObject1s_1 != null)
            {
                foreach (GameObject obj in otherObject1s_1)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject1s_1.Clear();
            }

            if (otherObject1s_2 != null)
            {
                foreach (GameObject obj in otherObject1s_2)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject1s_2.Clear();
            }

            if (otherObject2s_1 != null)
            {
                foreach (GameObject obj in otherObject2s_1)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject2s_1.Clear();
            }

            if (otherObject2s_2 != null)
            {
                foreach (GameObject obj in otherObject2s_2)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject2s_2.Clear();
            }

            if (otherObject3s_1 != null)
            {
                foreach (GameObject obj in otherObject3s_1)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject3s_1.Clear();
            }

            if (otherObject3s_2 != null)
            {
                foreach (GameObject obj in otherObject3s_2)
                {
                    if (obj != null)
                    {
                        GameObject.Destroy(obj);
                    }
                }
                otherObject3s_2.Clear();
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
