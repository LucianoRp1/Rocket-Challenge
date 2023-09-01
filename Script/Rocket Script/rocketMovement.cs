using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.XR;


public class rocketMovement : MonoBehaviour
{

    #region GameObject
    public GameObject paracaidas;
    public GameObject corpoNariz;
    public GameObject primerEstagio;
    #endregion
    #region AudioSource
    public AudioSource corpoPullSound;
    public AudioSource corpoPropeller;
    public AudioSource paracaidasSound;
    public AudioSource airSound;
    public AudioSource primerEstagioSound;
    #endregion

    #region Float
    public float upwardForce;
    public float maxHeight;
    public float currentHeight;
    public float fuelConsumptionRate;
    public float fuelRocket;
    public float timeRotateCorpo;
    public float timeRocket;
    private float rotationDuration = 2f;
    public float volumenFirst;
    #endregion

    #region Bool
    private bool isFuelEmpty = false;
    private bool isSoundAir = false;
    private bool soundPlayed = false;
    private bool isRotatingCorpo = false; 
    private bool soundPlayedCorpo = false;
    private bool isSecoundPropeller = false;
    #endregion

    #region ParticleSystem
    public ParticleSystem flameCorpoPropeller;
    public ParticleSystem flamePrimerEstagioSound;
    #endregion


    #region Rigibody
    public Rigidbody rb;
    public Rigidbody primerEstagioBody;
    #endregion


    #region MeshRenderer
    public MeshRenderer paracaidasRenderer;
    #endregion

    #region Quaternion
    private Quaternion initialCorpoRotation; 
    #endregion

    #region
    public TextMeshProUGUI textMeshComponent;
    #endregion

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        fuelRocket = fuelRocket * fuelConsumptionRate;
        initialCorpoRotation = corpoNariz.transform.rotation;
        primerEstagioBody.isKinematic = true;
        maxHeight = transform.position.y;
    }




    private void FixedUpdate()
    {
        float newXPosition = primerEstagio.transform.position.x + 0.02f;
        Vector3 newPosition = new Vector3(newXPosition, primerEstagio.transform.position.y, primerEstagio.transform.position.z);


        Vector3 positionFirst = new Vector3(primerEstagio.transform.position.x, primerEstagio.transform.position.y, primerEstagio.transform.position.z);

        float newZPositionCorpo = corpoNariz.transform.position.z + 0.02f;
        Vector3 newPositionCorpo = new Vector3(corpoNariz.transform.position.x, corpoNariz.transform.position.y, newZPositionCorpo);

        float newZPositionParacaida = paracaidas.transform.position.z + 0.02f;
        Vector3 newPositionParacaidas = new Vector3(paracaidas.transform.position.x, paracaidas.transform.position.y, newZPositionParacaida);


        var emissionModule = flamePrimerEstagioSound.emission;
        emissionModule.enabled = false;




        var emissionModuleCorpo = flameCorpoPropeller.emission;
        emissionModuleCorpo.enabled = false;

        currentHeight = transform.position.y;

        if (!isFuelEmpty)
        {
            rb.AddForce(Vector3.up * upwardForce, ForceMode.Force);
            fuelRocket -= fuelConsumptionRate * Time.deltaTime;
            emissionModule.enabled = true;
          //  Debug.Log("holi");
            if (!primerEstagioSound.isPlaying)
            {
                primerEstagioSound.Play();
            }
            if (fuelRocket <= 0)
            {
                fuelRocket = 0;
                timeRocket -= Time.deltaTime;
                timeRotateCorpo -= Time.deltaTime;
                primerEstagio.transform.parent = null;
                primerEstagioBody.isKinematic = false;
                primerEstagio.transform.position = newPosition;
               
                primerEstagioBody.drag = 2f;
                emissionModule.enabled = false;
                emissionModuleCorpo.enabled = true;
                primerEstagioSound.volume -= volumenFirst * Time.fixedDeltaTime;
                if (!soundPlayedCorpo)
                {
                    corpoPullSound.Play();
                    soundPlayedCorpo = true;
                }
                if (!isSecoundPropeller)
                {
                    corpoPropeller.Play();
                    isSecoundPropeller = true;
                }
                if (primerEstagio.transform.position.y <= 1)
                {
                    newPosition = positionFirst;
                    primerEstagioBody.position = newPosition;
                    primerEstagioBody.drag = 0f;
                    primerEstagio.transform.position = positionFirst;
                 
                    primerEstagioBody.freezeRotation = true;

                }
                if (timeRocket <= 0 && !isRotatingCorpo)
                {
                    StartCoroutine(RotateCorpo());
                    timeRocket = 0;
                }
                if (isRotatingCorpo)
                {
                    float rotationSpeed = 90f;
                    float step = rotationSpeed * Time.fixedDeltaTime;
                    emissionModuleCorpo.enabled = false;
                    Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f);
                    corpoNariz.transform.rotation = Quaternion.RotateTowards(corpoNariz.transform.rotation, targetRotation, step);
                    if (timeRotateCorpo <= 0)
                    {
                        Quaternion nuevaRotacion = Quaternion.Euler(90f, paracaidas.transform.localEulerAngles.y, paracaidas.transform.localEulerAngles.z);
                        paracaidas.transform.rotation = nuevaRotacion;
                        paracaidasRenderer.enabled = true;

                        if (!soundPlayed)
                        {
                            paracaidasSound.Play();
                            soundPlayed = true;
                            corpoPropeller.Stop();
                            if (!isSoundAir)
                            {
                                isSoundAir = true;
                                airSound.Play();
                                Debug.Log("de aire");
                            }

                        }

                        var corpoPosition = corpoNariz.transform.position.y;
                        Vector3 nuevaPositionParacaide = new Vector3(paracaidas.transform.position.x, corpoPosition + 1f, paracaidas.transform.position.z);
                        paracaidas.transform.position = nuevaPositionParacaide;

                        timeRotateCorpo = 0f;
                        timeRocket = 0f;
                        rb.velocity = Vector3.down;
                        rb.mass = 2.5f;
                        rb.freezeRotation = true;
                    }

                }
                if (rb.velocity.y <= 0 && currentHeight > maxHeight)
                {
                    maxHeight = currentHeight;
                    textMeshComponent.text = "Altura Maxima: " + maxHeight;
                    Debug.Log("altura maxima " + maxHeight);
                }
            }
        }
    }

    private IEnumerator RotateCorpo()
    {
        isRotatingCorpo = true; 
        Quaternion initialRotation = corpoNariz.transform.rotation;
        Quaternion targetRotation = Quaternion.Euler(90f, 0f, 0f);

        float elapsedTime = 0f;

        while (elapsedTime < rotationDuration)
        {
            float t = elapsedTime / rotationDuration;
            corpoNariz.transform.rotation = Quaternion.Lerp(initialRotation, targetRotation, t);

            elapsedTime += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

       
        corpoNariz.transform.rotation = targetRotation;
        isRotatingCorpo = false;
        timeRotateCorpo = 0f;
        timeRocket = 0f;
    }



}

