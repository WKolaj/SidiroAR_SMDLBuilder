using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

public class SMDLModelCreator : EditorWindow
{
    #region MAIN_METHOD

    /// <summary>
    /// Main method for creating SMDL file
    /// </summary>
    [MenuItem("SidiroAR/Create SMDL file...")]
    static void CreateSMDLFile()
    {
        var editorWindow = EditorWindow.CreateInstance<SMDLModelCreator>();
        editorWindow.position = new Rect(Screen.width / 2, Screen.height / 2, 250, 150);

        try
        {
            //Creation starts with showing select obj file
            var objSelected = editorWindow.SelectOBJFile();

            //Returning imediatelly if file not selected
            if (!objSelected) return;
        }
        catch (Exception err)
        {
            Debug.Log(err);
            EditorUtility.DisplayDialog("Error occured", "Error while creating SMDL file! Details are displayed in console", "OK");
            //Returning imidiatelly
            return;
        }

        //Now we can show a window to select properties such as a scale etc.
        editorWindow.ShowModalUtility();

    }

    #endregion MAIN_METHOD

    #region CONSTANTS

    const string assetsDirName = "Assets";
    const string resourcesDirName = "Resources";
    const string prefabsDirName = "Prefabs";
    const string assetBundleDirName = "AssetBundles";
    const string modelContainerName = "_ModelContainer_";

    #endregion CONSTANTS

    #region SCRIPTABLE_VALUES

    //Elements associated with scale
    private float Scale
    {
        get
        {
            return this.scaleValues[this.scaleIndex];
        }
    }
    private string[] scaleLabels = new string[] { "m", "dm", "cm", "mm" };
    private float[] scaleValues = new float[] { 1f, 0.1f, 0.01f, 0.001f };
    private int scaleIndex = 3;

    #endregion SCRIPTABLE_VALUES

    #region PROCEDURE_VALUES

    private string _externalObjFilePath;
    private string ExternalOBJFilePath
    {
        get
        {
            return _externalObjFilePath;
        }
    }

    private string ExternalMTLFilePath
    {
        get
        {
            return ExternalOBJFilePath.Replace(".obj", ".mtl");
        }
    }

    private string ModelName
    {
        get
        {
            return Path.GetFileNameWithoutExtension(ExternalOBJFilePath);
        }
    }

    private string InteralModelOBJPath
    {
        get
        {
            return Path.Combine(ResourcesDirPath, ModelName + ".obj");
        }
    }

    private string InteralModelMTLPath
    {
        get
        {
            return Path.Combine(ResourcesDirPath, ModelName + ".mtl");
        }
    }

    private string _temporaryModelName;
    private string TemporaryModelName
    {
        get
        {
            return _temporaryModelName;
        }
    }

    private string ModelsPrefabPath
    {
        get
        {
            return Path.Combine(PrefabsDirPath, TemporaryModelName + ".prefab");
        }
    }

    private string ModelsAssetBundleFilePath
    {
        get
        {
            return Path.Combine(AssetBundlesDirPath, TemporaryModelName + ".smdl");
        }
    }

    private GameObject _modelContainer;
    private GameObject ModelContainer
    {
        get
        {
            return _modelContainer;
        }
    }

    private GameObject _modelsAsset;
    private GameObject ModelsAsset
    {
        get
        {
            return _modelsAsset;
        }
    }

    private GameObject _modelsPrefab;
    private GameObject ModelsPrefab
    {
        get
        {
            return _modelsPrefab;
        }
    }
    #endregion PROCEDURE_VALUES

    #region UTILITIES

    public static void ClearDirIfExistsOrCreateItIfNot(string dirPath)
    {
        if (Directory.Exists(dirPath))
        {
            ClearDirectory(dirPath);
        }
        else
        {
            Directory.CreateDirectory(dirPath);
        }
    }

    public static void ClearDirectory(string dirPath)
    {
        DirectoryInfo directory = new DirectoryInfo(dirPath);

        //delete files:
        foreach (FileInfo file in directory.GetFiles())
            file.Delete();

        //delete directories in this directory:
        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            directory.Delete(true);
    }

