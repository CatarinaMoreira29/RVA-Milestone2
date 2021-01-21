using Google.XR.ARCoreExtensions;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;



[RequireComponent(typeof(ARTrackedImageManager))]
public class TrackedImage : MonoBehaviour
{

    [SerializeField]
    private TrackedPrefab[] prefabToInstantiate;

    private Dictionary<string, GameObject> instanciatePrefab;

    private ARTrackedImageManager trackedImageManager;


    [SerializeField]
    private Text imageTrackedText;

    [SerializeField]
    private Text lista;

    [SerializeField]
    private Text tarefa1;

    [SerializeField]
    private Text tarefa2;

    [SerializeField]
    private Text tarefa3;

    private Toggle toggle1;
    private Toggle toggle11;
    private Toggle toggle2;
    private Toggle toggle22;
    private Toggle toggle3;
    private Toggle toggle33;

    [SerializeField]
    private GameObject aparecer;
    [SerializeField]
    private GameObject desaparecer;
    [SerializeField]
    private GameObject painel;

    /**************************************************************
    ************************************************************
    *********************************************************/

    [Header("AR Foundation")]
    public ARSessionOrigin SessionOrigin;
    public ARSession SessionCore;
    public ARCoreExtensions Extensions;
    public ARAnchorManager AnchorManager;
    public ARPlaneManager PlaneManager;
    public ARRaycastManager RaycastManager;

    public string firstStage;


    public Camera MainCamera
    {
        get
        {
            return SessionOrigin.camera;
        }
    }


    [Header("UI")]
    public Text feddback;


    [Header("Postion Elements")]
    public GameObject object1;
    public GameObject object2;
    public GameObject placementIndicator;

    public GameObject MapQualityIndicatorPrefab;
    private MapQualityIndicator _qualityIndicator = null;

    private ARPlane plane;


    private CloudObjectStatus cloudAnchorStatus;

    private Pose pose;
    private GameObject objectPalced;
    private ARAnchor _anchor = null;
    private ARCloudAnchor _cloudAnchor;


    //private ARRaycastManager aRRaycastManager;
    private bool placementPoseIsValid = false;

    public enum CloudObjectStatus
    {
        NOT_PLACED,
        PLACED,
        QUALITY_CHECKED,
        UPLOADING,
        UPLOADED,
        ERROR
    }

