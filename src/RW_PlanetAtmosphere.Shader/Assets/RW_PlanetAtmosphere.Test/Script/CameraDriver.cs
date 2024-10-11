using RW_PlanetAtmosphere;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace RW_PlanetAtmosphere
{

    [RequireComponent(typeof(Camera))]
    public class CameraDriver : MonoBehaviour
    {
        public bool showData = false;
        public float dragPlane = 64;
        public float heightLimitOffset = 1;
        public Light senceLight;
        private Vector2 mousePos;
        private Vector2 lightDir;
        private Vector3 viewPos;
        private Camera m_camera;
        private Transform m_transform;
        private Transform m_lightTransform;

        private void Start()
        {
            mousePos = Input.mousePosition;
        }
        private void Update()
        {
            m_camera = m_camera ?? gameObject.GetComponent<Camera>();
            m_transform = m_transform ?? transform;
            Vector2 mouseDir = Input.mousePosition;
            mouseDir -= mousePos;
            float toAng = m_camera.fieldOfView / m_camera.pixelHeight;
            if (Input.GetMouseButton(2))
            {
                float factor = (viewPos.z - dragPlane) / dragPlane;
                viewPos.x += mouseDir.x * toAng * factor;
                viewPos.y += mouseDir.y * toAng * factor;
            }
            viewPos.z -= Input.mouseScrollDelta.y*16;
            if (Mathf.Abs(viewPos.x) > 180)
            {
                viewPos.x = Mathf.Sign(viewPos.x) * ((Mathf.Abs(viewPos.x) % 180) - 180);
            }
            viewPos.y = Mathf.Clamp(viewPos.y, -89, 89);
            viewPos.z = Mathf.Clamp(viewPos.z, dragPlane + heightLimitOffset + m_camera.nearClipPlane, 800);

            m_transform.position = new Vector3(
                +Mathf.Sin(-viewPos.x * Mathf.PI / 180) * Mathf.Cos(viewPos.y * Mathf.PI / 180),
                -Mathf.Sin(+viewPos.y * Mathf.PI / 180),
                -Mathf.Cos(-viewPos.x * Mathf.PI / 180) * Mathf.Cos(viewPos.y * Mathf.PI / 180)
            ) * viewPos.z;
            m_transform.LookAt(Vector3.zero);

            if(senceLight)
            {
                m_lightTransform = m_lightTransform ?? senceLight.transform;
                m_lightTransform.position = Vector3.zero;
                if (Input.GetMouseButton(0))
                {
                    lightDir.x += mouseDir.x * toAng;
                    lightDir.y += mouseDir.y * toAng;
                }
                if (Mathf.Abs(lightDir.x) > 180)
                {
                    lightDir.x = Mathf.Sign(lightDir.x) * ((Mathf.Abs(lightDir.x) % 180) - 180);
                }
                lightDir.y = Mathf.Clamp(lightDir.y, -89, 89);

                m_lightTransform.LookAt(new Vector3(
                    -Mathf.Sin(-lightDir.x * Mathf.PI / 180) * Mathf.Cos(lightDir.y * Mathf.PI / 180),
                    -Mathf.Sin(+lightDir.y * Mathf.PI / 180),
                    +Mathf.Cos(-lightDir.x * Mathf.PI / 180) * Mathf.Cos(lightDir.y * Mathf.PI / 180)
                ));
            }
            mousePos = Input.mousePosition;
        }

        private void OnGUI()
        {
            if(showData)
            {
                GUI.Label(new Rect(0, 0, 128, 32), viewPos.ToString());
                GUI.Label(new Rect(0, 32, 128, 32), lightDir.ToString());
                GUI.Label(new Rect(0, 64, 128, 32), mousePos.ToString());
            }
        }
    }
}