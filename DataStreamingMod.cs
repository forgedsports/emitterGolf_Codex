using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.IO;
using Assets.Scripts.Helpers.DataAndAnalytics;
using Assets.Scripts.Helpers.DataAndAnalytics.Models;
using UnityEngine;
using UnityEngine.Networking;
using Newtonsoft.Json;
using System.Linq;

namespace BirdieBoosters.DataStreaming
{
    public class GameEvent
    {
        [JsonProperty("event_type")]
        public string EventType { get; set; }

        [JsonProperty("session_id")]
        public int SessionId { get; set; }

        [JsonProperty("metadata")]
        public GameMetadata Metadata { get; set; }

        public GameEvent()
        {
            Metadata = new GameMetadata();
        }
    }

    public class XYPosition
    {
        [JsonProperty("x")]
        public int X { get; set; }

        [JsonProperty("y")]
        public int Y { get; set; }

        public XYPosition() { }

        public XYPosition(Vector2 vector)
        {
            X = Mathf.RoundToInt(vector.x);
            Y = Mathf.RoundToInt(vector.y);
        }
    }

    public class GameMetadata
    {
        [JsonProperty("match_id")]
        public string MatchId { get; set; }

        [JsonProperty("timestamp")]
        public string Timestamp { get; set; }

        [JsonProperty("game_details")]
        public GameDetails GameDetails { get; set; }

        [JsonProperty("players")]
        public List<PlayerInfo> Players { get; set; }

        [JsonProperty("sub_type")]
        public string SubType { get; set; }

        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("details")]
        public EventDetails Details { get; set; }

