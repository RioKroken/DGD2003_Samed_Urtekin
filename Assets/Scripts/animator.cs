using UnityEngine;

public class animator : MonoBehaviour

{
    public Animator Animator { get; set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyUp("w"))
        {
            Animator.SetBool("iswalking", true);
        }
        else
            Animator.SetBool("iswalking", false);



    }
}
