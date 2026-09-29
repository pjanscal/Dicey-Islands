using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class VisualeEffectAnimatorSupport : MonoBehaviour
{
    /*how to use
    set it in the character prehabs
    assign all vfx
    when editing in animation what happend let it send a event in here*/

    [Serializable]
    private class VisualeTypeConfig
    {
        public string type;
        public ParticleSystem particleSystem;
    }

    private Dictionary<string, ParticleSystem> allParticales = new();

    //configs
    [SerializeField] private VisualeTypeConfig[] visualeTypeConfigs = new VisualeTypeConfig[0];


    void Start()
    {
        foreach (VisualeTypeConfig visualeTypeConfig in visualeTypeConfigs)
        {
            //debug for when editing
            #if UNITY_EDITOR
                if (allParticales.Keys.Contains(visualeTypeConfig.type)) {Debug.LogError("there can't be 2 of the same type"); continue;}
            #endif

            allParticales.Add(visualeTypeConfig.type, visualeTypeConfig.particleSystem);
        }
    }

    //start a effect
    public void StartVisualeEffect(string visualeType) //it can't have bool or enum
    {
        #if UNITY_EDITOR
            //debug if it was forgotted to add then it won't give error
            if (!allParticales.Keys.Contains(visualeType)) {Debug.LogError($"there is no partical with type of {visualeType} assign"); return;}
        #endif

        ParticleSystem newPartical = allParticales[visualeType];
        if (!newPartical.isPlaying) newPartical.Play();
    }

    //stop one specifike effect
    public void StopVisualeEffect(string visualeType)
    {
        #if UNITY_EDITOR
            //debug if it was forgotted to add then it won't give error
            if (!allParticales.Keys.Contains(visualeType)) {Debug.LogError($"there is no partical with type of {visualeType} assign"); return;}
        #endif

        ParticleSystem newPartical = allParticales[visualeType];
        if (newPartical.isPlaying) newPartical.Stop();
    }

    //stop all vfx that the char is playing
    public void StopAllVisualeEffect()
    {
        foreach (ParticleSystem particle in allParticales.Values)
        {
            if (particle.isPlaying) particle.Stop();
        }
    }
}
