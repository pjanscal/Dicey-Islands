using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CPUMangeren : MonoBehaviour
{
    [Serializable]
    protected class CPUCharacter
    {
        public int plrId;
        public GameObject character;
    }

    [SerializeField] List<CPUCharacter> cpuCharacters = new(4);
    protected Dictionary<int, GameObject> plrsChar = new();
    protected float secBeforeReadyUp = .2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected virtual void Start()
    {
        foreach (CPUCharacter cpuCharacter in cpuCharacters)
        {
            plrsChar.Add(cpuCharacter.plrId, cpuCharacter.character);
        }

        //setup the first time
        StartCoroutine(GetAllGpuStart());

        //if (plrsChar.Count != 4) Debug.LogWarning("make sure that there is 4 plrid inside the cpucharacters the gameobject can just be empty :3");
    }

    protected IEnumerator GetAllGpuStart()
    {
        yield return new WaitUntil(() => GameMangeren.inGame && LokaalConnecter.connectionType == LokaalConnecter.ConnectionTypes.nothing);

        foreach (int plrId in GameMangeren.allCPU)
        {
            StartCoroutine(CPUStart(plrId));
        }
    }

    protected virtual IEnumerator CPUStart(int plrId)
    {
        yield return null;
    }

    // Update is called once per frame
    protected virtual void Update()
    {
        //go back if it is not valid
        if (!GameMangeren.inGame || GameMangeren.isPaused || LokaalConnecter.connectionType != LokaalConnecter.ConnectionTypes.nothing) return;

        foreach (int plrId in GameMangeren.allCPU)
        {
            CpuUpdate(plrId);
        }
    }

    protected virtual void CpuUpdate(int plrId)
    {
        
    }
}
