﻿using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using GameCommon;
using MGFClient;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CharacterSelectView : MonoBehaviour {

    public void Awake()
    {
        RequestCharacterList();
    }

    public void RequestCharacterList()
    {
        OperationRequest request = new OperationRequest()
        {
            OperationCode = (byte)MessageOperationCode.Login,
            Parameters = new Dictionary<byte, object>()
                {{(byte) PhotonEngine.Instance.SubCodeParameterCode, MessageSubCode.CharacterList}}
        };
        Debug.Log("Sending request for character list");
        PhotonEngine.Instance.SendRequest(request);
    }

    public void CreateNewCharacter()
    {
        SceneManager.LoadScene("CharacterCreate");
    }
}
