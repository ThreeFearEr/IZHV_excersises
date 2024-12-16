using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary> Helper behavior which manages the debug UI. </summary>
public class DebugMenuUI : MonoBehaviour
{
#region Editor

    [ Header("Global") ]
    [Tooltip("Display the debug UI?")]
    public bool displayUI;
    
#endregion // Editor

#region Internal

    /// <summary> Dimensions of the main window. </summary>
    private static Vector2 WINDOW_DIMENSION = new Vector2(256.0f, 192.0f);
    /// <summary> Base padding used within the UI. </summary>
    private static float BASE_PADDING = 8.0f;

    /// <summary> Rectangle representing the screen drawing area. </summary>
    private Rect mScreenRect;
    /// <summary> Rectangle representing the main window. </summary>
    private Rect mMainWindowRect;

    /// <summary> Dummy value used for demonstration. </summary>
    private float mDummyValue = 0.0f;
    
#endregion // Internal

#region Interface

#endregion // Interface

    /// <summary> Called when the script instance is first loaded. </summary>
    private void Awake()
    { }

    /// <summary> Called before the first frame update. </summary>
    void Start()
    {
        // Deduce the drawing screen area from the main camera.
        var mainCamera = GameSettings.Instance.mainCamera;
        mScreenRect = new Rect(
            mainCamera.rect.x * Screen.width, 
            mainCamera.rect.y * Screen.height, 
            mainCamera.rect.width * Screen.width, 
            mainCamera.rect.height * Screen.height
        );
        // Initially place the debug window into the top right corner.
        mMainWindowRect = new Rect(
            mScreenRect.x + mScreenRect.width - WINDOW_DIMENSION.x, mScreenRect.y, 
            WINDOW_DIMENSION.x, WINDOW_DIMENSION.y
        );
    }

    /// <summary> Update called once per frame. </summary>
    void Update()
    { }

    /// <summary> Called when GUI drawing should be happening. </summary>
    private void OnGUI()
    {
        if (displayUI)
        {
            mMainWindowRect = GUI.Window(0, mMainWindowRect, MainWindowUI, "Cheat Console");
            mMainWindowRect.x = Mathf.Clamp(
                mMainWindowRect.x, mScreenRect.x, 
                mScreenRect.x + mScreenRect.width - WINDOW_DIMENSION.x
            );
            mMainWindowRect.y = Mathf.Clamp(
                mMainWindowRect.y, mScreenRect.y, 
                mScreenRect.y + mScreenRect.height - WINDOW_DIMENSION.y
            );
        }
    }

    /// <summary> Function used for drawing the main window. </summary>
    private void MainWindowUI(int windowId)
    {
        GUILayout.BeginArea(new Rect(
            BASE_PADDING, 2.0f * BASE_PADDING, 
            WINDOW_DIMENSION.x - 2.0f * BASE_PADDING, 
            WINDOW_DIMENSION.y - 3.0f * BASE_PADDING
        ));
        {
            GUILayout.BeginVertical();
            {
                // Task 3b
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Currency: ", GUILayout.Width(WINDOW_DIMENSION.x / 4.0f));
                    var currency = InventoryManager.Instance.availableCurrency;
                    currency = (int)GUILayout.HorizontalSlider(currency, 0.0f, 1000.0f, GUILayout.ExpandWidth(true));
                    if (GUI.changed)
                    {
                        InventoryManager.Instance.availableCurrency = currency;
                    }
                }
                GUILayout.EndHorizontal();

                // Task 3c
                // 1. Interactive Mode toggle
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Interactive Mode: ");
                    GameManager.Instance.interactiveMode = GUILayout.Toggle(GameManager.Instance.interactiveMode, "Enabled");
                }
                GUILayout.EndHorizontal();

                // 2. Master Volume Slider
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Master Volume: ", GUILayout.Width(WINDOW_DIMENSION.x / 4.0f));
                    var volume = SoundManager.Instance.masterVolume;
                    volume = GUILayout.HorizontalSlider(volume, -80.0f, 20.0f, GUILayout.ExpandWidth(true));
                    if (GUI.changed)
                    {
                        SoundManager.Instance.masterVolume = volume;
                    }
                }
                GUILayout.EndHorizontal();

                // 3. Mute Toggle
                GUILayout.BeginHorizontal();
                {
                    GUILayout.Label("Mute: ");
                    SoundManager.Instance.masterMuted = GUILayout.Toggle(SoundManager.Instance.masterMuted, "Muted");
                }
                GUILayout.EndHorizontal();

                // Task 3a
                GUILayout.BeginHorizontal();
                {
                    for (var iii = 1; iii <= 10; ++iii)
                    { 
                        mDummyValue = GUILayout.VerticalSlider(
                            mDummyValue, 0.0f, 10.0f * iii, 
                            GUILayout.ExpandHeight(true)
                        );
                    }

                    if (GUILayout.Button("Enable\nDummy\nCharacter", 
                        GUILayout.ExpandWidth(true), 
                        GUILayout.ExpandHeight(true)))
                    { 
                        GameManager.Instance.TogglePlayerCharacter();
                    }
                }
                GUILayout.EndHorizontal();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndArea();
        
        GUI.DragWindow(new Rect(
            2.0f * BASE_PADDING, 0.0f,
            WINDOW_DIMENSION.x - 4.0f * BASE_PADDING, 
            WINDOW_DIMENSION.y
        ));
    }

}
