using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour{
    private Animator animator;
    [SerializeField] private Player player;

    private void Start() {
        animator = GetComponent<Animator>();
    }

    private void Update() {
        animator.SetBool("IsWalking", player.IsWalking());
    }
}
