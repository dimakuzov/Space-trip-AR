﻿//-----------------------------------------------------------------------
// <copyright file="CloudAnchorController.cs" company="Google">
//
// Copyright 2018 Google Inc. All Rights Reserved.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
//
// </copyright>
//-----------------------------------------------------------------------

namespace GoogleARCore.Examples.CloudAnchor
{
    using System.Collections;
    using System.Collections.Generic;
    using GoogleARCore;
    using GoogleARCore.CrossPlatform;
    using GoogleARCore.Examples.Common;
    using UnityEngine;
    using UnityEngine.UI;

#if UNITY_EDITOR
    // Set up touch input propagation while using Instant Preview in the editor.
    using Input = InstantPreviewInput;
#endif

    
public class GameController : MonoBehaviour {
       
        public CloudAnchorUIController UIController;

        private Player playerComp;
        
        private GameObject player;

        public MapGenerator mG;
        
        [Header("ARCore")]

        public GameObject ARCoreRoot;

        public GameObject ARCoreAndyAndroidPrefab;

        public GameObject playerARCore;

        public GameObject ARCoreCamera;
        
        [Header("ARKit")]

        public GameObject ARKitRoot;

        public Camera ARKitFirstPersonCamera;

        public GameObject ARKitAndyAndroidPrefab;

        public GameObject playerARKit;

       
    
        GameObject target;
        GameObject luna;
        GameObject solarSistem;
        bool activeGame = false;
        [Header("Game Options")]
        public GameObject pointPlayerPref;
        [Header("Canvas")]
        public GameObject buttonNewGame;

        public GameObject textDis;
        public Image fade;
        public GameObject gameoverText;

        private bool gameoverBool = false;

        
        
        private const float k_ModelRotation = 180.0f;

        private ARKitHelper m_ARKit = new ARKitHelper();

        private bool m_IsQuitting = false;

        private Component m_LastPlacedAnchor = null;

        private XPAnchor m_LastResolvedAnchor = null;
 
    
        public void Start()
        {
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                ARCoreRoot.SetActive(true);
                ARKitRoot.SetActive(false);
                player = playerARCore;
            }
            else
            {
                ARCoreRoot.SetActive(false);
                ARKitRoot.SetActive(true);
                player = playerARKit;
            }

            playerComp = player.GetComponent<Player>();
        }

        
        public void Update()
        {
            
            _UpdateApplicationLifecycle();
            if (activeGame && gameoverBool == false)
            {
                textDis.GetComponent<Text>().text = "Your health: " +
                                    Mathf.RoundToInt(player.GetComponent<Player>().health * 100.0f);
            }
            if (target)
            {
                target.transform.Rotate(Vector3.up * Time.deltaTime * -13.0f);
                luna.transform.Rotate(new Vector3(-0.35f, 0.8f, 0) * 14 * Time.deltaTime);
            }
            
            // If the player has not touched the screen then the update is complete.
            Touch touch;
            if (Input.touchCount < 1 || (touch = Input.GetTouch(0)).phase != TouchPhase.Began)
            {
                return;
            }

            // Raycast against the location the player touched to search for planes.
            if (Application.platform != RuntimePlatform.IPhonePlayer)
            {
                TrackableHit hit;
                if (Frame.Raycast(touch.position.x, touch.position.y,
                        TrackableHitFlags.PlaneWithinPolygon, out hit))
                {
                    m_LastPlacedAnchor = hit.Trackable.CreateAnchor(hit.Pose);
                }
            }
            else
            {
                Pose hitPose;
                if (m_ARKit.RaycastPlane(ARKitFirstPersonCamera, touch.position.x, touch.position.y, out hitPose))
                {
                    m_LastPlacedAnchor = m_ARKit.CreateAnchor(hitPose);
                }
            }

            if (m_LastPlacedAnchor != null)
            {
                InstantiateSolarSistem();
            }
        }

