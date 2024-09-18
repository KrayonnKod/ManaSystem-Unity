using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(AudioSource))]
public class FPSController : MonoBehaviour
{
    [Header("Hareket Ayarları")]
    public float yurumeHizi = 5f;
    public float kosmaHizi = 10f;
    public float egilmeHizi = 2f;
    public float ziplamaHizi = 8f;
    public float yercekimi = 20f;

    [Header("Kamera Ayarları")]
    public Camera oyuncuKamerasi;
    public float kameraHassasiyeti = 2f;
    public float kameraEgilmeLimiti = 85f;

    [Header("Animasyon Ayarları")]
    public string yurumeAnimParam = "isWalking";
    public string kosmaAnimParam = "isRunning";
    public string ziplamaAnimParam = "isJumping";
    public string egilmeAnimParam = "isCrouching";
    public string ciftZiplaAnimParam = "isDoubleJumping";

    [Header("Kontrol Ayarları")]
    public KeyCode kosmaTus = KeyCode.LeftShift;
    public KeyCode egilmeTus = KeyCode.LeftControl;

    [Header("Özellik Ayarları")]
    public bool kosmaAktif = true;
    public bool egilmeAktif = true;
    public bool ciftZiplamaAktif = true;

    [Header("Ses Ayarları")]
    public AudioClip yurumeSesi;
    public AudioClip kosmaSesi;
    public AudioClip ziplamaSesi;
    public AudioClip egilmeSesi;
    public float sesSeviyesi = 0.5f;

    [Header("Mana Ayarları")]
    public ManaSystem manaSystem;
    public float ziplamaManaTuketimi = 10f;
    public float ciftZiplamaManaTuketimi = 15f;

    private CharacterController karakterKontrol;
    private Animator animator;
    private AudioSource sesKaynagi;

    private Vector3 hareketYon = Vector3.zero;
    private bool zipliyor;
    private bool ciftZipliyor;
    private bool kosuyor;
    private bool egiliyor;
    private int ziplamaSayaci = 0;

    private float kameraEgilme = 0f;

    private bool yurumeSesiCalisiyor = false;
    private bool kosmaSesiCalisiyor = false;
    private bool egilmeSesiCalisiyor = false;

    void Start()
    {
        karakterKontrol = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        sesKaynagi = GetComponent<AudioSource>();

        if (oyuncuKamerasi == null)
        {
            oyuncuKamerasi = Camera.main;

            if (oyuncuKamerasi == null)
            {
                GameObject kameraObjesi = new GameObject("OyuncuKamerasi");
                oyuncuKamerasi = kameraObjesi.AddComponent<Camera>();
                kameraObjesi.transform.parent = this.transform;
                kameraObjesi.transform.localPosition = new Vector3(0, 1.6f, 0);
            }
        }

        if (animator != null)
        {
            animator.SetBool(yurumeAnimParam, false);
            animator.SetBool(kosmaAnimParam, false);
            animator.SetBool(ziplamaAnimParam, false);
            animator.SetBool(egilmeAnimParam, false);
            animator.SetBool(ciftZiplaAnimParam, false);
        }

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        sesKaynagi.playOnAwake = false;
        sesKaynagi.loop = false;
        sesKaynagi.volume = sesSeviyesi;
    }

    void Update()
    {
        HareketKontrol();
        KameraKontrol();

        if (kosmaAktif) KosmaKontrol();
        if (egilmeAktif) EgilmeKontrol();
    }

