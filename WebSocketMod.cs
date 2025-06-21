using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BirdieBoosters.Core
{
    [BepInPlugin("com.birdieboosters.websocket", "WebSocket Mod", "1.0.0")]
    public class WebSocketMod : BaseUnityPlugin
    {
        private static ManualLogSource LoggerInstance;
        private static ClientWebSocket webSocket;
        private static CancellationTokenSource cancellationTokenSource;
        private static Task webSocketTask;
        private static bool isConnected = false;
        private static readonly object messageLock = new object();
        private static readonly Queue<string> messageHistory = new Queue<string>();
        private static TextMeshProUGUI messageDisplay;
        private static GameObject messageDisplayObject;
        
        // WebSocket configuration
        private const string WebSocketUri = "ws://192.168.1.42:3000/ws";
        private const int MaxMessageHistory = 5;
        private const int ReconnectDelayMs = 5000;

        // Mobile input data structure
        [Serializable]
        public class MobileInput
        {
            public string eventType;
            public object payload;
        }

        [Serializable]
        public class PayloadData
        {
            public float x;
            public float y;
            public string button;
            public string state; // pressed, held, released
        }

        private void Awake()
        {
            LoggerInstance = Logger;
            Logger.LogInfo("[WebSocket Mod] Initializing WebSocket plugin...");

            // Apply Harmony patches
            var harmony = new Harmony("com.birdieboosters.websocket");
            harmony.PatchAll();

            // Start WebSocket connection
            StartWebSocketConnection();
        }

        private void Start()
        {
            // Create UI display for messages
            CreateMessageDisplay();
        }

        private void CreateMessageDisplay()
        {
            try
            {
                // Find the main canvas
                Canvas mainCanvas = FindObjectOfType<Canvas>();
                if (mainCanvas == null)
                {
                    Logger.LogWarning("[WebSocket Mod] No Canvas found, creating one...");
                    GameObject canvasObj = new GameObject("MainCanvas");
                    mainCanvas = canvasObj.AddComponent<Canvas>();
                    mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvasObj.AddComponent<CanvasScaler>();
                    canvasObj.AddComponent<GraphicRaycaster>();
                }

                // Create the message display object
                messageDisplayObject = new GameObject("WebSocketMessageDisplay");
                messageDisplayObject.transform.SetParent(mainCanvas.transform, false);

                // Add TextMeshProUGUI component
                messageDisplay = messageDisplayObject.AddComponent<TextMeshProUGUI>();
                messageDisplay.text = "WebSocket: Connecting...";
                messageDisplay.fontSize = 18;
                messageDisplay.color = Color.white;
                messageDisplay.alignment = TextAlignmentOptions.TopLeft;
                messageDisplay.font = Resources.Load<TMPro.TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");

                // Position the text in the top-left corner
                RectTransform rectTransform = messageDisplay.GetComponent<RectTransform>();
                rectTransform.anchorMin = new Vector2(0, 1);
                rectTransform.anchorMax = new Vector2(0, 1);
                rectTransform.pivot = new Vector2(0, 1);
                rectTransform.anchoredPosition = new Vector2(10, -10);
                rectTransform.sizeDelta = new Vector2(400, 200);

                Logger.LogInfo("[WebSocket Mod] Message display created successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[WebSocket Mod] Failed to create message display: {ex.Message}");
            }
        }

        private void StartWebSocketConnection()
        {
            cancellationTokenSource = new CancellationTokenSource();
            webSocketTask = Task.Run(() => WebSocketLoop(cancellationTokenSource.Token));
        }

        private async Task WebSocketLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    using (webSocket = new ClientWebSocket())
                    {
                        Logger.LogInfo($"[WebSocket Mod] Connecting to {WebSocketUri}...");
                        
                        await webSocket.ConnectAsync(new Uri(WebSocketUri), cancellationToken);
                        isConnected = true;
                        
                        Logger.LogInfo("[WebSocket Mod] Connected successfully!");
                        UpdateMessageDisplay("WebSocket: Connected");

                        // Message receiving loop
                        var buffer = new byte[4096];
                        while (webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
                        {
                            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

                            if (result.MessageType == WebSocketMessageType.Text)
                            {
                                string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                                ProcessMessage(message);
                            }
                            else if (result.MessageType == WebSocketMessageType.Close)
                            {
                                Logger.LogInfo("[WebSocket Mod] WebSocket closed by server");
                                break;
                            }
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    Logger.LogInfo("[WebSocket Mod] WebSocket operation cancelled");
                    break;
                }
                catch (Exception ex)
                {
                    Logger.LogError($"[WebSocket Mod] WebSocket error: {ex.Message}");
                }

                isConnected = false;
                UpdateMessageDisplay("WebSocket: Disconnected");

                // Wait before attempting to reconnect
                if (!cancellationToken.IsCancellationRequested)
                {
                    Logger.LogInfo($"[WebSocket Mod] Attempting to reconnect in {ReconnectDelayMs}ms...");
                    await Task.Delay(ReconnectDelayMs, cancellationToken);
                }
            }
        }

        private void ProcessMessage(string message)
        {
            try
            {
                Logger.LogInfo($"[WebSocket Mod] Received: {message}");

                // Parse JSON message
                MobileInput mobileInput = JsonConvert.DeserializeObject<MobileInput>(message);
                
                if (mobileInput != null)
                {
                    string displayMessage = $"Type: {mobileInput.eventType}";
                    
                    if (mobileInput.payload != null)
                    {
                        var payload = JsonConvert.DeserializeObject<PayloadData>(mobileInput.payload.ToString());
                        if (payload != null)
                        {
                            displayMessage += $"\nX: {payload.x:F2}, Y: {payload.y:F2}";
                            if (!string.IsNullOrEmpty(payload.button))
                            {
                                displayMessage += $"\nButton: {payload.button} ({payload.state})";
                            }
                        }
                    }

                    // Add to message history
                    lock (messageLock)
                    {
                        messageHistory.Enqueue(displayMessage);
                        while (messageHistory.Count > MaxMessageHistory)
                        {
                            messageHistory.Dequeue();
                        }
                    }

                    // Update display on main thread
                    UpdateMessageDisplay(displayMessage);
                }
            }
            catch (JsonException ex)
            {
                Logger.LogError($"[WebSocket Mod] JSON parsing error: {ex.Message}");
            }
            catch (Exception ex)
            {
                Logger.LogError($"[WebSocket Mod] Message processing error: {ex.Message}");
            }
        }

        private void UpdateMessageDisplay(string message)
        {
            // Ensure we're on the main thread for UI updates
            if (Application.isPlaying)
            {
                UnityMainThreadDispatcher.Instance.Enqueue(() =>
                {
                    try
                    {
                        if (messageDisplay != null)
                        {
                            lock (messageLock)
                            {
                                string displayText = $"WebSocket Status: {(isConnected ? "Connected" : "Disconnected")}\n\n";
                                displayText += "Recent Messages:\n";
                                
                                foreach (string msg in messageHistory)
                                {
                                    displayText += $"{msg}\n\n";
                                }
                                
                                messageDisplay.text = displayText;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"[WebSocket Mod] Failed to update display: {ex.Message}");
                    }
                });
            }
        }

        private void OnDestroy()
        {
            // Cleanup
            cancellationTokenSource?.Cancel();
            webSocket?.Dispose();
            Logger.LogInfo("[WebSocket Mod] Plugin destroyed");
        }
    }

    // Helper class to dispatch actions to the main thread
    public class UnityMainThreadDispatcher : MonoBehaviour
    {
        private static UnityMainThreadDispatcher instance;
        private readonly Queue<Action> executionQueue = new Queue<Action>();

        public static UnityMainThreadDispatcher Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<UnityMainThreadDispatcher>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("UnityMainThreadDispatcher");
                        instance = go.AddComponent<UnityMainThreadDispatcher>();
                        DontDestroyOnLoad(go);
                    }
                }
                return instance;
            }
        }

        public void Enqueue(Action action)
        {
            lock (executionQueue)
            {
                executionQueue.Enqueue(action);
            }
        }

        private void Update()
        {
            lock (executionQueue)
            {
                while (executionQueue.Count > 0)
                {
                    executionQueue.Dequeue().Invoke();
                }
            }
        }
    }
} 