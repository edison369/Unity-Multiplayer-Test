using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using TMPro;

public class Menu : MonoBehaviourPunCallbacks
{

    public TMP_InputField createInput;
    public TMP_InputField joinInput;
    public TMP_InputField nameInput;

    public void ChangeName()
    {
        PhotonNetwork.NickName = nameInput.text;
    }

    public void CreateRoom()
    {
        RoomOptions roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinedRoom()
    {
        // To load a scenes shared by users use PhotonNetwork.LoadLevel, not SceneManager.LoadScene
        PhotonNetwork.LoadLevel("Game"); // Add scene you want to load
    }
}