    void HareketKontrol()
    {
        float yatayHareket = Input.GetAxis("Horizontal");
        float dikeyHareket = Input.GetAxis("Vertical");

        Vector3 hareket = transform.right * yatayHareket + transform.forward * dikeyHareket;

        if (karakterKontrol.isGrounded)
        {
            hareketYon = hareket * (egiliyor ? egilmeHizi : (kosuyor ? kosmaHizi : yurumeHizi));
            ziplamaSayaci = 0;

            if (hareket.magnitude > 0 && !zipliyor)
            {
                if (kosuyor)
                {
                    if (!kosmaSesiCalisiyor && kosmaSesi != null)
                    {
                        SesCal(kosmaSesi);
                        kosmaSesiCalisiyor = true;
                        yurumeSesiCalisiyor = false;
                    }
                }
                else
                {
                    if (!yurumeSesiCalisiyor && yurumeSesi != null)
                    {
                        SesCal(yurumeSesi);
                        yurumeSesiCalisiyor = true;
                        kosmaSesiCalisiyor = false;
                    }
                }
            }
            else
            {
                if (sesKaynagi.isPlaying)
                {
                    sesKaynagi.Stop();
                    yurumeSesiCalisiyor = false;
                    kosmaSesiCalisiyor = false;
                }
            }

        
            if (Input.GetButtonDown("Jump"))
            {
                if (manaSystem.HasEnoughMana() && manaSystem.currentMana >= ziplamaManaTuketimi)
                {
                    hareketYon.y = ziplamaHizi;
                    zipliyor = true;
                    animator.SetBool(ziplamaAnimParam, true);
                    manaSystem.UseMana(ziplamaManaTuketimi); 

                    if (ziplamaSesi != null)
                    {
                        SesBirKereCal(ziplamaSesi);
                    }

                    if (sesKaynagi.isPlaying)
                    {
                        sesKaynagi.Stop();
                        yurumeSesiCalisiyor = false;
                        kosmaSesiCalisiyor = false;
                    }
                }
            }
            else
            {
                zipliyor = false;
                animator.SetBool(ziplamaAnimParam, false);
            }

            if (hareket.magnitude > 0)
            {
                animator.SetBool(yurumeAnimParam, !kosuyor && !egiliyor);
                animator.SetBool(kosmaAnimParam, kosuyor);
                animator.SetBool(egilmeAnimParam, egiliyor);
            }
            else
            {
                animator.SetBool(yurumeAnimParam, false);
                animator.SetBool(kosmaAnimParam, false);
                animator.SetBool(egilmeAnimParam, false);
            }
        }
        else
        {
            if (ciftZiplamaAktif && ziplamaSayaci == 0 && Input.GetButtonDown("Jump"))
            {
                if (manaSystem.HasEnoughMana() && manaSystem.currentMana >= ciftZiplamaManaTuketimi)
                {
                    hareketYon.y = ziplamaHizi;
                    ciftZipliyor = true;
                    ziplamaSayaci++;
                    animator.SetBool(ciftZiplaAnimParam, true);
                    manaSystem.UseMana(ciftZiplamaManaTuketimi); 

                    if (ziplamaSesi != null)
                    {
                        SesBirKereCal(ziplamaSesi);
                    }

                    if (sesKaynagi.isPlaying)
                    {
                        sesKaynagi.Stop();
                        yurumeSesiCalisiyor = false;
                        kosmaSesiCalisiyor = false;
                    }
                }
            }
        }

        hareketYon.y -= yercekimi * Time.deltaTime;

        karakterKontrol.Move(hareketYon * Time.deltaTime);

        if (ciftZipliyor && karakterKontrol.isGrounded)
        {
            ciftZipliyor = false;
            animator.SetBool(ciftZiplaAnimParam, false);
        }
    }

    void KameraKontrol()
    {
        float fareX = Input.GetAxis("Mouse X") * kameraHassasiyeti;
        float fareY = Input.GetAxis("Mouse Y") * kameraHassasiyeti;

        transform.Rotate(Vector3.up * fareX);

        kameraEgilme -= fareY;
        kameraEgilme = Mathf.Clamp(kameraEgilme, -kameraEgilmeLimiti, kameraEgilmeLimiti);

        oyuncuKamerasi.transform.localEulerAngles = Vector3.right * kameraEgilme;
    }

void KosmaKontrol()
{
    if (Input.GetKey(kosmaTus) && !egiliyor) 
    {
 
        if (manaSystem.currentMana > 1f) 
        {
            kosuyor = true; 
            manaSystem.UseMana(30f * Time.deltaTime);  //Kullanılacak mana için değiştiriniz! -KrayonnKod-


            if (!kosmaSesiCalisiyor && kosmaSesi != null)
            {
                SesCal(kosmaSesi);
                kosmaSesiCalisiyor = true;
                yurumeSesiCalisiyor = false;
            }
        }
        else
        {
            
            kosuyor = false;
            if (kosmaSesiCalisiyor)
            {
                sesKaynagi.Stop();
                kosmaSesiCalisiyor = false;
            }
        }
    }
    else
    {
        kosuyor = false; 

        
        if (!yurumeSesiCalisiyor && yurumeSesi != null && karakterKontrol.velocity.magnitude > 0)
        {
            SesCal(yurumeSesi);
            yurumeSesiCalisiyor = true;
            kosmaSesiCalisiyor = false;
        }
        else if (sesKaynagi.isPlaying)
        {
            sesKaynagi.Stop();
            kosmaSesiCalisiyor = false;
        }
    }
}




    void EgilmeKontrol()
    {
        if (Input.GetKey(egilmeTus))
        {
            egiliyor = true;

            if (!egilmeSesiCalisiyor && egilmeSesi != null)
            {
                SesCal(egilmeSesi);
                egilmeSesiCalisiyor = true;
            }

            if (sesKaynagi.clip == yurumeSesi || sesKaynagi.clip == kosmaSesi)
            {
                sesKaynagi.Stop();
                yurumeSesiCalisiyor = false;
                kosmaSesiCalisiyor = false;
            }
        }
        else
        {
            egiliyor = false;
            egilmeSesiCalisiyor = false;

            if (karakterKontrol.velocity.magnitude > 0)
            {
                if (kosuyor)
                {
                    SesCal(kosmaSesi);
                    kosmaSesiCalisiyor = true;
                }
                else
                {
                    SesCal(yurumeSesi);
                    yurumeSesiCalisiyor = true;
                }
            }
        }
    }

    void SesCal(AudioClip ses)
    {
        if (sesKaynagi.clip != ses)
        {
            sesKaynagi.clip = ses;
            sesKaynagi.Play();
        }
    }

    void SesBirKereCal(AudioClip ses)
    {
        if (sesKaynagi.clip != ses || !sesKaynagi.isPlaying)
        {
            sesKaynagi.clip = ses;
            sesKaynagi.Play();
        }
    }
}
