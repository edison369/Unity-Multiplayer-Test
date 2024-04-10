using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    public float speed;
    PhotonView view;
    Animator animator;

    public TMP_Text nameDisplay;

    // Start is called before the first frame update
    void Start()
    {
        view = GetComponent<PhotonView>();
        animator = GetComponent<Animator>();

        if (view.IsMine)
        {
            nameDisplay.text = PhotonNetwork.NickName;
        }
        else
        {
            nameDisplay.text = view.Owner.NickName;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // To ensure the local player moves the prefab check the view
        if(view.IsMine)
        {
            Vector2 moveInput = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            Vector2 moveAmount = moveInput.normalized * speed * Time.deltaTime;
            transform.position += (Vector3)moveAmount;

            if (moveInput.x == 0)
            {
                animator.SetBool("left", false);
                animator.SetBool("right", false);
            }
            else if (moveInput.x == -1)
            {
                animator.SetBool("left", true);
            }
            else
            {
                animator.SetBool("right", true);
            }

            if (moveInput.y == 0)
            {
                animator.SetBool("up", false);
                animator.SetBool("down", false);
            }
            else if (moveInput.y == -1)
            {
                animator.SetBool("down", true);
            }
            else
            {
                animator.SetBool("up", true);
            }

        }
    }
}