    public void OnEnable()
    {
        UpdatePlaneVisibility(true);
        trackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }


    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    }




    void Start()
    {
        aparecer.SetActive(true);
        desaparecer.SetActive(false);
        painel.SetActive(false);
        firstStage = "Desocupada";
    }


    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        instanciatePrefab = new Dictionary<string, GameObject>();

    }




    void Update()
    {
       /* if (cloudAnchorStatus == CloudObjectStatus.NOT_PLACED)
        {

            placeObject();
        }
        else*/ if (cloudAnchorStatus == CloudObjectStatus.PLACED)
        {
            updateMapQuality();
            Debug.Log("Object Placed");
            feddback.text = "Placed!!!!!!";
            checkCloudAnchorQualityAndUpload();
        }
        else if (cloudAnchorStatus == CloudObjectStatus.QUALITY_CHECKED)
        {
            feddback.text = "Uploading!!!!!!";



            checkUploadStatus();
        }
        else if (cloudAnchorStatus == CloudObjectStatus.UPLOADED)
        {
            Debug.Log("Success hosting cloud Anchor");
            uploadAnotherCloudAnchor();
        }
        else if (cloudAnchorStatus == CloudObjectStatus.ERROR)
        {
            Debug.Log("Error hosting cloud Anchor");
            if (objectPalced != null)
            {
                Destroy(objectPalced);
                objectPalced = null;
            }
            if (_cloudAnchor != null)
            {
                Destroy(_cloudAnchor);
                _cloudAnchor = null;
            }
            uploadAnotherCloudAnchor();
        }
    }
    private void uploadAnotherCloudAnchor()
    {
        // Restart
        cloudAnchorStatus = CloudObjectStatus.NOT_PLACED;
    }

    private void checkUploadStatus()
    {
        if (_cloudAnchor.cloudAnchorState == CloudAnchorState.TaskInProgress)
        {
            Debug.Log("In progress hosting cloud Anchor");
        }
        else if (_cloudAnchor != null && _cloudAnchor.cloudAnchorState == CloudAnchorState.Success)
        {
            Debug.Log("Success hosting cloud Anchor");
            cloudAnchorStatus = CloudObjectStatus.UPLOADED;
        }
        else
        {
            Debug.Log("Error hosting cloud Anchor");
            cloudAnchorStatus = CloudObjectStatus.ERROR;
        }
    }


    private void checkCloudAnchorQualityAndUpload()
    {
        placementIndicator.SetActive(false);

        FeatureMapQuality quality = AnchorManager.EstimateFeatureMapQualityForHosting(pose);

        feddback.text = string.Format("Quality for object palced {0} in   ", quality);

        if (quality != FeatureMapQuality.Insufficient)
        {
            // Host anchors 
            _cloudAnchor = AnchorManager.HostCloudAnchor(_anchor, 1);
            feddback.text = "Quality is ok for hosting cloud Anchor";
            cloudAnchorStatus = CloudObjectStatus.QUALITY_CHECKED;
        }
        else if (quality == FeatureMapQuality.Insufficient)
        {
            feddback.text = "Quality is NOT ok for hosting cloud Anchor";
        }
        else
        {
            feddback.text = "Quality is DAFUCK";
        }
    }




    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage addedImage in eventArgs.added)
        {
            feddback.text = " "+firstStage+" e " +addedImage.referenceImage.name+" \n\n  " + feddback.text;

            GameObject tempObject;
         //   if (firstStage == "Desocupada" && addedImage.referenceImage.name == "Blue")
          //  {
                feddback.text = "Vai desenhar a cloud";
                imageTrackedText.text = addedImage.referenceImage.name;
                //InstantiateGameObject(addedImage);
                if (addedImage.referenceImage.name == "Blue")
                {
                    tempObject = object1;
                }
                else
                {
                 //   Destroy(_anchor);
                   // _anchor = null;
                    tempObject = object2;
                  // cloudAnchorStatus = CloudObjectStatus.NOT_PLACED;
                }
                feddback.text = "vai por a imagem: " + tempObject;
                placeObject(tempObject);
                cloudAnchorStatus = CloudObjectStatus.PLACED;
                firstStage = "Ocupada";
                feddback.text = "Mudou o estado para ocupada";

                // PerformHitTest(Input.GetTouch(0).position);
           // }
          /*  else{ if (firstStage == "Ocupada" && addedImage.referenceImage.name == "Pink")
                {
                    feddback.text = "Vai destruir uma e desenhar a outra";

                    Destroy(_cloudAnchor);
                    _cloudAnchor = null;
                    //tempObject = object2;
                    //placeObject(tempObject);
                    //cloudAnchorStatus = CloudObjectStatus.PLACED;
                    //firstStage = "Desocupada";
                }

            }*/

        }

        /*foreach (ARTrackedImage updatedImage in eventArgs.updated)
        {
            if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Tracking)
            {
                UpdateTrackingGameObject(updatedImage);
            }
            else if (updatedImage.trackingState == UnityEngine.XR.ARSubsystems.TrackingState.Limited)
            {
                UpdateLimitedGameObject(updatedImage);
            }
            else
            {
                UpdateNoneGameObject(updatedImage);
            }
        }

        foreach (ARTrackedImage removedImage in eventArgs.removed)
        {
            DestroyGameObject(removedImage);
        }*/
    }

    private void placeObject(GameObject objectToPlace)
    {
        UpdatePlacementPose();
        UpdatePlacementIndicator();
        if (_anchor != null)
        {
            _anchor.transform.position = pose.position;
            _anchor.transform.rotation = pose.rotation;


        }
        else
        {
            _anchor = AnchorManager.AddAnchor(pose);

        }
        feddback.text = "anchor "+_anchor;
        if (_anchor != null)
        {
            var planeType = PlaneAlignment.HorizontalUp;
            planeType = plane.alignment;
            Instantiate(objectToPlace, _anchor.transform);
            // Attach map quality indicator to this anchor.
            var indicatorGO =
                Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
            _qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
            _qualityIndicator.DrawIndicator(planeType, MainCamera);


            feddback.text = "Waiting for sufficient mapping quaility...";

            // Hide plane generator so users can focus on the object they placed.
            UpdatePlaneVisibility(false);
        }
    }


    /* private void placeObject()
     {
         UpdatePlacementPose();
         UpdatePlacementIndicator();

         if (placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
         {
             PerformHitTest(Input.GetTouch(0).position);
             //_anchor = AnchorManager.AddAnchor(pose);
             //objectPalced = Instantiate(objectToPlace, _anchor.transform);
             ////_anchor = gameObject.GetComponent<ARAnchor>();



             //var indicatorGO =
             //        Instantiate(MapQualityIndicatorPrefab, _anchor.transform);
             //_qualityIndicator = indicatorGO.GetComponent<MapQualityIndicator>();
             //_qualityIndicator.DrawIndicator(plane.alignment, MainCamera);

             cloudAnchorStatus = CloudObjectStatus.PLACED;
         }
     }*/



    /*private void InstantiateGameObject(ARTrackedImage addedImage)
    {
        for (int i = 0; i < prefabToInstantiate.Length; i++)
        {
            if (addedImage.referenceImage.name == prefabToInstantiate[i].name)
            {
                GameObject prefab = Instantiate<GameObject>(prefabToInstantiate[i].prefab, transform.parent);
                prefab.transform.position = addedImage.transform.position;
                prefab.transform.rotation = addedImage.transform.rotation;

                instanciatePrefab.Add(addedImage.referenceImage.name, prefab);
            }
        }
    }*/

    private void UpdateTrackingGameObject(ARTrackedImage updatedImage)
    {

        for (int i = 0; i < instanciatePrefab.Count; i++)
        {
            if (instanciatePrefab.TryGetValue(updatedImage.referenceImage.name, out GameObject prefab))
            {
                prefab.transform.position = updatedImage.transform.position;
                prefab.transform.rotation = updatedImage.transform.rotation;
                prefab.SetActive(true);


                imageTrackedText.text = updatedImage.referenceImage.name;

              
                if (updatedImage.referenceImage.name == "Blue")
                {
                    lista.enabled = true;
                    lista.text = "Lista Posto 2";
                    tarefa1.text = "Adicionar pernas";
                    tarefa2.text = "Adicionar vidro";
                    tarefa3.text = "Pintar";

                    toggle11.enabled = false;
                    toggle22.enabled = false;
                    toggle33.enabled = false; 
                }
                if (updatedImage.referenceImage.name == "Pink")
                {
                    lista.enabled = true;
                    lista.text = "Lista Posto 4";
                    tarefa1.text = "Ir buscar papel de embrulho";
                    tarefa2.text = "Ir buscar tesoura";
                    tarefa3.text = "Ir buscar tesoura e Embrulhar";

                    toggle1.enabled = false;
                    toggle2.enabled = false;
                    toggle3.enabled = false;
                }
              
            }
        }
    }

    private void UpdateLimitedGameObject(ARTrackedImage updatedImage)
    {
        for (int i = 0; i < instanciatePrefab.Count; i++)
        {
            if (instanciatePrefab.TryGetValue(updatedImage.referenceImage.name, out GameObject prefab))
            {
                if (!prefab.GetComponent<ARTrackedImage>().destroyOnRemoval)
                {
                    prefab.transform.position = updatedImage.transform.position;
                    prefab.transform.rotation = updatedImage.transform.rotation;
                    prefab.SetActive(true);
                   
                    if (updatedImage.referenceImage.name == "Blue")
                    {
                        lista.enabled = true;
                        lista.text = "Lista Posto 2";
                        tarefa1.text = "Tarefa 1";
                        tarefa2.text = "Tarefa 2";
                        tarefa3.text = "Tarefa 3";
                    }
                    if (updatedImage.referenceImage.name == "Pink")
                    {
                        lista.enabled = true;
                        lista.text = "Lista Posto 4";
                        tarefa1.text = "Ir buscar papel de embrulho";
                        tarefa2.text = "Ir buscar tesoura";
                        tarefa3.text = "Ir buscar tesoura e Embrulhar";
                    }
                   
                }
                else
                {
                    prefab.SetActive(false);
                }

            }
        }
    }

    private void UpdateNoneGameObject(ARTrackedImage updateImage)
    {
        for (int i = 0; i < instanciatePrefab.Count; i++)
        {
            if (instanciatePrefab.TryGetValue(updateImage.referenceImage.name, out GameObject prefab))
            {
                prefab.SetActive(false);
            }
        }
    }

    private void DestroyGameObject(ARTrackedImage removedImage)
    {
        for (int i = 0; i < instanciatePrefab.Count; i++)
        {
            if (instanciatePrefab.TryGetValue(removedImage.referenceImage.name, out GameObject prefab))
            {
                instanciatePrefab.Remove(removedImage.referenceImage.name);
                Destroy(prefab);
            }
        }
    }

    public void ClosePanel()
    {
        aparecer.SetActive(true);
        desaparecer.SetActive(false);
        painel.SetActive(false); 
    }

    public void openPanel()
    {
        aparecer.SetActive(false);
        desaparecer.SetActive(true);
        painel.SetActive(true); 

    }


  

    private void UpdatePlacementIndicator()
    {
        if (placementPoseIsValid)
        {
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(pose.position, pose.rotation);
        }
        else
        {
            placementIndicator.SetActive(false);
        }
    }


    private void UpdatePlacementPose()
    {
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        RaycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid)
        {
            plane = PlaneManager.GetPlane(hits[0].trackableId);
            pose = hits[0].pose;
            var cameraFoward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraFoward.x, 0, cameraFoward.z).normalized;

            pose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }


    private void updateMapQuality()
    {
        int qualityState = 2;
        // Can pass in ANY valid camera pose to the mapping quality API.
        // Ideally, the pose should represent users’ expected perspectives.
        FeatureMapQuality quality =
           AnchorManager.EstimateFeatureMapQualityForHosting(GetCameraPose());
       // feddback.text = "Current mapping quality: " + quality;
        qualityState = (int)quality;
        _qualityIndicator.UpdateQualityState(qualityState);
    }


    public Pose GetCameraPose()
    {
        return new Pose(MainCamera.transform.position,
            MainCamera.transform.rotation);
    }

    private void UpdatePlaneVisibility(bool visible)
    {
        foreach (var plane in PlaneManager.trackables)
        {
            plane.gameObject.SetActive(visible);
        }
    }
}


[System.Serializable]
public struct TrackedPrefab
{
    public string name;
    public GameObject prefab;

}
