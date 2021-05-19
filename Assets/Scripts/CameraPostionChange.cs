using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraPostionChange : MonoBehaviour
{
    public List<Transform> camTransformList;
    Camera _camera;

    Coroutine _coroutine;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad0))
        {
            GoToNewTransform(0, 90);

        }
        else if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            GoToNewTransform(1,90);

        }
        else if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            GoToNewTransform(2,120);
        }

    }

    public void OnClick_CamBtn1()
    {
         GoToNewTransform(0, 90);
    }
    public void OnClick_CamBtn2()
    {
        GoToNewTransform(1, 90);
    }
    public void OnClick_CamBtn3()
    {
        GoToNewTransform(2, 120);
    }

    void GoToNewTransform(int index, float targetFoV)
    {
        if (camTransformList[index] != null)
        {
            if (_coroutine != null)
                StopCoroutine(_coroutine);

            _coroutine = StartCoroutine(BlendToNew(camTransformList[index],targetFoV));
        }
    }
    IEnumerator BlendToNew(Transform target, float targetFoV, float speed = 2f)
    {
        while (true)
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFoV, speed * Time.deltaTime);
            transform.position = Vector3.Lerp(transform.position, target.position, speed * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, target.rotation, speed * Time.deltaTime);
            yield return null;
        }
    }
}
