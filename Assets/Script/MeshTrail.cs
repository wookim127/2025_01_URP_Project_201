using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTrail : MonoBehaviour
{
    public float activeTime = 2.0f;
    public MovementInput moveScript;
    public float speedBoost = 6;
    public Animator animator;
    public float animSpeedBoost = 1.5f;

    [Header("Mesh Related")]
    public float meshBefreshRate = 1.0f;
    public float meshDestroyDelay = 3.0f;
    public Transform positionToSpawn;

    [Header("Shader Related")]
    public Material mat;
    public string shaderVerRef;
    public float shaderVarRate = 0.1f;
    public float shaderVarRetreshRate = 0.05f;

    private SkinnedMeshRenderer[] skinnedRenderer;
    private bool isTrailActive;

    private float normalSpeed;
    private float normalAnimSpeed;

    // Coroutine to animate material transparency
    IEnumerator AnimateMaterialFloat(Material m, float valueGoal, float rate, float refreshRate)
    {
        float valueToAnimate = m.GetFloat(shaderVerRef);

        // Reduce value to reach target value
        while (valueToAnimate > valueGoal)
        {
            valueToAnimate -= rate;
            m.SetFloat(shaderVerRef, valueToAnimate);
            yield return new WaitForSeconds(refreshRate);
        }
    }

    IEnumerator ActivateTrail(float timeActivated)   // Activating the trail effect
    {
        normalSpeed = moveScript.movementSpeed;      // Save the current speed and apply the boosted speed
        moveScript.movementSpeed = speedBoost;

        normalAnimSpeed = animator.GetFloat("animSpeed");    // Save the current animation speed and apply the boosted speed
        animator.SetFloat("animSpeed", animSpeedBoost);

        while (timeActivated > 0)
        {
            timeActivated -= meshBefreshRate;

            if (skinnedRenderer == null || skinnedRenderer.Length == 0)
                skinnedRenderer = positionToSpawn.GetComponentsInChildren<SkinnedMeshRenderer>(); // Fetch SkinnedMeshRenderer components

            for (int i = 0; i < skinnedRenderer.Length; i++)
            {
                GameObject g0bj = new GameObject();
                g0bj.transform.SetPositionAndRotation(positionToSpawn.position, positionToSpawn.rotation);

                MeshRenderer mr = g0bj.AddComponent<MeshRenderer>();
                MeshFilter mf = g0bj.AddComponent<MeshFilter>();

                Mesh m = new Mesh();
                skinnedRenderer[i].BakeMesh(m); // Bake the mesh of the current SkinnedMeshRenderer
                mf.mesh = m;
                mr.material = mat;

                StartCoroutine(AnimateMaterialFloat(mr.material, 0, shaderVarRate, shaderVarRetreshRate));

                Destroy(g0bj, meshDestroyDelay);  // Destroy the object after the given delay
            }

            yield return new WaitForSeconds(meshBefreshRate); // Wait for the specified refresh rate before next iteration
        }

        // Reset the speed and animation after the trail deactivates
        moveScript.movementSpeed = normalSpeed;
        animator.SetFloat("animSpeed", normalAnimSpeed);
        isTrailActive = false;
    }


    // Start is called before the first frame update
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && !isTrailActive) //스페이스바를 누르고 현재 잔상 효과가 비활성화 일 때
        {
            isTrailActive = true;
            StartCoroutine(ActivateTrail(activeTime)); //잔상 효과 코루틴 시작
        }

    }
}

    // Update is called once per frame
   