﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SpectralDaze.Camera
{
    public class CameraFunctions : MonoBehaviour
    {
        readonly float _originalFOV = UnityEngine.Camera.main.fieldOfView;
        private void Update()
        {
            /*
            if (Input.GetKeyDown(KeyCode.E))
            {
                StartCoroutine(FOVKick(2.0f, 0.2f));
            }
            */
        }

        public void Shake(float duration, float magnitude)
        {
            StartCoroutine(shake(duration,magnitude));
        }

        IEnumerator shake(float duration, float magnitude)
        {
            Vector3 orignalPosistion = transform.localPosition;
            float elapsedTime = 0.0f;
            while (elapsedTime < duration)
            {
                float x = Random.Range(-1, 1) * magnitude;
                float z = Random.Range(-1, 1) * magnitude;
                transform.localPosition = orignalPosistion + new Vector3(x, orignalPosistion.y, z);
                elapsedTime += UnityEngine.Time.deltaTime;
                yield return null;
            }

            transform.localPosition = orignalPosistion;
        }

        public void FOVKick(float fovOffset, float time)
        {
            StartCoroutine(fovKick(fovOffset, time));
        }

        IEnumerator fovKick(float fovOffset, float time)
        {

            var t = 0.0f;
            var t2 = 0.0f;
            while (t < time / 2)
            {
                UnityEngine.Camera.main.fieldOfView = Mathf.Lerp(UnityEngine.Camera.main.fieldOfView,
                    UnityEngine.Camera.main.fieldOfView + fovOffset, t2);
                t += UnityEngine.Time.deltaTime;
                t2 += UnityEngine.Time.deltaTime / time;
                yield return new WaitForEndOfFrame();
            }
            t = 0;
            t2 = 0.0f;
            while (t < time / 2)
            {
                UnityEngine.Camera.main.fieldOfView = Mathf.Lerp(UnityEngine.Camera.main.fieldOfView,
                    _originalFOV, t2);
                t += UnityEngine.Time.deltaTime;
                t2 += UnityEngine.Time.deltaTime / time;
                yield return new WaitForEndOfFrame();
            }

            UnityEngine.Camera.main.fieldOfView = _originalFOV;
        }
    }
}