        public GameMetadata()
        {
            Players = new List<PlayerInfo>();
            GameDetails = new GameDetails();
        }
    }

    public class GameDetails
    {
        [JsonProperty("game_id")]
        public int GameId { get; set; }

        [JsonProperty("course_details")]
        public CourseDetails CourseDetails { get; set; }

        [JsonProperty("final_scores")]
        public List<FinalScore> FinalScores { get; set; }

        public GameDetails()
        {
            CourseDetails = new CourseDetails();
            FinalScores = new List<FinalScore>();
        }
    }

    public class CourseDetails
    {
        [JsonProperty("course_id")]
        public string CourseId { get; set; }

        [JsonProperty("course_name")]
        public string CourseName { get; set; }

        [JsonProperty("num_holes")]
        public int NumHoles { get; set; }

        [JsonProperty("holes")]
        public List<HoleInfo> Holes { get; set; }

        public CourseDetails()
        {
            Holes = new List<HoleInfo>();
        }
    }

    public class HoleInfo
    {
        [JsonProperty("hole_number")]
        public int HoleNumber { get; set; }

        [JsonProperty("par")]
        public int Par { get; set; }

        [JsonProperty("yards")]
        public int Yards { get; set; }

        [JsonProperty("difficulty_ranking")]
        public int DifficultyRanking { get; set; }
    }

    public class PlayerInfo
    {
        [JsonProperty("player_id")]
        public string PlayerId { get; set; }
    }

    public class FinalScore
    {
        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("score")]
        public int Score { get; set; }

        [JsonProperty("leaderboard_position")]
        public int LeaderboardPosition { get; set; }
    }

    public class EventDetails
    {
        [JsonProperty("club_used")]
        public string ClubUsed { get; set; }

        [JsonProperty("total_yards")]
        public float TotalYards { get; set; }

        [JsonProperty("carry_yards")]
        public float CarryYards { get; set; }

        [JsonProperty("roll_yards")]
        public float RollYards { get; set; }

        [JsonProperty("strokes")]
        public int Strokes { get; set; }

        [JsonProperty("hole_completed")]
        public bool HoleCompleted { get; set; }

        [JsonProperty("offline_raw")]
        public float OfflineRaw { get; set; }

        [JsonProperty("carry_raw")]
        public float CarryRaw { get; set; }

        [JsonProperty("total_length")]
        public float TotalLength { get; set; }

        [JsonProperty("ball_speed")]
        public float BallSpeed { get; set; }

        [JsonProperty("spin_axis")]
        public float SpinAxis { get; set; }

        [JsonProperty("total_spin")]
        public float TotalSpin { get; set; }

        [JsonProperty("launch_angle")]
        public float LaunchAngle { get; set; }

        [JsonProperty("apex_height")]
        public float ApexHeight { get; set; }

        [JsonProperty("flight_time")]
        public float FlightTime { get; set; }

        [JsonProperty("landing_angle")]
        public float LandingAngle { get; set; }

        [JsonProperty("shot_shape")]
        public string ShotShape { get; set; }

        [JsonProperty("distance_to_pin")]
        public float DistanceToPin { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("starting_surface")]
        public string StartingSurface { get; set; }

        [JsonProperty("ending_surface")]
        public string EndingSurface { get; set; }

        [JsonProperty("hole_number")]
        public int HoleNumber { get; set; }

        [JsonProperty("minimap_start_pos")]
        public XYPosition MinimapStartPos { get; set; }

        [JsonProperty("minimap_end_pos")]
        public XYPosition MinimapEndPos { get; set; }

        [JsonProperty("minimap_pin_pos")]
        public XYPosition MinimapPinPos { get; set; }

        [JsonProperty("leaderboard")]
        public List<LeaderboardEntry> Leaderboard { get; set; }

        [JsonProperty("current_hole_stats")]
        public HoleStats CurrentHoleStats { get; set; }

        public EventDetails()
        {
            Leaderboard = new List<LeaderboardEntry>();
            CurrentHoleStats = new HoleStats();
        }
    }

    public class LeaderboardEntry
    {
        [JsonProperty("player_id")]
        public string PlayerId { get; set; }

        [JsonProperty("player_name")]
        public string PlayerName { get; set; }

        [JsonProperty("last_played_hole")]
        public int LastPlayedHole { get; set; }

        [JsonProperty("last_hole_strokes")]
        public int LastHoleStrokes { get; set; }

        [JsonProperty("cumulative_strokes")]
        public int CumulativeStrokes { get; set; }

        [JsonProperty("cumulative_par")]
        public int CumulativePar { get; set; }

        [JsonProperty("leaderboard_position")]
        public int LeaderboardPosition { get; set; }
    }

    public class HoleStats
    {
        [JsonProperty("par")]
        public int Par { get; set; }

        [JsonProperty("total_yards")]
        public int TotalYards { get; set; }
    }

    [BepInPlugin("com.birdieboosters.datastreamingmod", "Data Streaming Mod", "1.0.0")]
    public class DataStreamingMod : BaseUnityPlugin
    {
        private static ManualLogSource LoggerInstance;
        private static string ApiUrl = "https://staging.forgedsports.link/api/events/create/";
        private static bool SendLiveData = true;
        private static int SessionId = 1;
        private static int GameId = 1;
        private static int CurrentPlayer = 1;
        private static bool ScreenshotsEnabled = false;  // New flag for screenshot functionality
        private static Dictionary<int, int> PlayerStrokes = new Dictionary<int, int>()
        {
            {1, 0},
            {2, 0}
        };
        private static Dictionary<int, string> PlayerIds = new Dictionary<int, string>()
        {
            {1, "85"},
            {2, "80"}
        };
        private static Dictionary<int, string> PlayerNames = new Dictionary<int, string>()
        {
            {1, "Juli"},
            {2, "Tiger"}
        };

        // Add fields to store minimap frustum data
        private static Vector3[] currentFrustumCorners;
        private static Vector3 currentMinWorld;
        private static Vector3 currentMaxWorld;
        private static bool isFrustumDataValid = false;
        private static int lastCalculatedHole = -1;  // Track the last hole we calculated frustum for

        private static readonly string[] clubsRealNames = new string[27]
        {
            "Driver",
            "2 Wood",
            "3 Wood",
            "4 Wood",
            "5 Wood",
            "6 Wood",
            "7 Wood",
            "2 Hybrid",
            "3 Hybrid",
            "4 Hybrid",
            "5 Hybrid",
            "6 Hybrid",
            "7 Hybrid",
            "1 Iron",
            "2 Iron",
            "3 Iron",
            "4 Iron",
            "5 Iron",
            "6 Iron",
            "7 Iron",
            "8 Iron",
            "9 Iron",
            "P Wedge",
            "G Wedge",
            "S Wedge",
            "L Wedge",
            "Putter"
        };

        private void Awake()
        {
            LoggerInstance = Logger;
            Logger.LogInfo("[Data Streaming Mod] Plugin Loaded!");
            // Parse command line arguments for player IDs and names
            ParseCommandLineArgs();
            
            // Create screenshots directory if screenshots are enabled
            if (ScreenshotsEnabled)
            {
                string screenshotsDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "screenshots");
                if (!Directory.Exists(screenshotsDir))
                {
                    Directory.CreateDirectory(screenshotsDir);
                    Logger.LogInfo($"[Data Streaming Mod] Created screenshots directory at: {screenshotsDir}");
                }
            }
        }

        private void ParseCommandLineArgs()
        {
            string[] arguments = Environment.GetCommandLineArgs();
            
            // Log all arguments in one message
            StringBuilder argsLog = new StringBuilder("[Data Streaming Mod] Command line arguments: ");
            for (int i = 0; i < arguments.Length; i++)
            {
                argsLog.Append($"\nArg {i}: {arguments[i]}");
            }
            Logger.LogInfo(argsLog.ToString());

            for (int i = 1; i < arguments.Length; i++)
            {
                if (arguments[i].StartsWith("-"))
                {
                    switch (arguments[i])
                    {
                        case "-NoStats":
                            SendLiveData = false;
                            Logger.LogInfo("[Data Streaming Mod] Stats sending disabled");
                            break;
                        case "-playerid1":
                            if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                            {
                                PlayerIds[1] = arguments[i + 1];
                                i++;
                            }
                            break;
                        case "-playerid2":
                            if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                            {
                                PlayerIds[2] = arguments[i + 1];
                                i++;
                            }
                            break;
                        case "-playername1":
                            if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                            {
                                PlayerNames[1] = arguments[i + 1];
                                i++;
                            }
                            break;
                        case "-playername2":
                            if (i + 1 < arguments.Length && !arguments[i + 1].StartsWith("-"))
                            {
                                PlayerNames[2] = arguments[i + 1];
                                i++;
                            }
                            break;
                        case "-sessionid":
                            if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int sessionId))
                            {
                                SessionId = sessionId;
                                i++;
                            }
                            break;
                        case "-gameid":
                            if (i + 1 < arguments.Length && int.TryParse(arguments[i + 1], out int gameId))
                            {
                                GameId = gameId;
                                i++;
                            }
                            break;
                        case "-ScreenshotsOn":
                            ScreenshotsEnabled = true;
                            Logger.LogInfo("[Data Streaming Mod] Screenshot functionality enabled");
                            break;
                    }
                }
            }
        }

        private IEnumerator CaptureScreenshotInMemoryCoroutine(Action<Texture2D> callback)
        {
            if (!ScreenshotsEnabled)
            {
                LoggerInstance.LogInfo("[Data Streaming Mod] Screenshots are disabled, skipping capture");
                callback(null);
                yield break;
            }

            LoggerInstance.LogInfo("[Data Streaming Mod] Attempting to capture screenshot");
            
            // Wait for end of current frame to ensure rendering is complete
            yield return new WaitForEndOfFrame();
            
            Texture2D screenshot = null;
            try
            {
                screenshot = ScreenCapture.CaptureScreenshotAsTexture();
            }
            catch (Exception ex)
            {
                LoggerInstance.LogError($"[Data Streaming Mod] Error capturing screenshot: {ex.Message}");
                callback(null);
                yield break;
            }

            // Give a frame for the texture to fully load
            yield return null;
            
            if (screenshot == null)
            {
                LoggerInstance.LogError("[Data Streaming Mod] Failed to capture screenshot - returned null");
                callback(null);
                yield break;
            }
            
            LoggerInstance.LogInfo($"[Data Streaming Mod] Successfully captured screenshot: {screenshot.width}x{screenshot.height}");
            callback(screenshot);
        }

        private void CaptureScreenshotInMemory(Action<Texture2D> callback)
        {
            StartCoroutine(CaptureScreenshotInMemoryCoroutine(callback));
        }

        private void TakeScreenshot(string prefix)
        {
            if (!ScreenshotsEnabled)
                return;

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string filename = $"{prefix}_{timestamp}.png";
            string screenshotsDir = Path.Combine(Path.GetDirectoryName(Application.dataPath), "screenshots");
            string fullPath = Path.Combine(screenshotsDir, filename);
            
            ScreenCapture.CaptureScreenshot(fullPath);
            Logger.LogInfo($"[Data Streaming Mod] Screenshot saved to: {fullPath}");
        }

        // Game Start Event
        private string ConstructGameStartJson(string courseId, string courseName, int numHoles, List<HoleInfo> holes)
        {
            var gameEvent = new GameEvent
            {
                EventType = "game_start",
                SessionId = SessionId,
                Metadata = new GameMetadata
                {
                    MatchId = "12345",
                    Timestamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                    GameDetails = new GameDetails
                    {
                        GameId = GameId,
                        CourseDetails = new CourseDetails
                        {
                            CourseId = courseId,
                            CourseName = courseName,
                            NumHoles = numHoles,
                            Holes = holes
                        }
                    },
                    Players = new List<PlayerInfo>
                    {
                        new PlayerInfo { PlayerId = PlayerIds[1] },
                        new PlayerInfo { PlayerId = PlayerIds[2] }
                    }
                }
            };

            // Configure JSON serialization to ignore null values
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(gameEvent, jsonSettings);
        }

        // Game End Event
        private string ConstructGameEndJson()
        {
            var gameEvent = new GameEvent
            {
                EventType = "game_end",
                SessionId = SessionId,
                Metadata = new GameMetadata
                {
                    MatchId = "12345",
                    Timestamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                    GameDetails = new GameDetails
                    {
                        GameId = GameId,
                        FinalScores = new List<FinalScore>
                        {
                            new FinalScore
                            {
                                PlayerId = PlayerIds[1],
                                Score = PlayerStrokes[1],
                                LeaderboardPosition = PlayerStrokes[1] <= PlayerStrokes[2] ? 1 : 2
                            },
                            new FinalScore
                            {
                                PlayerId = PlayerIds[2],
                                Score = PlayerStrokes[2],
                                LeaderboardPosition = PlayerStrokes[2] <= PlayerStrokes[1] ? 1 : 2
                            }
                        }
                    }
                }
            };

            // Configure JSON serialization to ignore null values
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(gameEvent, jsonSettings);
        }

        // Shot Executed Event
        private string ConstructShotExecutedJson(ShotData shotData)
        {
            PlayerStrokes[CurrentPlayer]++;

            var gameEvent = new GameEvent
            {
                EventType = "in_game_event",
                SessionId = SessionId,
                Metadata = new GameMetadata
                {
                    SubType = "shot_executed",
                    PlayerId = PlayerIds[CurrentPlayer],
                    Timestamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                    Details = new EventDetails
                    {
                        ClubUsed = shotData.ClubUsed,
                        TotalYards = shotData.TotalYards,
                        CarryYards = shotData.CarryYards,
                        RollYards = shotData.RollYards,
                        Strokes = PlayerStrokes[CurrentPlayer],
                        HoleCompleted = shotData.HoleCompleted,
                        OfflineRaw = shotData.OfflineRaw,
                        CarryRaw = shotData.CarryRaw,
                        TotalLength = shotData.TotalLength,
                        BallSpeed = shotData.BallSpeed,
                        SpinAxis = shotData.SpinAxis,
                        TotalSpin = shotData.TotalSpin,
                        LaunchAngle = shotData.LaunchAngle,
                        ApexHeight = shotData.ApexHeight,
                        FlightTime = shotData.FlightTime,
                        LandingAngle = shotData.LandingAngle,
                        ShotShape = shotData.ShotShape,
                        DistanceToPin = shotData.DistanceToPin,
                        Area = shotData.Area,
                        StartingSurface = shotData.StartingSurface,
                        EndingSurface = shotData.EndingSurface,
                        HoleNumber = shotData.HoleNumber,
                        MinimapStartPos = shotData.MinimapStartPos,
                        MinimapEndPos = shotData.MinimapEndPos,
                        MinimapPinPos = shotData.MinimapPinPos
                    }
                }
            };

            // Configure JSON serialization with custom contract resolver
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                ContractResolver = new CustomContractResolver()
            };

            return JsonConvert.SerializeObject(gameEvent, jsonSettings);
        }

        // Custom contract resolver to control property serialization
        private class CustomContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        {
            protected override IList<Newtonsoft.Json.Serialization.JsonProperty> CreateProperties(Type type, Newtonsoft.Json.MemberSerialization memberSerialization)
            {
                var properties = base.CreateProperties(type, memberSerialization);

                // Filter out properties we don't want to serialize
                properties = properties.Where(p => 
                    // Always include these properties
                    p.PropertyName == "hole_completed" ||
                    p.PropertyName == "event_type" ||
                    p.PropertyName == "metadata" ||
                    p.PropertyName == "sub_type" ||
                    p.PropertyName == "details" ||
                    // Exclude empty collections and default values
                    (p.PropertyType.IsGenericType && 
                     p.PropertyType.GetGenericTypeDefinition() == typeof(List<>) && 
                     p.PropertyName != "leaderboard") ||
                    // Exclude game_details and its children
                    p.PropertyName != "game_details" &&
                    p.PropertyName != "final_scores" &&
                    p.PropertyName != "course_details" &&
                    p.PropertyName != "holes"
                ).ToList();

                return properties;
            }
        }

        // Hole Completed Event
        private string ConstructHoleCompletedJson()
        {
            var gameEvent = new GameEvent
            {
                EventType = "in_game_event",
                SessionId = SessionId,
                Metadata = new GameMetadata
                {
                    SubType = "hole_completed",
                    PlayerId = PlayerIds[CurrentPlayer],
                    Timestamp = DateTime.Now.ToUniversalTime().ToString("yyyy-MM-dd'T'HH:mm:ssK", CultureInfo.InvariantCulture),
                    Details = new EventDetails
                    {
                        Leaderboard = new List<LeaderboardEntry>
                        {
                            new LeaderboardEntry
                            {
                                PlayerId = PlayerIds[1],
                                PlayerName = PlayerNames[1],
                                LastHoleStrokes = PlayerStrokes[1],
                                CumulativeStrokes = PlayerStrokes[1],
                                LeaderboardPosition = PlayerStrokes[1] <= PlayerStrokes[2] ? 1 : 2
                            },
                            new LeaderboardEntry
                            {
                                PlayerId = PlayerIds[2],
                                PlayerName = PlayerNames[2],
                                LastHoleStrokes = PlayerStrokes[2],
                                CumulativeStrokes = PlayerStrokes[2],
                                LeaderboardPosition = PlayerStrokes[2] <= PlayerStrokes[1] ? 1 : 2
                            }
                        },
                        CurrentHoleStats = new HoleStats
                        {
                            Par = 4,
                            TotalYards = 500
                        }
                    }
                }
            };

            // Configure JSON serialization to ignore null values
            var jsonSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            return JsonConvert.SerializeObject(gameEvent, jsonSettings);
        }

        private IEnumerator SendDataToServer(string jsonData, Texture2D screenshot = null)
        {
            if (!SendLiveData)
            {
                LoggerInstance.LogInfo("[Data Streaming Mod] Live data sending is disabled");
                yield break;
            }

            LoggerInstance.LogInfo($"[Data Streaming Mod] SendDataToServer called with screenshot: {(screenshot != null ? "present" : "null")}");

            var request = new UnityWebRequest(ApiUrl, "POST");
            
            if (screenshot != null)
            {
                LoggerInstance.LogInfo("[Data Streaming Mod] Attempting to send screenshot with event data");
                LoggerInstance.LogInfo($"[Data Streaming Mod] Screenshot dimensions: {screenshot.width}x{screenshot.height}, format: {screenshot.format}");

                try
                {
                    // Convert screenshot to PNG bytes
                    byte[] screenshotBytes = screenshot.EncodeToPNG();
                    if (screenshotBytes == null || screenshotBytes.Length == 0)
                    {
                        LoggerInstance.LogError("[Data Streaming Mod] Failed to encode screenshot to PNG");
                        yield break;
                    }
                    LoggerInstance.LogInfo($"[Data Streaming Mod] Screenshot PNG size: {screenshotBytes.Length} bytes");
                    
                    // Create multipart form data with named parts
                    List<IMultipartFormSection> formData = new List<IMultipartFormSection>();
                    
                    // Add event_data part with JSON content
                    formData.Add(new MultipartFormDataSection("event_data", jsonData, "application/json"));
                    
                    // Add image part with PNG content
                    formData.Add(new MultipartFormFileSection("image", screenshotBytes, "game_event.png", "image/png"));
                    
                    // Generate boundary and serialize form sections
                    byte[] boundary = Encoding.UTF8.GetBytes("----WebKitFormBoundary" + DateTime.Now.Ticks.ToString("x"));
                    byte[] formSections = UnityWebRequest.SerializeFormSections(formData, boundary);
                    
                    // Create termination string (CRLF--{boundary}--)
                    byte[] terminate = Encoding.UTF8.GetBytes(String.Concat("\r\n--", Encoding.UTF8.GetString(boundary), "--"));
                    
                    // Combine form sections and termination into complete body
                    byte[] body = new byte[formSections.Length + terminate.Length];
                    Buffer.BlockCopy(formSections, 0, body, 0, formSections.Length);
                    Buffer.BlockCopy(terminate, 0, body, formSections.Length, terminate.Length);
                    
                    // Set content type with boundary
                    string contentType = String.Concat("multipart/form-data; boundary=", Encoding.UTF8.GetString(boundary));
                    
                    // Set up request with proper content type and body
                    request.uploadHandler = new UploadHandlerRaw(body);
                    request.SetRequestHeader("Content-Type", contentType);

                    LoggerInstance.LogInfo($"[Data Streaming Mod] Total request body size: {body.Length} bytes");
                }
                catch (Exception ex)
                {
                    LoggerInstance.LogError($"[Data Streaming Mod] Error preparing screenshot for upload: {ex.Message}");
                    LoggerInstance.LogError($"[Data Streaming Mod] Stack trace: {ex.StackTrace}");
                    yield break;
                }
            }
            else
            {
                LoggerInstance.LogInfo("[Data Streaming Mod] No screenshot provided, sending regular JSON request");
                // Regular JSON request
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.SetRequestHeader("Content-Type", "application/json");
            }
            
            request.downloadHandler = new DownloadHandlerBuffer();

            LoggerInstance.LogInfo("[Data Streaming Mod] Sending data to server...");
            
            yield return request.SendWebRequest();

            string output = $"Data sent: {jsonData}\nStatus Code: {request.responseCode}";
            if (request.uploadHandler != null && request.uploadHandler.data != null)
            {
                output += $"\nRequest Body Size: {request.uploadHandler.data.Length} bytes";
            }
            
            if (request.responseCode == 201)
            {
                output += "\nCreated. Success! Request was fulfilled and a new resource was created";
                LoggerInstance.LogInfo(output);
            }
            else
            {
                string errorDetails = request.downloadHandler.text;
                output += $"\nErrors: {request.error}";
                if (!string.IsNullOrEmpty(errorDetails))
                {
                    output += $"\nServer Response: {errorDetails}";
                }
                LoggerInstance.LogError(output);
            }
        }

        // Data structure for shot information
        public class ShotData
        {
            public string ClubUsed;
            public float TotalYards;
            public float CarryYards;
            public float RollYards;
            public bool HoleCompleted;
            public float OfflineRaw;
            public float CarryRaw;
            public float TotalLength;
            public float BallSpeed;
            public float SpinAxis;
            public float TotalSpin;
            public float LaunchAngle;
            public float ApexHeight;
            public float FlightTime;
            public float LandingAngle;
            public string ShotShape;
            public float DistanceToPin;
            public string Area;
            public string StartingSurface;
            public string EndingSurface;
            public XYPosition MinimapStartPos;
            public XYPosition MinimapEndPos;
            public XYPosition MinimapPinPos;
            public int HoleNumber;
        }

        // Store the instance for use in patches
        private static DataStreamingMod Instance;
        private void Start()
        {
            Instance = this;
        }

        // Harmony patches for game events
        [HarmonyPatch(typeof(MainGameControler), "FirstTeeSetup")]
        public class Patch_MainGameController_FirstTeeSetup
        {
            static void Postfix(MainGameControler __instance)
            {
                if (__instance == null || __instance.parseDescFile == null || __instance.ActiveGameHoles == null)
                {
                    LoggerInstance.LogError("[Data Streaming Mod] Failed to get course information - required objects are null");
                    return;
                }

                // Start coroutine with delay
                Instance.StartCoroutine(DelayedGameStart(__instance));
            }

            private static IEnumerator DelayedGameStart(MainGameControler __instance)
            {
                // Wait for 1 second
                yield return new WaitForSeconds(1.0f);

                // Take screenshot at game start
                Instance.TakeScreenshot("gamestart");

                // Get course details from MainGameControler
                string courseId = "003";
                string courseName = __instance.ActiveGameCourseName ?? "Unknown Course";
                int numHoles = __instance.ActiveGameHoles.Length;

                // Create list of holes
                List<HoleInfo> holes = new List<HoleInfo>();
                for (int i = 0; i < numHoles; i++)
                {
                    var hole = __instance.ActiveGameHoles[i];
                    if (hole.active)
                    {
                        holes.Add(new HoleInfo
                        {
                            HoleNumber = i + 1,
                            Par = hole.par,
                            Yards = (int)Vector3.Distance(hole.teePos, hole.pinPos),
                            DifficultyRanking = hole.pinDifficulty
                        });
                    }
                }

                // Send game start event with actual course data
                string startJsonData = Instance.ConstructGameStartJson(courseId, courseName, numHoles, holes);
                
                // Capture screenshot and send with game start data
                Instance.CaptureScreenshotInMemory((screenshot) => {
                    Instance.StartCoroutine(Instance.SendDataToServer(startJsonData, screenshot));
                });

                // Wait a frame to ensure previous send completes
                yield return null;

                // Send game end event immediately after
                string endJsonData = Instance.ConstructGameEndJson();
                Instance.StartCoroutine(Instance.SendDataToServer(endJsonData));
            }
        }

        // Add new patch for end of round scorecard screenshot
        [HarmonyPatch(typeof(Assets.Scripts.GameModes.Scramble), "AllPlayersHoledOut")]
        public class Patch_Scramble_AllPlayersHoledOut
        {
            static void Postfix(Assets.Scripts.GameModes.Scramble __instance)
            {
                if (!ScreenshotsEnabled)
                    return;

                // Only take screenshot if round is complete
                if (__instance.IsRoundComplete())
                {
                    // Wait for scorecard to be fully rendered
                    Instance.StartCoroutine(DelayedFinalScorecardScreenshot());
                }
            }
        }

        private static IEnumerator DelayedFinalScorecardScreenshot()
        {
            // Wait for scorecard UI to fully render
            yield return new WaitForSeconds(1.0f);
            Instance.TakeScreenshot("final_scorecard");
        }

        private void OnApplicationQuit()
        {
            // Take screenshot at game end
            Instance.TakeScreenshot("gameend");
            
            // Send game end event when the application is quitting
            string jsonData = Instance.ConstructGameEndJson();
            Instance.StartCoroutine(Instance.SendDataToServer(jsonData));
        }


        // Add method to calculate and store frustum data
        private static void CalculateAndStoreMinimapFrustum(Camera minimapCam)
        {
            if (minimapCam == null)
            {
                LoggerInstance.LogError("[Data Streaming Mod] Failed to get minimap camera for frustum calculation");
                return;
            }

            // Get the main game controller to access ball and pin positions
            var mGC = UnityEngine.Object.FindObjectOfType<MainGameControler>();
            if (mGC == null || mGC.mainBall == null || mGC.ActiveGameHoles == null)
            {
                LoggerInstance.LogError("[Data Streaming Mod] Failed to get required game objects for frustum calculation");
                return;
            }

            // Calculate the midpoint height between ball and pin
            Vector3 ballPos = mGC.mainBall.transform.position;
            Vector3 pinPos = mGC.ActiveGameHoles[mGC.currentHole].pinPos;
            float midpointHeight = (ballPos.y + pinPos.y) / 2f;
            float planeHeight = midpointHeight; // Use midpoint height directly

            LoggerInstance.LogInfo($"[Data Streaming Mod] Frustum Calculation Details:");
            LoggerInstance.LogInfo($"- Camera Position: {minimapCam.transform.position}");
            LoggerInstance.LogInfo($"- Camera Rotation: {minimapCam.transform.rotation.eulerAngles}");
            LoggerInstance.LogInfo($"- Ball Position: {ballPos}");
            LoggerInstance.LogInfo($"- Pin Position: {pinPos}");
            LoggerInstance.LogInfo($"- Midpoint Height: {midpointHeight}");
            LoggerInstance.LogInfo($"- Target Plane Height: {planeHeight}");

            // Calculate frustum corners in view space
            currentFrustumCorners = new Vector3[4];
            minimapCam.CalculateFrustumCorners(
                new Rect(0, 0, 1, 1),
                minimapCam.farClipPlane,
                Camera.MonoOrStereoscopicEye.Mono,
                currentFrustumCorners
            );

            LoggerInstance.LogInfo($"[Data Streaming Mod] View Space Frustum Corners:");
            for (int i = 0; i < 4; i++)
            {
                LoggerInstance.LogInfo($"- Corner {i}: {currentFrustumCorners[i]}");
            }

            // Convert to world space using the camera's view matrix
            Matrix4x4 viewToWorld = minimapCam.cameraToWorldMatrix;
            for (int i = 0; i < 4; i++)
            {
                currentFrustumCorners[i] = viewToWorld.MultiplyPoint3x4(currentFrustumCorners[i]);
            }

            LoggerInstance.LogInfo($"[Data Streaming Mod] World Space Frustum Corners:");
            for (int i = 0; i < 4; i++)
            {
                LoggerInstance.LogInfo($"- Corner {i}: {currentFrustumCorners[i]}");
            }

            // Project corners onto the horizontal plane
            for (int i = 0; i < 4; i++)
            {
                currentFrustumCorners[i] = new Vector3(
                    currentFrustumCorners[i].x,
                    planeHeight,
                    currentFrustumCorners[i].z
                );
            }

            LoggerInstance.LogInfo($"[Data Streaming Mod] Projected Frustum Corners (at height {planeHeight}):");
            for (int i = 0; i < 4; i++)
            {
                LoggerInstance.LogInfo($"- Corner {i}: {currentFrustumCorners[i]}");
            }

            // Get camera's y rotation and position
            float cameraYRotation = mGC.miniMapCam.transform.rotation.eulerAngles.y;
            Quaternion rotation = Quaternion.Euler(0, -cameraYRotation, 0); // Use negative rotation to get inverse
            Vector3 cameraPos = mGC.miniMapCam.transform.position;

            LoggerInstance.LogInfo($"[Data Streaming Mod] Camera Details:");
            LoggerInstance.LogInfo($"- Position: {cameraPos}");
            LoggerInstance.LogInfo($"- Y Rotation: {cameraYRotation}");
            LoggerInstance.LogInfo($"- Inverse Y Rotation: {-cameraYRotation}");

            // Rotate positions about camera position using inverse rotation
            for (int i = 0; i < 4; i++)
            {
                currentFrustumCorners[i] = cameraPos + rotation * (currentFrustumCorners[i] - cameraPos);
            }

            LoggerInstance.LogInfo($"[Data Streaming Mod] Rotated Frustum Corners:");
            for (int i = 0; i < 4; i++)
            {
                LoggerInstance.LogInfo($"- Corner {i}: {currentFrustumCorners[i]}");
            }

            // Verify corner 0 is bottom-left (lowest x and z)
            bool isCorner0BottomLeft = true;
            for (int i = 1; i < 4; i++)
            {
                if (currentFrustumCorners[i].x < currentFrustumCorners[0].x || 
                    currentFrustumCorners[i].z < currentFrustumCorners[0].z)
                {
                    isCorner0BottomLeft = false;
                    break;
                }
            }

            if (!isCorner0BottomLeft)
            {
                LoggerInstance.LogWarning($"[Data Streaming Mod] Corner 0 is not bottom-left after rotation!");
                LoggerInstance.LogWarning($"Corner 0: {currentFrustumCorners[0]}");
                for (int i = 1; i < 4; i++)
                {
                    LoggerInstance.LogWarning($"Corner {i}: {currentFrustumCorners[i]}");
                }
            }

            // Calculate world bounds
            currentMinWorld = new Vector3(
                Mathf.Min(currentFrustumCorners[0].x, currentFrustumCorners[1].x, currentFrustumCorners[2].x, currentFrustumCorners[3].x),
                planeHeight,
                Mathf.Min(currentFrustumCorners[0].z, currentFrustumCorners[1].z, currentFrustumCorners[2].z, currentFrustumCorners[3].z)
            );
            currentMaxWorld = new Vector3(
                Mathf.Max(currentFrustumCorners[0].x, currentFrustumCorners[1].x, currentFrustumCorners[2].x, currentFrustumCorners[3].x),
                planeHeight,
                Mathf.Max(currentFrustumCorners[0].z, currentFrustumCorners[1].z, currentFrustumCorners[2].z, currentFrustumCorners[3].z)
            );

            LoggerInstance.LogInfo($"[Data Streaming Mod] Final Frustum Bounds:");
            LoggerInstance.LogInfo($"- Min World: {currentMinWorld}");
            LoggerInstance.LogInfo($"- Max World: {currentMaxWorld}");
        }

        [HarmonyPatch(typeof(DataTracker), "SubmitPlayerShot")]
        public class Patch_DataTracker_SubmitPlayerShot
        {
            static void Postfix(DataTracker __instance, playerShot thisShot)
            {
                if (__instance == null || thisShot == null)
                    return;

                // Get the club index and use it to get the real club name
                int clubIndex = thisShot.ClubIndex;
                string clubName = clubIndex >= 0 && clubIndex < clubsRealNames.Length ? clubsRealNames[clubIndex] : "Unknown";

                // Determine if the hole is completed based on whether the ball went into the hole
                bool holeCompleted = thisShot.HoleResult == 7;

                // Convert surface types to string
                string startingSurface = ((Trajectory.eSurfaces)thisShot.StartingSurface).ToString();
                string endingSurface = ((Trajectory.eSurfaces)thisShot.EndingSurface).ToString();

                // Get the minimap camera and texture from the main game controller
                Camera minimapCam = __instance.mGC.miniMapCam;
                if (minimapCam == null)
                {
                    LoggerInstance.LogError("[Data Streaming Mod] Failed to get minimap camera");
                    return;
                }

                // Get the minimap texture dimensions
                float minimapWidth = 317f;  // Width of minimap texture
                float minimapHeight = 600f; // Height of minimap texture
                isFrustumDataValid = lastCalculatedHole == thisShot.Hole;
                // If frustum data is not valid, calculate it now
                if (!isFrustumDataValid)
                {
                    CalculateAndStoreMinimapFrustum(minimapCam);
                    lastCalculatedHole = thisShot.Hole;
                }

                LoggerInstance.LogInfo($"[Data Streaming Mod] Using stored frustum data for hole {thisShot.Hole + 1}:");
                LoggerInstance.LogInfo($"- Min World: {currentMinWorld}");
                LoggerInstance.LogInfo($"- Max World: {currentMaxWorld}");

                // Log world positions before conversion
                LoggerInstance.LogInfo($"[Data Streaming Mod] World Positions:");
                LoggerInstance.LogInfo($"- Start Position: {thisShot.StartingPOS}");
                LoggerInstance.LogInfo($"- End Position: {thisShot.EndingPOS}");

                // Calculate the midpoint height between ball and pin
                Vector3 ballPos = __instance.mGC.mainBall.transform.position;
                Vector3 pinPos = __instance.mGC.ActiveGameHoles[thisShot.Hole].pinPos;
                float midpointHeight = (ballPos.y + pinPos.y) / 2f;
                float planeHeight = midpointHeight; // Use midpoint height directly

                LoggerInstance.LogInfo($"[Data Streaming Mod] Height Calculation:");
                LoggerInstance.LogInfo($"- Ball Position: {ballPos}");
                LoggerInstance.LogInfo($"- Pin Position: {pinPos}");
                LoggerInstance.LogInfo($"- Midpoint Height: {midpointHeight}");
                LoggerInstance.LogInfo($"- Target Plane Height: {planeHeight}");

                // Project positions onto horizontal plane
                Vector3 startPos = new Vector3(thisShot.StartingPOS.x, planeHeight, thisShot.StartingPOS.z);
                Vector3 endPos = new Vector3(thisShot.EndingPOS.x, planeHeight, thisShot.EndingPOS.z);
                Vector3 pinPosProjected = new Vector3(__instance.mGC.ActiveGameHoles[thisShot.Hole].pinPos.x, planeHeight, __instance.mGC.ActiveGameHoles[thisShot.Hole].pinPos.z);

                LoggerInstance.LogInfo($"[Data Streaming Mod] Projected Positions (at height {planeHeight}):");
                LoggerInstance.LogInfo($"- Start Position: {startPos}");
                LoggerInstance.LogInfo($"- End Position: {endPos}");
                LoggerInstance.LogInfo($"- Pin Position: {pinPosProjected}");

                // Get camera's y rotation and position
                float cameraYRotation = __instance.mGC.miniMapCam.transform.rotation.eulerAngles.y;
                Quaternion rotation = Quaternion.Euler(0, -cameraYRotation, 0); // Use negative rotation to get inverse
                Vector3 cameraPos = __instance.mGC.miniMapCam.transform.position;

                LoggerInstance.LogInfo($"[Data Streaming Mod] Camera Details:");
                LoggerInstance.LogInfo($"- Position: {cameraPos}");
                LoggerInstance.LogInfo($"- Y Rotation: {cameraYRotation}");
                LoggerInstance.LogInfo($"- Inverse Y Rotation: {-cameraYRotation}");

                // Rotate positions about camera position using inverse rotation
                startPos = cameraPos + rotation * (startPos - cameraPos);
                endPos = cameraPos + rotation * (endPos - cameraPos);
                pinPosProjected = cameraPos + rotation * (pinPosProjected - cameraPos);

                LoggerInstance.LogInfo($"[Data Streaming Mod] Rotated Positions (using inverse camera rotation):");
                LoggerInstance.LogInfo($"- Start Position: {startPos}");
                LoggerInstance.LogInfo($"- End Position: {endPos}");
                LoggerInstance.LogInfo($"- Pin Position: {pinPosProjected}");

                // Convert world positions to minimap texture coordinates (0-1 range)
                Vector2 minimapStartPos = new Vector2(
                    Mathf.InverseLerp(currentMinWorld.x, currentMaxWorld.x, startPos.x),
                    Mathf.InverseLerp(currentMinWorld.z, currentMaxWorld.z, startPos.z)
                );
                Vector2 minimapEndPos = new Vector2(
                    Mathf.InverseLerp(currentMinWorld.x, currentMaxWorld.x, endPos.x),
                    Mathf.InverseLerp(currentMinWorld.z, currentMaxWorld.z, endPos.z)
                );

                // Calculate pin position in minimap space if this is the first stroke
                Vector2 minimapPinPos = Vector2.zero;
                if (thisShot.HoleShot == 1)
                {
                    minimapPinPos = new Vector2(
                        Mathf.InverseLerp(currentMinWorld.x, currentMaxWorld.x, pinPosProjected.x),
                        Mathf.InverseLerp(currentMinWorld.z, currentMaxWorld.z, pinPosProjected.z)
                    );

                    LoggerInstance.LogInfo($"[Data Streaming Mod] Pin Position in Minimap Space (0-1):");
                    LoggerInstance.LogInfo($"- Minimap Position: {minimapPinPos}");
                }

                LoggerInstance.LogInfo($"[Data Streaming Mod] Normalized Minimap Coordinates (0-1):");
                LoggerInstance.LogInfo($"- Start: {minimapStartPos}");
                LoggerInstance.LogInfo($"- End: {minimapEndPos}");

                // Scale to minimap texture dimensions
                minimapStartPos.x *= minimapWidth;
                minimapStartPos.y *= minimapHeight;
                minimapEndPos.x *= minimapWidth;
                minimapEndPos.y *= minimapHeight;
                if (thisShot.HoleShot == 1)
                {
                    minimapPinPos.x *= minimapWidth;
                    minimapPinPos.y *= minimapHeight;
                }

                // Clamp coordinates to minimap texture bounds
                minimapStartPos.x = Mathf.Clamp(minimapStartPos.x, 0, minimapWidth);
                minimapStartPos.y = Mathf.Clamp(minimapStartPos.y, 0, minimapHeight);
                minimapEndPos.x = Mathf.Clamp(minimapEndPos.x, 0, minimapWidth);
                minimapEndPos.y = Mathf.Clamp(minimapEndPos.y, 0, minimapHeight);
                if (thisShot.HoleShot == 1)
                {
                    minimapPinPos.x = Mathf.Clamp(minimapPinPos.x, 0, minimapWidth);
                    minimapPinPos.y = Mathf.Clamp(minimapPinPos.y, 0, minimapHeight);
                }

                LoggerInstance.LogInfo($"[Data Streaming Mod] Final Minimap Coordinates (Texture Space, Clamped):");
                LoggerInstance.LogInfo($"- Start: {minimapStartPos}");
                LoggerInstance.LogInfo($"- End: {minimapEndPos}");
                if (thisShot.HoleShot == 1)
                {
                    LoggerInstance.LogInfo($"- Pin: {minimapPinPos}");
                }

                // Get shot data from the playerShot object
                ShotData shotData = new ShotData
                {
                    ClubUsed = clubName,
                    TotalYards = thisShot.TotalDistance,
                    CarryYards = thisShot.BallData?.CarryDistance ?? 0,
                    RollYards = thisShot.TotalDistance - (thisShot.BallData?.CarryDistance ?? 0),
                    HoleCompleted = holeCompleted,
                    OfflineRaw = thisShot.BallData?.Offline ?? 0,
                    CarryRaw = thisShot.BallData?.CarryDistance ?? 0,
                    TotalLength = thisShot.TotalDistance,
                    BallSpeed = (float)thisShot.BallSpeed,
                    SpinAxis = thisShot.BallData?.SpinAxis ?? 0,
                    TotalSpin = thisShot.BallData?.TotalSpin ?? 0,
                    LaunchAngle = 0,
                    ApexHeight = 0,
                    FlightTime = 0,
                    LandingAngle = 0,
                    ShotShape = "N/A",
                    DistanceToPin = 0,
                    Area = thisShot.StartingSurface.ToString(),
                    StartingSurface = startingSurface,
                    EndingSurface = endingSurface,
                    MinimapStartPos = new XYPosition(minimapStartPos),
                    MinimapEndPos = new XYPosition(minimapEndPos),
                    MinimapPinPos = new XYPosition(minimapPinPos),
                    HoleNumber = thisShot.Hole+1
                };

                string jsonData = Instance.ConstructShotExecutedJson(shotData);
                
                // Capture screenshot and send with data
                Instance.CaptureScreenshotInMemory((screenshot) => {
                    Instance.StartCoroutine(Instance.SendDataToServer(jsonData, screenshot));
                });
            }
        }
        
        // [HarmonyPatch(typeof(DataTracker), "SubmitHole")]
        // public class Patch_DataTracker_SubmitHole
        // {
        //     static void Postfix(DataTracker __instance)
        //     {
        //         if (__instance == null || __instance.mGC == null)
        //             return;
        //
        //         // Get scorecard data
        //         var scoreCardData = __instance.mGC.scoreCard.InitScoreCardDataObject();
        //         var gameModeData = __instance.mGC.gameMode.GetScorecardDataObject();
        //
        //         // Update current hole and strokes from scorecard data
        //         if (scoreCardData != null && scoreCardData.Count > 0)
        //         {
        //             var currentHoleData = scoreCardData.Find(row => row.HoleNumber == Instance.CurrentHole);
        //             if (currentHoleData != null)
        //             {
        //                 Instance.PlayerStrokes[1] = currentHoleData.Player1Score;
        //                 Instance.PlayerStrokes[2] = currentHoleData.Player2Score;
        //             }
        //         }
        //
        //         string holeCompletedJson = Instance.ConstructHoleCompletedJson();
        //         Instance.StartCoroutine(Instance.SendDataToServer(holeCompletedJson));
        //         
        //         // Switch to next player or advance hole
        //         if (Instance.CurrentPlayer == 1)
        //         {
        //             Instance.CurrentPlayer = 2;
        //         }
        //         else
        //         {
        //             Instance.CurrentPlayer = 1;
        //             Instance.CurrentHole++;
        //         }
        //     }
        // }
    }
} 