    public static void ClearDirectoryIfExists(string dirPath)
    {
        //Returning immediately if dicrectory doesn't exist
        if (!Directory.Exists(dirPath)) return;

        DirectoryInfo directory = new DirectoryInfo(dirPath);

        //delete files:
        foreach (FileInfo file in directory.GetFiles())
            file.Delete();

        //delete directories in this directory:
        foreach (DirectoryInfo subDirectory in directory.GetDirectories())
            directory.Delete(true);
    }
    #endregion UTILITIES

    #region DIRECTORIES

    private static string AssetBundlesDirPath
    {
        get
        {
            return Path.Combine(assetsDirName, assetBundleDirName);
        }
    }

    private static string PrefabsDirPath
    {
        get
        {
            return Path.Combine(assetsDirName, prefabsDirName);
        }
    }

    private static string ResourcesDirPath
    {
        get
        {
            return Path.Combine(assetsDirName, resourcesDirName);
        }
    }



    #endregion

    #region GUI_METHODS

    /// <summary>
    /// Method for generatring UI
    /// </summary>
    private void OnGUI()
    {
        EditorGUILayout.LabelField("Models unit");
        scaleIndex = EditorGUILayout.Popup(scaleIndex, scaleLabels);

        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();

        if (GUILayout.Button("Create")) this.onCreateClicked();
        if (GUILayout.Button("Cancel")) this.onCancelClicked();

        GUILayout.EndHorizontal();

    }

    /// <summary>
    /// Method called on cancel button clicked
    /// </summary>
    void onCancelClicked()
    {
        this.Close();
    }

    /// <summary>
    /// Method called on create button clicked
    /// </summary>
    void onCreateClicked()
    {
        StartCreatingSMDLProcedure();
        this.Close();
    }

    #endregion GUI_METHODS

    #region CREATE_SMDL_PROCEDURE

    /// <summary>
    /// Main method for creating smdl;
    /// </summary>
    void StartCreatingSMDLProcedure()
    {
        try
        {
            //Initializing SMDL procedure - creating or clearing directories
            Debug.Log("Initialzing smdl creation procedure...");
            InitSMDLCreationFileProcedure();

            //Generating random model name (for asset, prefab and assetBundle)
            Debug.Log("Generating temporary file name...");
            GenerateTemporaryFileName();

            //Creating new model container
            Debug.Log("Creating model container...");
            CreateModelContainer();

            //Import external model files to unity
            Debug.Log("Import external model files to Unity project");
            ImportExternalModelFiles();

            //Setting properties of imported model such as scale, write/read enable etc.
            Debug.Log("Setting imported model properties");
            SetImportedModelProperties();

            //Creating asset in project from imported model
            Debug.Log("Setting imported model properties");
            CreateAssetFromImportedModel();

            //Creating prefab from asset and setting its asset bundle name
            Debug.Log("Creating prefab from asset and setting its asset bundle name");
            CreatePrefabFromModelsAsset();

            //Generating asset bundles
            Debug.Log("Creating asset bundles");
            CreateAssetBundles();

            //Exporting asset bundle file to external dir
            Debug.Log("Exporting asset bundle file...");
            ExportAssetBundleFile();

            //Refreshing assets database
            Debug.Log("Refreshing assets database..");
            AssetDatabase.Refresh();

            Debug.Log("Cleaning up..");
            Debug.Log("Operation ended successfully..");

            //Clearing all data if exist
            Debug.Log("Cleaning up directories...");
            ClearAllDirectories();

            //Deleting model container if exists
            Debug.Log("Cleaning up model container...");
            DeleteModelContainer();

            EditorUtility.DisplayDialog("SMDL file creation", "SMDL file created successfully", "OK");
        }
        catch (Exception error)
        {
            Debug.Log(error);
            EditorUtility.DisplayDialog("Error occured", "Error while creating SMDL file! Details are displayed in console", "OK");

            //Cleaning up in case of error

            //Clearing all data if exist
            ClearAllDirectories();

            //Deleting model container if exists
            DeleteModelContainer();
        }
    }

