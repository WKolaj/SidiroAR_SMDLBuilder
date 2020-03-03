using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public enum ModelSurface
{
    XY,YZ,XZ
}

public class DoorAutoDetector
{
    /// <summary>
    /// Class representing element for performing automatic door detection in switchboard elements
    /// </summary>
    /// <param name="switchboardGO">
    /// Main switchboard element
    /// </param>
    /// <param name="minFlatSurfaceRatio">
    /// minimal flat surface ratio (default 0.5f)
    /// </param>
    /// <param name="maxRaycastDistance">
    /// maximum distance of raycast (default -1 as Infinite)
    /// </param>
    public DoorAutoDetector(GameObject switchboardGO, float minFlatSurfaceRatio = 0.5f, float maxRaycastDistance = -1)
    {
        this._switchboardGO = switchboardGO;
        this._minFlatSurfaceRatio = minFlatSurfaceRatio;
        this._maxRaycastDistance = maxRaycastDistance;
    }

    /// <summary>
    /// Property determining ratio between main surface area and others 
    /// </summary>

    private float _minFlatSurfaceRatio = 0.5f;
    public float MinFlatSurfaceRatio
    {
        get
        {
            return this._minFlatSurfaceRatio;
        }
    }

    /// <summary>
    /// Property determining raycast length - -1 for infinity
    /// </summary>
    private float _maxRaycastDistance = -1;
    public float MaxRaycastDistance
    {
        get
        {
            return this._maxRaycastDistance;
        }
    }

    private GameObject _switchboardGO = null;
    public GameObject SwitchboardGO
    {
        get
        {
            return _switchboardGO;
        }

    }

    /// <summary>
    /// Method for getting all elements inside switchboard OBJ model
    /// </summary>
    /// <returns>
    /// All elements of switchboard obj model
    /// </returns>
    private List<GameObject> GetAllSwitchboardElements()
    {
        List<GameObject> listToReturn = new List<GameObject>();
        foreach(Transform childTransform in SwitchboardGO.transform)
        {
            listToReturn.Add(childTransform.gameObject);
        }

        return listToReturn;
    }

    /// <summary>
    /// Method for getting size of game object
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private Vector3 GetSizeOfGameObject(GameObject element)
    {
        var meshRendered = element.GetComponent<MeshRenderer>();

        //Returning empty vector if there is no mesh renderer
        if (meshRendered == null) return new Vector3(0, 0, 0);

        return meshRendered.bounds.size;
    }

    /// <summary>
    /// Method for gettting center point of Game object 
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private Vector3 GetCenterPointOfGameObject(GameObject element)
    {
        var meshRendered = element.GetComponent<MeshRenderer>();

        //Returning empty vector if there is no mesh renderer
        if (meshRendered == null) return new Vector3(0, 0, 0);

        return meshRendered.bounds.center;
    }

    /// <summary>
    /// Method for activating every element of switchboard - needed to perform raycasting
    /// </summary>
    private void ActivateEverySwitchboardElement()
    {
        var allElements = GetAllSwitchboardElements();

        foreach (var element in allElements)
        {
            element.SetActive(true);
        }

    }

    /// <summary>
    /// Method for adding Mesh Colliders to every element of switchboard - needed to perform raycasting
    /// </summary>
    private void AddMeshColliderToEverySwitchboardElement()
    {
        var allElements = GetAllSwitchboardElements();

        foreach(var element in allElements)
        {
            element.AddComponent<MeshCollider>();
        }

    }

    /// <summary>
    /// Method for removing Mesh Colliders from every element of switchboard - needed to perform raycasting
    /// </summary>
    private void RemoveMeshColliderFromEverySwitchboardElement()
    {
        var allElements = GetAllSwitchboardElements();

        foreach (var element in allElements)
        {
            var meshColider = element.GetComponent<MeshCollider>();
            GameObject.DestroyImmediate(meshColider);
        }

    }

    /// <summary>
    /// Method for finding surface type of given element
    /// </summary>
    /// <param name="element">
    /// Game object to check
    /// </param>
    /// <returns>
    /// Surface of given game object
    /// </returns>
    private ModelSurface GetMainSurfaceOfGameObject(GameObject element)
    {
        Vector3 elementSize = this.GetSizeOfGameObject(element);

        float x = elementSize.x;
        float y = elementSize.y;
        float z = elementSize.z;

        bool xSizeIsSmallest = false;
        bool ySizeIsSmallest = false;
        bool zSizeIsSmallest = false;
        
        //Finding the smallest size
        if (z <= x && z <= y) zSizeIsSmallest = true;
        if (x <= z && x <= y) xSizeIsSmallest = true;
        if (y <= z && y <= x) ySizeIsSmallest = true;

        if (xSizeIsSmallest) return ModelSurface.YZ;
        else if (ySizeIsSmallest) return ModelSurface.XZ;
        else if (zSizeIsSmallest) return ModelSurface.XY;

        //Will never happen but method has to return something
        return ModelSurface.XY;
    }

