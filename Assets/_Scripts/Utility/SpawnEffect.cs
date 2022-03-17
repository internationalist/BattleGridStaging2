using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEffect : MonoBehaviour {

    public float spawnEffectTime = 2;
    public float pause = 1;
    public AnimationCurve fadeIn;
    public Material spawnMaterial;
    Material cachedMaterial;

    ParticleSystem ps;
    float timer = 0;
    Renderer _renderer;

    int shaderProperty;

	void Start ()
    {
        shaderProperty = Shader.PropertyToID("_cutoff");
        _renderer = GetComponent<Renderer>();
        ps = GetComponentInChildren <ParticleSystem>();
        cachedMaterial = _renderer.material;
        _renderer.material = spawnMaterial;
        Color c = _renderer.material.color;
        c.a = 0;
        _renderer.material.color = c;
        var main = ps.main;
        main.duration = spawnEffectTime;
        ps.Play();

    }
	
	void Update ()
    {
        if (timer < spawnEffectTime + pause)
        {
            timer += Time.deltaTime;

            Color c = _renderer.material.color;
            c.a = Mathf.Lerp(0, 1, timer / (spawnEffectTime));
            //c.a = fadeIn.Evaluate(Mathf.InverseLerp(0, spawnEffectTime, timer));
            _renderer.material.color = c;

            
            //_renderer.material.SetFloat(shaderProperty, fadeIn.Evaluate(Mathf.InverseLerp(0, spawnEffectTime, timer)));
        } else
        {
            _renderer.material = cachedMaterial;
        }
    }
}
