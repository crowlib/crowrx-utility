# CrowRx Utility Package

A collection of specialized utilities for Unity, extending the CrowRx ecosystem with advanced camera systems, networking, physics, and editor-workflow enhancements.

## Dependencies

- **CrowRx.Core**: Base utilities and logging.
- **CrowRx.Unity**: Core Unity extensions and base classes.
- **UniTask**: Asynchronous operations.
- **R3**: Reactive extensions.
- **ZLinq**: Performance-optimized LINQ.

## API Reference

### 1. Camera Utilities (`CameraUtility`)
Advanced camera control including asynchronous transitions and UI positioning.

*   **`TransitionAsync` / `TransitionObservable`**: Smoothly lerp position, rotation, FOV, and projection matrices between two cameras.
*   **`UpdateUIPositionFromWorldPosition`**: Maps a 3D world position to a 2D `RectTransform` position using specific game and UI cameras.

**Example:**
```csharp
// Async transition between cameras over 1 second
await mainCamera.TransitionAsync(targetCamera, 1.0f, destroyCancellationToken);

// Update floating health bar position
CameraUtility.UpdateUIPositionFromWorldPosition(gameCam, uiCam, targetPoint, healthBarRect, offset);
```

---

### 2. Networking (`Ftp` Client)
A fully-featured asynchronous FTP client designed for Unity.

*   **`Download` / `Upload`**: Transfer files with `CancellationToken` support.
*   **`GetDirectoryListSimple` / `Detailed`**: Retrieve remote file listings.
*   **`CreateDirectory` / `Delete` / `Rename`**: Remote file system management.

**Example:**
```csharp
var client = new Ftp("ftp://127.0.0.1", "user", "pass");
bool success = await client.Download("remote/data.bin", Application.persistentDataPath + "/local.bin", token);
```

---

### 3. Geometry & Physics (`PhysicsUtility`, `PCA`, `Graphic`)
Mathematical and physical calculation helpers.

*   **`PhysicsUtility.MaskForLayer(layer)`**: Returns a pre-cached collision mask for a specific layer based on the Physics Collision Matrix.
*   **`PCA.ComputeOBB(points)`**: Calculates an Oriented Bounding Box (OBB) for a set of points using Principal Component Analysis.
*   **`Graphic.RectIntersect(rect, a, b)`**: Checks if a 2D line segment intersects a given `Rect`.

---

### 4. Data Persistence (`PlayerPrefUtil`, `BytesFile`)
Extended storage options for Unity applications.

*   **`SaveObjectToPlayerPref`**: Serializes any `[Serializable]` object into `PlayerPrefs` via Base64.
*   **`ReadAtPersistentDataPath` / `WriteAtPersistentDataPath`**: Simplified byte-array file operations at the application's persistent path.

**Example:**
```csharp
PlayerData data = new PlayerData { Level = 10 };
PlayerPrefUtil.SaveObjectToPlayerPref("SAVE_DATA", data);
```

---

### 5. Editor Workflow (`AppVersion`, `TypeUtility`)
Tools for streamlining the build and development process.

*   **`AppVersion`**: Centralized management for `bundleVersion` and `bundleVersionCode`, supporting automated incrementing and platform-specific applying.
*   **`TypeUtility.GetPrefabPaths<T>()`**: Efficiently finds all prefab assets that contain a specific component type `T`.
*   **`AssetLinkerEditor`**: A base editor for linking assets via AssetBundle and Asset names rather than direct references.

---

### 6. Motion & Momentum (`MomentumVector3`)
Physically-inspired smoothing for user input or object movement.

*   **`Tick_On` / `Tick_Off`**: Simulates momentum accumulation and spring-based dampening for smooth deceleration.

## Requirements
- **Unity 6000.3 or newer**
- **C# 9.0+** compatible environment

## License
This project is licensed under the [MIT License](LICENSE).
