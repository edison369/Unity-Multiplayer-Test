# How to create a multi-user experience?
This guide will help create a multi-user experience using [Photon Unity Networking (PUN)](https://doc.photonengine.com/pun/current/getting-started/pun-intro) components and API.
The project has 3 scenes:
1. [**Loading**](#loading-scene): Makes the connection to the PUN server
2. [**Menu**](#menu-scene): UI that allows the user to select a user name and create or join a room in the server
3. [**Game**](#game-scene): The game where all the users interact

## Loading Scene
This is a scene that will display a "Loading..." message to the user while the app establishes the connection to the PUN server. To do it, this scene has a gameobject with a script attached that has the follwoing code:
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class ConnectServer : MonoBehaviourPunCallbacks
{
    // Start is called before the first frame update
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        SceneManager.LoadScene("Menu");
    }
}
```
This code will try to establish a connection to the server. If the client is connected to the Master Server and ready for matchmaking the `OnConnectedToMaster()` will be called to load the [Menu](#menu-scene) scene.

## Menu Scene
This scene will display a menu that allows the user to type a user name and a room name to create or join. To do it, this scene has a gameobject with a script attached that has the follwoing code:
```c#
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
    private RoomOptions roomOptions;
    private bool errorCount;

    void Start()
    {
        roomOptions = new RoomOptions();
        roomOptions.MaxPlayers = 2;

        errorCount = false;
    }

    public void ChangeName()
    {
        PhotonNetwork.NickName = nameInput.text;
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(createInput.text, roomOptions);
    }

    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(joinInput.text);
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if (!errorCount)
        {
            Debug.Log("No room available, so we create one.\nCalling: PhotonNetwork.CreateRoom");
            errorCount = true;
            PhotonNetwork.CreateRoom(joinInput.text, roomOptions);
        }
        else 
        {
            Debug.Log("You should change the name of the room");
        }
        
    }

    public override void OnJoinedRoom()
    {
        // To load a scenes shared by users use PhotonNetwork.LoadLevel, not SceneManager.LoadScene
        PhotonNetwork.LoadLevel("Game"); // Add scene you want to load
    }

}
```
The methods `CreateRoom()` and `JoinRoom()` can be attached to some buttons `OnClicked()` event. That way when the user clicks the button the methods will be called. 
The same way the method `ChangeName()` can be attached to the username's InputField `OnValueChanged(String)` event.
If the user cannot join the room (most problably, the room does not exist) then the `OnJoinRoomFailed()` will be called and the app will try to create the room. If the client succesfully joins (or creates) the room then the [Game](#game-scene) scene will be loaded via `OnJoinedRoom()`. 

## Game Scene
This is the scene where the multi-user experience happens! This scene can have two types of objects:
- **Room GameObjects**: GameObjects that can be manipulated by multiple users depending on the ownership of the object.
- **Player GameObjects**: GameObjects that can be manipulated only by the user that instantiated it.
For this projects, both types of objects use an empty GameObject with a script component that instantiates the objects in a random location. The script for the instantiation of the objects has the following general structure:
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class SpawnObjects : MonoBehaviour
{
    public GameObject object;
    public float minX, minY, minZ, maxX, maxY, maxZ;

    // Start is called before the first frame update
    void Start()
    {
        Vector3 randomPosition = new Vector3(Random.Range(minX, maxX), Random.Range(minY, maxY), Random.Range(minZ, maxZ));
        //******** HERE GOES THE INSTANTIATION ********
    }
}

```
For both type of GameObjects, they need the following components:
- **Photon View**: Allows to determine the ownership of the object.
- **Photon Transform View Classic**: Allows to synchronize position, rotation and scale of the object between the users.
- Object Controller Script
After adding this components, the GameObjects need to be saved as prefabs in the **Resources** directory, that way they can be instantiated.
### Room GameObjects
In the case of the room objects, the instantiation method is:
```c#
PhotonNetwork.InstantiateRoomObject(roomObject.name, randomPosition, Quaternion.identity);
```
Once the object is instantiated, since we want to allow multiple users to manipulate them, we need to manage the ownership of the object with the **Object Controller Script**:
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using MixedReality.Toolkit.SpatialManipulation;

public class RoomObjectController : MonoBehaviour
{
    PhotonView view;
    private bool isManipulated = false;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    public void LockManipulation()
    {
        if (!view.IsMine && !isManipulated)
        {
            view.RequestOwnership();
        }
        isManipulated = true;
    }

    public void UnlockManipulation()
    {
        isManipulated = false;
    }
}

```
In this case, the `LockManipulation()` and `UnlockManipulation()` methods were attached to `Manipulation Started` and `Manipulation Ended` events.

### Player GameObjects
In the case of the room objects, the instantiation method is:
```c#
PhotonNetwork.Instantiate(playerObject.name, randomPosition, Quaternion.identity);
```
Once the object is instantiated, since we want to allow only the owner to manipulate them, we need to enable the Object Manipulator component:
```c#
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using MixedReality.Toolkit.SpatialManipulation;

public class ObjectController : MonoBehaviour
{
    PhotonView view;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
    }

    // Update is called once per frame
    void Update()
    {
        if(view.IsMine)
        {
            GetComponent<ObjectManipulator>().enabled = true;
        }
    }
}
```