    /// <summary>
    /// Method for initialzing procedure of creating SMDL file
    /// </summary>
    private void InitSMDLCreationFileProcedure()
    {

        //Checking if directories exists
        //a) Clearing them if exists
        //b) Creating them if not
        ClearDirIfExistsOrCreateItIfNot(AssetBundlesDirPath);
        ClearDirIfExistsOrCreateItIfNot(PrefabsDirPath);
        ClearDirIfExistsOrCreateItIfNot(ResourcesDirPath);

        //Removing model container if exists
        DeleteModelContainer();

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    private void ClearAllDirectories()
    {
        ClearDirectoryIfExists(AssetBundlesDirPath);
        ClearDirectoryIfExists(PrefabsDirPath);
        ClearDirectoryIfExists(ResourcesDirPath);

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    private void DeleteModelContainer()
    {
        var modelContainerGO = GameObject.Find(modelContainerName);
        if (modelContainerGO != null) GameObject.DestroyImmediate(modelContainerGO);
    }

    /// <summary>
    /// Method for selecting new obj file
    /// </summary>
    private bool SelectOBJFile()
    {
        //Selecting element
        this._externalObjFilePath = EditorUtility.OpenFilePanel("Load OBJ model file", "", "obj");

        //Returning false if file has not been selected
        if (this._externalObjFilePath.Length == 0) return false;
        //Throwing if mtl file does not exist
        if (!File.Exists(ExternalMTLFilePath)) throw new InvalidDataException("MTL file not found for given model ");

        //Returning true if everything is ok
        return true;
    }

    /// <summary>
    /// Method for setting random name for model in project
    /// </summary>
    private void GenerateTemporaryFileName()
    {
        this._temporaryModelName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    }

    /// <summary>
    /// Method for coping external models to unity project
    /// </summary>
    private void ImportExternalModelFiles()
    {
        //Coping model to resources
        File.Copy(ExternalOBJFilePath, InteralModelOBJPath, true);
        File.Copy(ExternalMTLFilePath, InteralModelMTLPath, true);

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    /// <summary>
    /// Method for setting internal model properties - such as scale, read/write enable etc.
    /// </summary>
    private void SetImportedModelProperties()
    {
        //Setting model properties
        var modelImporter = AssetImporter.GetAtPath(InteralModelOBJPath) as ModelImporter;
        modelImporter.globalScale = Scale;
        modelImporter.isReadable = true;
        modelImporter.SaveAndReimport();
    }

    /// <summary>
    /// Method for creating GameObject to store models prefab
    /// </summary>
    private void CreateModelContainer()
    {
        this._modelContainer = new GameObject(modelContainerName);
    }

    /// <summary>
    /// Method for creating Asset in project based on imported model
    /// </summary>
    private void CreateAssetFromImportedModel()
    {
        var loadedObject = (GameObject)Resources.Load(ModelName, typeof(GameObject));
        var createdObject = GameObject.Instantiate(loadedObject, ModelContainer.transform);
        createdObject.name = TemporaryModelName;

        _modelsAsset = createdObject;
    }

    /// <summary>
    /// Method for creating Prefab based on models asset in project
    /// </summary>
    private void CreatePrefabFromModelsAsset()
    {
        //Creating asset prefab
        this._modelsPrefab = PrefabUtility.SaveAsPrefabAsset(ModelsAsset, ModelsPrefabPath);

        //Setting name for created prefab
        AssetImporter.GetAtPath(ModelsPrefabPath).SetAssetBundleNameAndVariant(TemporaryModelName, "smdl");

    }

    /// <summary>
    /// Method for creating asset bundles from project
    /// </summary>
    private void CreateAssetBundles()
    {
        BuildPipeline.BuildAssetBundles(AssetBundlesDirPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

    private void ExportAssetBundleFile()
    {
        //Saving asset bundle file to external dir
        string assetBundleExportFilePath = EditorUtility.SaveFilePanel("Save SMDL to file", "", ModelName, "smdl");
        File.Copy(ModelsAssetBundleFilePath, assetBundleExportFilePath, true);
    }

    #endregion CREATE_SMDL_PROCEDURE


}
