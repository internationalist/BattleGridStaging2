using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrenadePayload : Payload
{
    public float blastRadius;
    private AudioSource audioSource;
    private ParticleSystem fuse;
    GameObject impactFlash;
    bool exploded;
    [Tooltip("Game objects in this layer will be damaged by the grenade")]
    public int targetLayer;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        fuse = GetComponentInChildren<ParticleSystem>();
        impactFlash = transform.Find("ImpactLight").gameObject;
    }

    private void OnTriggerEnter(Collider collider)
    {
        if(!collider.gameObject.name.Equals(this.ownerObject.name) && !exploded)
        {
            exploded = true;
            Vector3 hitPoint = collider.gameObject.GetComponent<Collider>().ClosestPointOnBounds(transform.position);
            impactEffect.transform.position = hitPoint;
            StartCoroutine(PlayEffects());
            impactEffect.Play();
            StartCoroutine(AudioManager.TransitionToActionAndBack(.1f));
            audioSource.Play();
            fuse.Stop();
            StartCoroutine(Deactivate());
            Collider[] hitColliders = Physics.OverlapSphere(transform.position, blastRadius);
            foreach (var hitCollider in hitColliders)
            {
                PlayerController pc = hitCollider.gameObject.GetComponent<PlayerController>();

                if (pc != null)
                {
                    ParticleSystem bloodMist = Instantiate(pc.bloodMist,
                                       pc.transform.position + Vector3.up * .5f,
                                       Quaternion.identity);
                    int cnt = pc.bloodEffects.Count;
                    ParticleSystem bloodEffectPrfab = pc.bloodEffects[Random.Range(0, cnt)];
                    Instantiate(bloodEffectPrfab, pc.transform.position + Vector3.up * .5f, Quaternion.identity).Play();
                    bloodMist.Play();
                    pc.TakeDamage(damageParameters.baseDamage);
                }
            }
            this.ownerObject.AnimationComplete("grenade");
        }
    }

    private void OnEnable()
    {
        exploded = false;
    }

    IEnumerator PlayEffects()
    {
        impactFlash.SetActive(true);
        UIManager.StartCamShake();
        yield return new WaitForSeconds(0.5f);
        UIManager.StopCamShake();
        impactFlash.SetActive(false);
    }

    IEnumerator Deactivate()
    {
        yield return new WaitForSeconds(1);
        gameObject.SetActive(false);
    }




}