    /// <summary>
    /// Method for checking if element can be assigned as flat - checking surface area ratio
    /// </summary>
    /// <param name="element"></param>
    /// <returns></returns>
    private bool CheckIfElementIsFlat(GameObject element)
    {
        Vector3 size = GetSizeOfGameObject(element);
        ModelSurface mainSurface = GetMainSurfaceOfGameObject(element);

        float mainSurfaceArea = 0.0f;
        float auxSurfaceArea0 = 0.0f;
        float auxSurfaceArea1 = 0.0f;

        if (mainSurface == ModelSurface.XY)
        {
            mainSurfaceArea = Math.Abs(size.x * size.y);
            auxSurfaceArea0 = Math.Abs(size.z * size.y);
            auxSurfaceArea1 = Math.Abs(size.z * size.x);
        }
        else if (mainSurface == ModelSurface.XZ)
        {
            mainSurfaceArea = Math.Abs(size.x * size.z);
            auxSurfaceArea0 = Math.Abs(size.y * size.z);
            auxSurfaceArea1 = Math.Abs(size.y * size.x);
        }
        else if (mainSurface == ModelSurface.YZ)
        {
            mainSurfaceArea = Math.Abs(size.y * size.z);
            auxSurfaceArea0 = Math.Abs(size.x * size.y);
            auxSurfaceArea1 = Math.Abs(size.x * size.z);
        }
        else return false;

        //If main surface area is 0 return false;
        if (mainSurfaceArea == 0) return false;

        //Checking first aux surface area
        if (auxSurfaceArea0 / mainSurfaceArea > MinFlatSurfaceRatio) return false;

        //Checking second aux surface area
        if (auxSurfaceArea1 / mainSurfaceArea > MinFlatSurfaceRatio) return false;

        //Main surface is big enough - element is flat
        return true;
    }

    /// <summary>
    /// Method for checking raycast from main surface of element and center point - in order to check whether element is inside switchboard or not
    /// </summary>
    /// <param name="element">
    /// Element to check
    /// </param>
    /// <returns>
    /// Array of raycast results - if one of them is false than element is outside the switchboard
    /// </returns>
    private bool[] CheckElementsRaycast(GameObject element)
    {
        ModelSurface mainSurface = GetMainSurfaceOfGameObject(element);
        Vector3 elementCenterPoint = GetCenterPointOfGameObject(element);
        Vector3 elementSize = GetSizeOfGameObject(element);

        //Two points of raycasting - raycast should be performed from center point, moved to the external surface of element
        //For every direction there is different external surface
        Vector3 raycastPoint0 = new Vector3(0, 0, 0);
        Vector3 raycastPoint1 = new Vector3(0, 0, 0);

        //Two directions of raycasting
        Vector3 raycastDirection0 = new Vector3(0, 0, 1);
        Vector3 raycastDirection1 = new Vector3(0, 0, -1);

        switch (mainSurface)
        {
            case ModelSurface.XY:
                {
                    raycastDirection0 = new Vector3(0, 0, 1);
                    raycastDirection1 = new Vector3(0, 0, -1);
                    raycastPoint0 = elementCenterPoint + new Vector3(0, 0, elementSize.z/2);
                    raycastPoint1 = elementCenterPoint + new Vector3(0, 0, -elementSize.z / 2);
                    break;
                }

            case ModelSurface.XZ:
                {
                    raycastDirection0 = new Vector3(0, 1, 0);
                    raycastDirection1 = new Vector3(0, -1, 0);
                    raycastPoint0 = elementCenterPoint + new Vector3(0, elementSize.y/2, 0);
                    raycastPoint1 = elementCenterPoint + new Vector3(0, -elementSize.y / 2, 0);
                    break;
                }

            case ModelSurface.YZ:
                {
                    raycastDirection0 = new Vector3(1, 0, 0);
                    raycastDirection1 = new Vector3(-1, 0, 0);
                    raycastPoint0 = elementCenterPoint + new Vector3(elementSize.x/2, 0, 0);
                    raycastPoint1 = elementCenterPoint + new Vector3(-elementSize.x / 2, 0, 0);
                    break;
                }
        }

        //Raycasting depening on raycast length set
        var raycast0Result = false;
        var raycast1Result = false;

        if (MaxRaycastDistance < 0)
        {
            raycast0Result = Physics.Raycast(raycastPoint0, raycastDirection0);
            raycast1Result = Physics.Raycast(raycastPoint1, raycastDirection1);
        }
        else
        {
            raycast0Result = Physics.Raycast(raycastPoint0, raycastDirection0, MaxRaycastDistance);
            raycast1Result = Physics.Raycast(raycastPoint1, raycastDirection1, MaxRaycastDistance);
        }

        return new bool[] { raycast0Result, raycast1Result };
    }

    /// <summary>
    /// Main method for perfoming auto detection of doors
    /// </summary>
    public void DetectAndRenameDoorElementsInSwitchboard()
    {
        var doorElements = new List<GameObject>();

        //All switchboard elements have to be active in order to perform raycasting
        ActivateEverySwitchboardElement();

        //Mesh colliders has to be added in order to perform raycasting
        AddMeshColliderToEverySwitchboardElement();

        //For all elements checking if element is flat and if it is - checking its raycast (is element inside or outside switchboard)
        var allElements = GetAllSwitchboardElements();

        foreach (var element in allElements)
        {
            //Element cannot be flat
            if (!CheckIfElementIsFlat(element)) continue;

            //At least one of raycast results has to be false (ray did not meet any element)
            var raycastResult = CheckElementsRaycast(element);
            if (raycastResult[0] && raycastResult[1]) continue;

            doorElements.Add(element);
        }

        //Renaming all doors elements
        for(int i=1; i<=doorElements.Count; i++)
        {
            doorElements[i-1].name = String.Format("Door{0}", i);
        }

        //Mesh colliders has removed after detection has been finished
        RemoveMeshColliderFromEverySwitchboardElement();
    }

    private void Awake()
    {
        DetectAndRenameDoorElementsInSwitchboard();
    }


}