        void InstantiateSolarSistem()
        {
            if (activeGame != true)
            {
                if (solarSistem == null)
                {
                    float dis = Vector3.Distance(ARCoreCamera.GetComponent<Transform>().position,
                                                 m_LastPlacedAnchor.transform.position);
                    if (dis < 2.2f)
                    {
                        textDis.GetComponent<Text>().text = "This is very close...         Try again!";
                    }
                    else if (dis > 4.0f)
                    {
                        textDis.GetComponent<Text>().text = "Too far, Better distance: 2 - 4 meters";
                    }
                    else
                    {
                        // Instantiate Andy model at the hit pose.
                        var solarSistemObject = Instantiate(_GetAndyPrefab(), m_LastPlacedAnchor.transform.position,
                            m_LastPlacedAnchor.transform.rotation);

                        // Compensate for the hitPose rotation facing away from the raycast (i.e. camera).
                        solarSistemObject.transform.Rotate(0, k_ModelRotation, 0, Space.Self);

                        // Make Andy model a child of the anchor.
                        solarSistemObject.transform.parent = m_LastPlacedAnchor.transform;
                        StartActiveGame();
                        buttonNewGame.SetActive(true);
                        solarSistem = solarSistemObject;
                    }
                }
                else
                {
                    solarSistem.transform.position = m_LastPlacedAnchor.transform.position;
                    solarSistem.transform.rotation = m_LastPlacedAnchor.transform.rotation;
                }
            }
        }
        
        // ------------------------- START --------------------------

        public void StartActiveGame()
        {
            activeGame = true;
            player.SetActive(true);
            target = GameObject.FindWithTag("Target");
            luna = GameObject.FindWithTag("Luna");
            
            playerComp.target = target.GetComponent<Transform>();
            playerComp.AdventPlayer();
            GameObject playerForGM = Instantiate(pointPlayerPref, ARCoreCamera.transform.position
                                                , ARCoreCamera.transform.rotation);
            mG.playerTrans = playerForGM.transform;
            mG.startTrans = target.transform;
            mG.GenerateMap();
            StartCoroutine(PlanetIncrease(target));
        }

        IEnumerator PlanetIncrease(GameObject planet)
        {
            float i = 0;
            while (i < 1)
            {
                luna.transform.localScale = Vector3.Lerp(Vector3.one,
                    Vector3.one * 13.0f, i);
                planet.transform.localScale = Vector3.Lerp(Vector3.one,
                    Vector3.one * 13.0f, i);
                i += (1.0f / (mG.maxLevels - 1)) /
                     (mG.timeOneLevel / Time.deltaTime);
                yield return null;
            }

            yield return null;
        }
        
        public void NewGame()
        {
            Application.LoadLevel("SpaceTripAR");
        }

        public void PlayerWin()
        {
            textDis.GetComponent<Text>().text = " You are winner!";
            gameoverBool = true;
        }
        
        public void GameOver()
        {
            StartCoroutine(Fade());
            gameoverBool = true;
        }

        IEnumerator Fade()
        {
            if (!gameoverBool)
            {
                float i = 0;
                Color color = Color.black;
                float a = 0;
                fade.gameObject.SetActive(true);
                float timeFade = 3.3f;
                while (i < timeFade)
                {
                    i += Time.deltaTime;
                    a += Time.deltaTime / timeFade;
                    color.a = a;
                    fade.color = color;
                    yield return null;
                }

                fade.color = Color.black;
                gameoverText.SetActive(true);
            }
            yield break;
        }
        
        
        private GameObject _GetAndyPrefab()
        {
            return Application.platform != RuntimePlatform.IPhonePlayer ?
                ARCoreAndyAndroidPrefab : ARKitAndyAndroidPrefab;
        }

        private void _UpdateApplicationLifecycle()
        {
            // Exit the app when the 'back' button is pressed.
            if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }

            var sleepTimeout = SleepTimeout.NeverSleep;

#if !UNITY_IOS
            // Only allow the screen to sleep when not tracking.
            if (Session.Status != SessionStatus.Tracking)
            {
                const int lostTrackingSleepTimeout = 15;
                sleepTimeout = lostTrackingSleepTimeout;
            }
#endif

            Screen.sleepTimeout = sleepTimeout;

            if (m_IsQuitting)
            {
                return;
            }

            // Quit if ARCore was unable to connect and give Unity some time for the toast to appear.
            if (Session.Status == SessionStatus.ErrorPermissionNotGranted)
            {
                _ShowAndroidToastMessage("Camera permission is needed to run this application.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
            else if (Session.Status.IsError())
            {
                _ShowAndroidToastMessage("ARCore encountered a problem connecting.  Please start the app again.");
                m_IsQuitting = true;
                Invoke("_DoQuit", 0.5f);
            }
        }

        private void _DoQuit()
        {
            Application.Quit();
        }

        private void _ShowAndroidToastMessage(string message)
        {
            AndroidJavaClass unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject unityActivity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");

            if (unityActivity != null)
            {
                AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast");
                unityActivity.Call("runOnUiThread", new AndroidJavaRunnable(() =>
                {
                    AndroidJavaObject toastObject = toastClass.CallStatic<AndroidJavaObject>("makeText", unityActivity,
                        message, 0);
                    toastObject.Call("show");
                }));
            }
        }
    }
}
