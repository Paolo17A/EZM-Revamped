using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MyBox;
using System;
using System.Linq;
using PlayFab.ClientModels;

public class CharacterPrefabController : MonoBehaviour
{
    //===============================================================================================
    [SerializeField][ReadOnly] private Vector3 mouseVector;
    [SerializeField] private CharacterPrefabCore characterPrefabCore;
    //===============================================================================================

    private void OnEnable()
    {
        characterPrefabCore.onCharacterStateChange += CharacterStateChange;
    }
    private void OnDisable()
    {
        characterPrefabCore.onCharacterStateChange -= CharacterStateChange;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == 7 && collision.transform.GetComponent<OreController>().OreName == characterPrefabCore.AssignedOre.OreName)
        {
            Debug.Log("you have reached your destination");
            characterPrefabCore.CurrentCharacterState = CharacterPrefabCore.CharacterStates.WORKING;
        }
    }

    private void Awake()
    {
        characterPrefabCore.updateCharacterData = new UpdateCharacterDataRequest();
        characterPrefabCore.updateCharacterData.Data = new Dictionary<string, string>();
    }

    private void Start()
    {
        characterPrefabCore.CurrentCharacterState = CharacterPrefabCore.CharacterStates.IDLE;
    }
    private void FixedUpdate() 
    {
        mouseVector = GameManager.Instance.MainCamera.ScreenToWorldPoint(GameManager.Instance.InputManager.GetMousePosition());
        if (characterPrefabCore.CurrentCharacterState == CharacterPrefabCore.CharacterStates.WALKING)
        {
            if (Vector2.Distance(transform.position, characterPrefabCore.CharacterNavMesh.destination) <= Mathf.Epsilon)
            {
                characterPrefabCore.CharacterNavMesh.enabled = false;
                if(characterPrefabCore.willMineOre)
                {
                    characterPrefabCore.CurrentCharacterState = CharacterPrefabCore.CharacterStates.WORKING;
                }
                else
                    characterPrefabCore.CurrentCharacterState = CharacterPrefabCore.CharacterStates.IDLE;
            }
        }
    }

    private void CharacterStateChange(object sender, EventArgs e)
    {
        characterPrefabCore.CharacterAnimator.SetInteger("state", (int)characterPrefabCore.CurrentCharacterState);
        if(characterPrefabCore.CurrentCharacterState == CharacterPrefabCore.CharacterStates.WALKING)
            characterPrefabCore.CharacterNavMesh.destination = characterPrefabCore.destinationVector;

    }
}
