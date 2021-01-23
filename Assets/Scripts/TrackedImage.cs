using Google.XR.ARCoreExtensions;
using Google.XR.ARCoreExtensions.Samples.PersistentCloudAnchors;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    [SerializeField]
    private GameObject painelTools;
    [SerializeField]
    private Button sprayButton;
    [SerializeField]
    private GameObject buttonTools;
    [SerializeField]
    private Button bodyButton;
    [SerializeField]
    private Button glassButton;

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

    public Text armClone;

    public GameObject secondStageObject;

    /*****************************************************/


    Ray ray;
    RaycastHit hit;



    [SerializeField]
    private UnityEngine.UI.Text choque;

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
    public GameObject placementIndicator;

    public GameObject MapQualityIndicatorPrefab;
    private MapQualityIndicator _qualityIndicator = null;

    private ARPlane plane;


    private CloudObjectStatus cloudAnchorStatus;

    private Pose pose;
    private GameObject objectPlaced;
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
        painelTools.SetActive(false);
        buttonTools.SetActive(false);
    }


    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        instanciatePrefab = new Dictionary<string, GameObject>();

    }




    void Update()
    {
        if (cloudAnchorStatus == CloudObjectStatus.PLACED)
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
            if (objectPlaced != null)
            {
                Destroy(objectPlaced);
                objectPlaced = null;
            }
            if (_cloudAnchor != null)
            {
                Destroy(_cloudAnchor);
                _cloudAnchor = null;
            }
            uploadAnotherCloudAnchor();
        }


        if (Input.touchCount > 0 || Input.GetTouch(0).phase == TouchPhase.Began)
        {


            //Converter a posição para array 
            ray = Camera.main.ScreenPointToRay(Input.GetTouch(0).position);

            Debug.DrawRay(ray.origin, ray.direction * 20f, Color.red);

            if (Physics.Raycast(ray, out hit, Mathf.Infinity))
            {


                if (hit.transform.gameObject.name == "Pink(Clone)")

                {
                    choque.text = hit.transform.gameObject.name;

                }



            }
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
              imageTrackedText.text = addedImage.referenceImage.name;

            //InstantiateGameObject(addedImage);
            if (addedImage.referenceImage.name == "Blue")
            {
                placeObject(object1);
                buttonTools.SetActive(true);
                choque.text = "Montar o primeiro objecto";
            }
            else
            {
                armClone.text = "Aquiiii mario " + addedImage.referenceImage.name;

                //VERIFICA SE O OBJECTO ESTA COMPLETO
                if (GameObject.Find("Arm").GetComponent<MeshRenderer>().enabled == true
                    && GameObject.Find("fire-nozzle").GetComponent<MeshRenderer>().enabled == true
                    && GameObject.Find("head").GetComponent<MeshRenderer>().enabled == true
                    && GameObject.Find("nute").GetComponent<MeshRenderer>().enabled == true
                    && GameObject.Find("win-frem").GetComponent<MeshRenderer>().enabled == true
                    && GameObject.Find("win.001").GetComponent<MeshRenderer>().enabled == true 
                    && GameObject.Find("boddy").GetComponent<Renderer>().material.color == Color.white)
                {
                    placeObject(object1);
                    buttonTools.SetActive(false);
                    painelTools.SetActive(true);
                    Invoke("createPresent", 2);

                }
                else
                {
                    choque.text = "Objecto nao esta completo";
                }
            }
            cloudAnchorStatus = CloudObjectStatus.PLACED;

        }
    }

    public void createPresent()
    {

        objectPlaced = Instantiate(secondStageObject, _anchor.transform);
    }

    private void placeObject(GameObject objectToPlace)
    {
        armClone.text = "Esta na funçao placeobject";

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
        if (_anchor != null)
        {
            var planeType = PlaneAlignment.HorizontalUp;
            planeType = plane.alignment;
            objectPlaced = Instantiate(objectToPlace, _anchor.transform);
            //objectPlaced.transform.localPosition.x=20.0;
            objectPlaced.name = "Rocket";
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

    public void ChangeColor()
    {
        // GameObject.Find("Rocket").GetComponent<MeshRenderer>().enabled = false;
        //objectPlaced.GetComponent<Renderer>().material.color = Color.white;
        sprayButton.enabled = false;
        GameObject.Find("boddy").GetComponent<Renderer>().material.color = Color.white;

    }

    public void addPartsOfBody()
    {

        GameObject.Find("Arm").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("fire-nozzle").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("head").GetComponent<MeshRenderer>().enabled = true;

        bodyButton.enabled = false;
    }

    public void addGlass()
    {

        GameObject.Find("nute").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("win-frem").GetComponent<MeshRenderer>().enabled = true;
        GameObject.Find("win.001").GetComponent<MeshRenderer>().enabled = true;

        glassButton.enabled = false;

    }

    public void openToolsPainel()
    {
        if (painelTools.active == true)
        {
            painelTools.SetActive(false);
        }
        else
        {
            painelTools.SetActive(true);
        }
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
