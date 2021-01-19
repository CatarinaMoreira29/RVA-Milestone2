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


    void Start()
    {
        aparecer.SetActive(true);
        desaparecer.SetActive(false);
        painel.SetActive(false); 
    }


    private void Awake()
    {
        trackedImageManager = GetComponent<ARTrackedImageManager>();
        instanciatePrefab = new Dictionary<string, GameObject>();

    }

    private void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += OnTrackedImageChanged;
    }

    private void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= OnTrackedImageChanged;
    }

    private void OnTrackedImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach (ARTrackedImage addedImage in eventArgs.added)
        {
            imageTrackedText.text = addedImage.referenceImage.name;
            InstantiateGameObject(addedImage);
        }

        foreach (ARTrackedImage updatedImage in eventArgs.updated)
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
        }
    }

    private void InstantiateGameObject(ARTrackedImage addedImage)
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
    }

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

    
}


[System.Serializable]
public struct TrackedPrefab
{
    public string name;
    public GameObject prefab;

}
