using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class AutoGenerateSMDL
{
    #region MAIN_METHOD
    [MenuItem("SidiroAR/Auto create SMDL file...")]
    public static void CreateSMDLFile()
    {
        var generator = new AutoGenerateSMDL();

        generator.StartCreatingSMDLProcedure();

    }

    #endregion MAIN_METHOD

    #region CONSTANTS

    const string assetsDirName = "Assets";
    const string resourcesDirName = "Resources";
    const string prefabsDirName = "Prefabs";
    const string assetBundleDirName = "AssetBundles";
    const string modelContainerName = "_ModelContainer_";
    const string inputDirName = "Input";
    const string outputDirName = "Output";
    const float scale = 0.01f;

    #endregion CONSTANTS

    #region PROCEDURE_VALUES

    private string _inputObjFilePath;
    private string InputOBJFilePath
    {
        get
        {
            return _inputObjFilePath;
        }
    }

    private string InputMTLFilePath
    {
        get
        {
            return InputOBJFilePath.Replace(".obj", ".mtl");
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

    private string ModelsPrefabPath
    {
        get
        {
            return Path.Combine(PrefabsDirPath, TemporaryModelName + ".prefab");
        }
    }

    private string ModelName
    {
        get
        {
            return Path.GetFileNameWithoutExtension(InputOBJFilePath);
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

    private string ModelsAssetBundleFilePathAndroid
    {
        get
        {
            return Path.Combine(AssetBundlesDirPath, TemporaryModelName + ".smdl");
        }
    }

    private string ModelsAssetBundleFilePathIOS
    {
        get
        {
            return Path.Combine(AssetBundlesDirPath, TemporaryModelName + ".ismdl");
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

    private static string InputDirPath
    {
        get
        {
            return Path.Combine(assetsDirName, inputDirName);
        }
    }

    private static string OutputDirPath
    {
        get
        {
            return Path.Combine(assetsDirName, outputDirName);
        }
    }

    #endregion

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

            Debug.Log("Getting input model file path...");
            //Getting input model path
            GetInputFilePath();

            Debug.Log("Importing input model file into resources...");
            ImportInputModelFiles();

            //Creating new model container
            Debug.Log("Creating model container...");
            CreateModelContainer();

            //Setting properties of imported model such as scale, write/read enable etc.
            Debug.Log("Setting imported model properties");
            SetImportedModelProperties();

            //Creating asset in project from imported model
            Debug.Log("Creating asset from imported model");
            CreateAssetFromImportedModel();

            //Creating prefab from asset and setting its asset bundle name
            Debug.Log("Creating prefab from asset and setting its asset bundle name");
            CreatePrefabFromModelsAsset();

            //Generating asset bundles for andoird
            Debug.Log("Creating asset bundles");
            CreateAssetBundlesForAndroid();

            //Changing prefab name to ios name
            Debug.Log("Changing prefab name to IOS");
            SetPrefabIOSName();

            //Generating asset bundles for andoird
            Debug.Log("Creating asset bundles");
            CreateAssetBundlesForIOS();

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
            ClearAllDirectoriesExceptOutput();

            //Deleting model container if exists
            Debug.Log("Cleaning up model container...");
            DeleteModelContainer();

        }
        catch (Exception error)
        {
            Debug.Log(error);
            //Cleaning up in case of error

            //Clearing all data if exist
            ClearAllDirectoriesExceptOutput();

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
        ClearDirIfExistsOrCreateItIfNot(OutputDirPath);

        //Removing model container if exists
        DeleteModelContainer();

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    private void GetInputFilePath()
    {
        Debug.Log(InputDirPath);

        //Searching for files in input directory
        var files = Directory.GetFiles(InputDirPath);

        var objFiles = (from file in files where file.Contains(".obj") && !file.Contains(".obj.meta") select file).ToArray();

        foreach(var file in objFiles)
        {
            Debug.Log(file);
        }

        if(objFiles.Length < 1) throw new InvalidDataException("No OBJ file in Input Dir found!");

        if (objFiles.Length > 1) throw new InvalidDataException("More than one file in Input Dir found!");

        //Selecting element
        this._inputObjFilePath = objFiles[0];

        //Throwing if mtl file does not exist
        if (!File.Exists(InputMTLFilePath)) throw new InvalidDataException("No MTL file in Input Dir Found!");
    }

    /// <summary>
    /// Method for coping external models to unity project
    /// </summary>
    private void ImportInputModelFiles()
    {
        //Coping model to resources
        File.Copy(InputOBJFilePath, InteralModelOBJPath, true);
        File.Copy(InputMTLFilePath, InteralModelMTLPath, true);

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    private void ClearAllDirectoriesExceptOutput()
    {
        ClearDirectoryIfExists(AssetBundlesDirPath);
        ClearDirectoryIfExists(PrefabsDirPath);
        ClearDirectoryIfExists(ResourcesDirPath);
        ClearDirectoryIfExists(InputDirPath);

        //refreshing editors database
        AssetDatabase.Refresh();
    }

    private void DeleteModelContainer()
    {
        var modelContainerGO = GameObject.Find(modelContainerName);
        if (modelContainerGO != null) GameObject.DestroyImmediate(modelContainerGO);
    }

    /// <summary>
    /// Method for setting random name for model in project
    /// </summary>
    private void GenerateTemporaryFileName()
    {
        this._temporaryModelName = Path.GetFileNameWithoutExtension(Path.GetRandomFileName());
    }

    /// <summary>
    /// Method for setting internal model properties - such as scale, read/write enable etc.
    /// </summary>
    private void SetImportedModelProperties()
    {
        //Setting model properties
        var modelImporter = AssetImporter.GetAtPath(InteralModelOBJPath) as ModelImporter;
        modelImporter.globalScale = scale;
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
    /// Method for setting name of the prefab for asmdl (IOS) model
    /// </summary>
    private void SetPrefabIOSName()
    {
        //Setting name for created prefab
        AssetImporter.GetAtPath(ModelsPrefabPath).SetAssetBundleNameAndVariant(TemporaryModelName, "ismdl");

    }

    /// <summary>
    /// Method for creating asset bundles from project
    /// </summary>
    private void CreateAssetBundlesForAndroid()
    {
        BuildPipeline.BuildAssetBundles(AssetBundlesDirPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.Android);
    }

    /// <summary>
    /// Method for creating asset bundles from project
    /// </summary>
    private void CreateAssetBundlesForIOS()
    {
        BuildPipeline.BuildAssetBundles(AssetBundlesDirPath, BuildAssetBundleOptions.ChunkBasedCompression, BuildTarget.iOS);
    }

    private void ExportAssetBundleFile()
    {
        //Saving asset bundle file to external dir - Android
        string assetBundleExportIOSFilePath = Path.Combine(OutputDirPath, String.Format("{0}.smdl", ModelName));
        File.Copy(ModelsAssetBundleFilePathAndroid, assetBundleExportIOSFilePath, true);

        //Saving asset bundle file to external dir - iOS
        string assetBundleExportIOSFilePathIOS = Path.Combine(OutputDirPath, String.Format("{0}.ismdl", ModelName));
        File.Copy(ModelsAssetBundleFilePathIOS, assetBundleExportIOSFilePathIOS, true);
    }

    #endregion CREATE_SMDL_PROCEDURE

}
