using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;

public sealed class GameViewPngCaptureWindow : EditorWindow
{
    private const string DefaultScope = "p01-invitation-visual-approval";
    private const string PendingCaptureSessionKey =
        "UnderTheHorizon.GameViewPngCapture.PendingCleanup";
    private const string PendingCaptureMarker = ".uth-game-view-capture.";
    private const double CaptureTimeoutSeconds = 15d;
    private const double PendingCleanupTimeoutSeconds = 60d;
    private const int RequiredStablePlayFrames = 3;
    private const int RequiredStableFilePolls = 2;

    private sealed class PendingCaptureCleanupState
    {
        public double deadline;
        public long fileLength = -1;
        public DateTime fileWriteTimeUtc;
        public int stableFilePolls;
        public bool deleteAttempted;
    }

    private static readonly Dictionary<string, PendingCaptureCleanupState>
        PendingCaptureCleanupStates =
        new(StringComparer.OrdinalIgnoreCase);

    private static readonly Vector2Int[] Resolutions =
    {
        new(1920, 1080),
        new(2560, 1440),
        new(1920, 1200),
    };

    private static readonly string[] StateLabels =
    {
        "idle",
        "hover",
        "focus",
        "completed",
        "restarted",
    };

    private int resolutionIndex;
    private int stateIndex;
    private string session = $"{DateTime.Now:yyyy-MM-dd}_working-tree";
    private string scope = DefaultScope;
    private string interactionId = "INT_P_01_INVITATION";
    private string pendingTemporaryPath;
    private string pendingFinalPath;
    private Vector2Int pendingResolution;
    private Vector2Int pendingScreenResolution;
    private long pendingFileLength = -1;
    private DateTime pendingFileWriteTimeUtc;
    private int pendingStableFilePolls;
    private int pendingStablePlayFrames;
    private int pendingLastObservedFrame = -1;
    private double pendingCaptureDeadline;
    private bool captureIssued;
    private Vector2Int requestedResolution;
    private Vector2Int currentGameResolution;
    private bool resolutionReady;
    private bool resolutionVerificationPending;
    private int resolutionStablePlayFrames;
    private int resolutionLastObservedFrame = -1;
    private double resolutionVerificationDeadline;
    private GameObject previousSelection;
    private string status = "Game View에서 고정 해상도를 선택하고 Play Mode에서 검증한 뒤 캡처합니다.";

    public static IReadOnlyList<Vector2Int> SupportedResolutions => Resolutions;

    [InitializeOnLoadMethod]
    private static void RestorePendingCaptureCleanup()
    {
        string serializedPaths = SessionState.GetString(PendingCaptureSessionKey, string.Empty);
        SessionState.SetString(PendingCaptureSessionKey, string.Empty);
        foreach (string path in serializedPaths.Split(
                     new[] { '\n' },
                     StringSplitOptions.RemoveEmptyEntries))
        {
            SchedulePendingCaptureCleanup(path);
        }

        QueueManagedPendingCaptureFiles(GetValidationRoot());
    }

    [MenuItem("Under The Horizon/Preview/Game View PNG Capture")]
    public static void Open()
    {
        GameViewPngCaptureWindow window = Resources
            .FindObjectsOfTypeAll<GameViewPngCaptureWindow>()
            .FirstOrDefault();
        if (window == null)
            window = CreateInstance<GameViewPngCaptureWindow>();
        window.titleContent = new GUIContent("Game View PNG");
        window.minSize = new Vector2(420f, 330f);
        window.ShowUtility();
        window.Focus();
    }

    [MenuItem("Under The Horizon/Preview/Capture Current Game View PNG %#&g")]
    private static void CaptureFromShortcut()
    {
        GameViewPngCaptureWindow window = FindOpenWindow();
        if (window == null)
        {
            Debug.LogError("Open the Game View PNG Capture window before using Ctrl+Alt+Shift+G.");
            return;
        }

        window.CaptureSelectedState();
    }

    [MenuItem("Under The Horizon/Preview/Submit Current Interaction Via EventSystem %#&f")]
    private static void SubmitInteractionFromShortcut()
    {
        GameViewPngCaptureWindow window = FindOpenWindow();
        if (window == null)
        {
            Debug.LogError(
                "Open the Game View PNG Capture window before using Ctrl+Alt+Shift+F.");
            return;
        }

        window.SubmitInteraction();
    }

    private static GameViewPngCaptureWindow FindOpenWindow() => Resources
        .FindObjectsOfTypeAll<GameViewPngCaptureWindow>()
        .FirstOrDefault();

    private void OnEnable()
    {
        QueueManagedPendingCaptureFiles(GetValidationRoot());
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
    }

    private void OnGUI()
    {
        EditorGUILayout.LabelField("Exact Game View QA Capture", EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "캡처 기능은 Bootstrap 실제 흐름의 현재 화면과 저장 상태를 변경하지 않습니다. "
            + "단, EventSystem submit 보조는 실제 interaction을 실행하므로 진행·효과·체크포인트를 변경할 수 있습니다.",
            MessageType.Info);
        EditorGUILayout.HelpBox(
            "고정 해상도 Game View를 별도 창으로 계속 보이게 둔 채 PASS가 표시될 때까지 Play Mode를 유지하세요.",
            MessageType.Warning);

        bool operationPending = resolutionVerificationPending
            || !string.IsNullOrEmpty(pendingTemporaryPath);
        using (new EditorGUI.DisabledScope(operationPending))
        {
            session = EditorGUILayout.TextField("Session", session);
            scope = EditorGUILayout.TextField("Scope", scope);
            resolutionIndex = EditorGUILayout.Popup(
                "Target Resolution",
                resolutionIndex,
                Resolutions.Select(FormatResolution).ToArray());
            stateIndex = EditorGUILayout.Popup("State", stateIndex, StateLabels);

            Vector2Int target = Resolutions[resolutionIndex];
            if (target != requestedResolution)
                resolutionReady = false;
            EditorGUILayout.LabelField(
                "Current Game Screen",
                currentGameResolution.x > 0
                    ? FormatResolution(currentGameResolution)
                    : "Not reported");
            EditorGUILayout.LabelField("Target Screen", FormatResolution(target));

            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Verify Target Resolution"))
                    VerifyTargetResolution(target);
            }

            EditorGUILayout.Space();
            interactionId = EditorGUILayout.TextField("Interaction ID", interactionId);
            using (new EditorGUILayout.HorizontalScope())
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
            {
                if (GUILayout.Button("Focus Interaction"))
                    FocusInteraction();
                if (GUILayout.Button("Restore Focus"))
                    RestoreFocus();
            }

            EditorGUILayout.Space();
            bool canCapture = EditorApplication.isPlaying
                && resolutionReady
                && currentGameResolution == target;
            using (new EditorGUI.DisabledScope(!canCapture))
            {
                if (GUILayout.Button("Capture Exact PNG", GUILayout.Height(32f)))
                    CaptureSelectedState();
            }

            EditorGUILayout.LabelField(
                "Hover shortcut",
                "Ctrl+Alt+Shift+G (pointer stays in Game View)");
            EditorGUILayout.LabelField(
                "EventSystem submit",
                "Ctrl+Alt+Shift+F (live selected interaction)");
        }

        EditorGUILayout.HelpBox(status, MessageType.None);
    }

    private void OnDisable()
    {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;
        CancelResolutionVerification();
        CancelPendingCapture(false);
    }

    private void OnPlayModeStateChanged(PlayModeStateChange change)
    {
        if (change != PlayModeStateChange.ExitingPlayMode
            && change != PlayModeStateChange.EnteredEditMode)
        {
            return;
        }

        resolutionReady = false;
        currentGameResolution = default;
        previousSelection = null;
        CancelResolutionVerification();
        CancelPendingCapture(true);
    }

    private void VerifyTargetResolution(Vector2Int target)
    {
        requestedResolution = target;
        resolutionReady = false;
        currentGameResolution = default;
        CancelResolutionVerification();
        resolutionStablePlayFrames = 0;
        resolutionLastObservedFrame = -1;
        resolutionVerificationDeadline = EditorApplication.timeSinceStartup
            + CaptureTimeoutSeconds;
        resolutionVerificationPending = true;
        EditorApplication.update -= PollResolutionVerification;
        EditorApplication.update += PollResolutionVerification;
        status = $"Game View 고정 해상도 {FormatResolution(target)}가 서로 다른 3개 Play frame에서 안정적인지 확인합니다.";
        Repaint();
    }

    private void PollResolutionVerification()
    {
        if (!EditorApplication.isPlaying)
        {
            FinishResolutionVerification("Play Mode가 종료되어 해상도 검증을 취소했습니다.");
            return;
        }

        if (EditorApplication.isPaused)
        {
            resolutionStablePlayFrames = 0;
            resolutionLastObservedFrame = Time.frameCount;
            resolutionVerificationDeadline = EditorApplication.timeSinceStartup
                + CaptureTimeoutSeconds;
            status = "PAUSED — Play Mode를 재개하면 해상도 검증을 다시 시작합니다.";
            Repaint();
            return;
        }

        var actual = new Vector2Int(Screen.width, Screen.height);
        currentGameResolution = actual;
        resolutionStablePlayFrames = AdvanceStablePlayFrameCount(
            actual == requestedResolution,
            Time.frameCount,
            ref resolutionLastObservedFrame,
            resolutionStablePlayFrames);
        if (resolutionStablePlayFrames >= RequiredStablePlayFrames)
        {
            CancelResolutionVerification();
            OnResolutionReady(actual);
            return;
        }

        if (EditorApplication.timeSinceStartup >= resolutionVerificationDeadline)
        {
            FinishResolutionVerification(
                $"Game View 고정 해상도 확인 시간이 초과되었습니다. 실제 {FormatResolution(actual)}, 목표 {FormatResolution(requestedResolution)}. Game View preset을 먼저 선택하세요.");
            return;
        }

        Repaint();
    }

    private void FinishResolutionVerification(string message)
    {
        CancelResolutionVerification();
        OnResolutionError(message);
    }

    private void CancelResolutionVerification()
    {
        EditorApplication.update -= PollResolutionVerification;
        resolutionVerificationPending = false;
        resolutionStablePlayFrames = 0;
        resolutionLastObservedFrame = -1;
    }

    private void OnResolutionReady(Vector2Int actual)
    {
        currentGameResolution = actual;
        resolutionReady = actual == requestedResolution;
        status = resolutionReady
            ? $"READY {FormatResolution(actual)}. Game View 상태를 만든 뒤 캡처하세요."
            : $"FAIL Game Screen {FormatResolution(actual)}, 목표 {FormatResolution(requestedResolution)}.";
        Repaint();
    }

    private void OnResolutionError(string message)
    {
        resolutionReady = false;
        status = message;
        Debug.LogError(message);
        Repaint();
    }

    private void FocusInteraction()
    {
        EventSystem eventSystem = EventSystem.current;
        if (eventSystem == null)
        {
            status = "활성 EventSystem을 찾지 못했습니다.";
            return;
        }

        InteractionPointView target = FindObjectsByType<InteractionPointView>(
                FindObjectsInactive.Exclude,
                FindObjectsSortMode.None)
            .FirstOrDefault(view =>
                view.Definition != null
                && string.Equals(view.Definition.Id, interactionId, StringComparison.Ordinal));
        if (target == null)
        {
            status = $"활성 Interaction '{interactionId}'을 찾지 못했습니다.";
            return;
        }

        if (eventSystem.currentSelectedGameObject != target.gameObject)
            previousSelection = eventSystem.currentSelectedGameObject;
        eventSystem.SetSelectedGameObject(target.gameObject);
        bool focusApplied = eventSystem.currentSelectedGameObject == target.gameObject
            && target.Marker != null
            && target.Marker.gameObject.activeSelf
            && target.Tooltip != null
            && target.Tooltip.IsVisible;
        status = focusApplied
            ? $"PASS EventSystem focus: {interactionId}"
            : $"FAIL EventSystem focus presentation: {interactionId}";
        Repaint();
    }

    private void SubmitInteraction()
    {
        bool operationPending = resolutionVerificationPending
            || !string.IsNullOrEmpty(pendingTemporaryPath);
        if (!CanDispatchStateChangingCommand(
                EditorApplication.isPlaying,
                EditorApplication.isPaused,
                operationPending))
        {
            status = "EventSystem submit을 실행할 수 없습니다. "
                + "실행 중이고 일시 정지되지 않은 Play Mode에서 검증·캡처 작업이 없을 때 다시 시도하세요.";
            Repaint();
            return;
        }

        FocusInteraction();
        EventSystem eventSystem = EventSystem.current;
        GameObject selected = eventSystem == null
            ? null
            : eventSystem.currentSelectedGameObject;
        InteractionPointView target = selected == null
            ? null
            : selected.GetComponent<InteractionPointView>();
        if (target?.Definition == null
            || !string.Equals(target.Definition.Id, interactionId, StringComparison.Ordinal))
        {
            status = $"FAIL EventSystem submit target: {interactionId}";
            Repaint();
            return;
        }

        bool handled = ExecuteEvents.Execute(
            selected,
            new BaseEventData(eventSystem),
            ExecuteEvents.submitHandler);
        status = handled
            ? $"PASS EventSystem submit dispatched: {interactionId}. 결과 화면·효과를 별도로 확인하세요."
            : $"FAIL EventSystem submit handler: {interactionId}";
        Repaint();
    }

    private void RestoreFocus()
    {
        if (EventSystem.current != null)
        {
            GameObject restore = previousSelection != null
                && previousSelection.activeInHierarchy
                    ? previousSelection
                    : null;
            EventSystem.current.SetSelectedGameObject(restore);
        }
        previousSelection = null;
        status = "이전 EventSystem focus를 복원했습니다.";
        Repaint();
    }

    private void CaptureSelectedState()
    {
        Vector2Int target = Resolutions[resolutionIndex];
        if (!EditorApplication.isPlaying
            || !resolutionReady
            || currentGameResolution != target
            || !string.IsNullOrEmpty(pendingTemporaryPath))
        {
            status = "캡처할 수 없습니다. Play Mode, READY 해상도, pending 상태를 확인하세요.";
            Repaint();
            return;
        }

        string projectRoot = GetProjectRoot();
        QueueManagedPendingCaptureFiles(GetValidationRoot(projectRoot));
        string requestedPath = BuildOutputPath(
            projectRoot,
            session,
            scope,
            StateLabels[stateIndex],
            target.x,
            target.y);
        Directory.CreateDirectory(Path.GetDirectoryName(requestedPath)
            ?? throw new InvalidOperationException("Capture directory could not be resolved."));
        pendingFinalPath = GetAvailableOutputPath(requestedPath);
        pendingTemporaryPath = pendingFinalPath
            + $"{PendingCaptureMarker}{Guid.NewGuid():N}.pending.png";
        pendingResolution = target;
        pendingScreenResolution = default;
        pendingFileLength = -1;
        pendingFileWriteTimeUtc = default;
        pendingStableFilePolls = 0;
        pendingStablePlayFrames = 0;
        pendingLastObservedFrame = -1;
        captureIssued = false;
        pendingCaptureDeadline = EditorApplication.timeSinceStartup + CaptureTimeoutSeconds;
        status = $"캡처 직전 Game View 해상도와 레이아웃 안정성을 재검증하는 중: {pendingFinalPath}";
        EditorApplication.update -= PollCapture;
        EditorApplication.update += PollCapture;
        Repaint();
    }

    private void PollCapture()
    {
        if (!captureIssued)
        {
            if (!EditorApplication.isPlaying)
            {
                FinishCapture("Play Mode가 종료되어 pending 캡처를 취소했습니다.", true);
                return;
            }

            if (EditorApplication.isPaused)
            {
                pendingStablePlayFrames = 0;
                pendingLastObservedFrame = Time.frameCount;
                pendingCaptureDeadline = EditorApplication.timeSinceStartup
                    + CaptureTimeoutSeconds;
                status = "PAUSED — Play Mode를 재개하면 캡처 직전 검증을 다시 시작합니다.";
                Repaint();
                return;
            }

            if (EditorApplication.timeSinceStartup >= pendingCaptureDeadline)
            {
                FinishCapture($"PNG 기록 시간이 초과되었습니다: {pendingFinalPath}", true);
                return;
            }

            var actual = new Vector2Int(Screen.width, Screen.height);
            currentGameResolution = actual;
            pendingStablePlayFrames = AdvanceStablePlayFrameCount(
                actual == pendingResolution,
                Time.frameCount,
                ref pendingLastObservedFrame,
                pendingStablePlayFrames);
            if (pendingStablePlayFrames < RequiredStablePlayFrames)
            {
                Repaint();
                return;
            }

            pendingScreenResolution = actual;
            try
            {
                ScreenCapture.CaptureScreenshot(pendingTemporaryPath, 1);
            }
            catch (Exception exception)
            {
                FinishCapture($"Game frame 캡처 요청에 실패했습니다: {exception.Message}", true);
                return;
            }

            captureIssued = true;
            pendingCaptureDeadline = EditorApplication.timeSinceStartup
                + CaptureTimeoutSeconds;
            status = $"PNG 기록 대기 중: {pendingFinalPath}";
            Repaint();
            return;
        }


        if (EditorApplication.timeSinceStartup >= pendingCaptureDeadline)
        {
            FinishCapture($"PNG 기록 시간이 초과되었습니다: {pendingFinalPath}", true);
            return;
        }

        if (!File.Exists(pendingTemporaryPath))
            return;

        var info = new FileInfo(pendingTemporaryPath);
        if (info.Length == 0)
            return;

        if (info.Length != pendingFileLength
            || info.LastWriteTimeUtc != pendingFileWriteTimeUtc)
        {
            pendingFileLength = info.Length;
            pendingFileWriteTimeUtc = info.LastWriteTimeUtc;
            pendingStableFilePolls = 0;
            return;
        }

        pendingStableFilePolls++;
        if (pendingStableFilePolls < RequiredStableFilePolls)
            return;

        if (!TryReadPngDimensions(pendingTemporaryPath, out Vector2Int dimensions))
            return;

        Vector2Int expected = pendingResolution;
        bool matches = pendingScreenResolution == expected && dimensions == expected;
        string finalPath = pendingFinalPath;
        if (matches)
        {
            try
            {
                File.Move(pendingTemporaryPath, finalPath);
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException)
            {
                FinishCapture($"검증된 PNG를 최종 경로로 이동하지 못했습니다: {exception.Message}", true);
                return;
            }
        }
        FinishCapture(
            matches
                ? $"PASS {dimensions.x}×{dimensions.y}: {finalPath}"
                : $"FAIL 요청 Screen {pendingScreenResolution.x}×{pendingScreenResolution.y}, 실제 PNG {dimensions.x}×{dimensions.y}, 목표 {expected.x}×{expected.y}: {finalPath}",
            !matches);
    }

    private void FinishCapture(string message, bool isError)
    {
        EditorApplication.update -= PollCapture;
        if (isError)
            SchedulePendingCaptureCleanup(pendingTemporaryPath);
        pendingTemporaryPath = null;
        pendingFinalPath = null;
        captureIssued = false;
        pendingLastObservedFrame = -1;
        status = message;
        if (isError)
            Debug.LogError(message);
        else
            Debug.Log(message);
        Repaint();
    }

    private void CancelPendingCapture(bool report)
    {
        EditorApplication.update -= PollCapture;
        if (string.IsNullOrEmpty(pendingTemporaryPath))
            return;

        SchedulePendingCaptureCleanup(pendingTemporaryPath);
        pendingTemporaryPath = null;
        pendingFinalPath = null;
        captureIssued = false;
        pendingLastObservedFrame = -1;
        if (report)
            status = "Play Mode 종료로 pending 캡처를 취소했습니다.";
    }

    private static void SchedulePendingCaptureCleanup(string path)
    {
        string validationRoot = GetValidationRoot();
        if (!IsManagedPendingCapturePath(validationRoot, path))
        {
            if (!string.IsNullOrEmpty(path))
                Debug.LogWarning($"Refused unmanaged pending capture cleanup: {path}");
            return;
        }

        PendingCaptureCleanupStates[path] = new PendingCaptureCleanupState
        {
            deadline = EditorApplication.timeSinceStartup + PendingCleanupTimeoutSeconds,
        };
        PersistPendingCaptureCleanupPaths();
        EditorApplication.update -= PollPendingCaptureCleanup;
        EditorApplication.update += PollPendingCaptureCleanup;
    }

    private static void PollPendingCaptureCleanup()
    {
        double now = EditorApplication.timeSinceStartup;
        bool changed = false;
        foreach (KeyValuePair<string, PendingCaptureCleanupState> pending
                 in PendingCaptureCleanupStates.ToArray())
        {
            if (now >= pending.Value.deadline)
            {
                bool deleted = TryDeletePendingFile(pending.Key, false);
                if (!deleted)
                {
                    Debug.LogWarning(
                        $"Pending capture cleanup expired after "
                        + $"{PendingCleanupTimeoutSeconds:0} seconds; leaving the file in place: "
                        + pending.Key);
                }

                PendingCaptureCleanupStates.Remove(pending.Key);
                changed = true;
                continue;
            }

            if (!File.Exists(pending.Key))
            {
                pending.Value.fileLength = -1;
                pending.Value.fileWriteTimeUtc = default;
                pending.Value.stableFilePolls = 0;
                pending.Value.deleteAttempted = false;
                continue;
            }

            FileInfo info;
            try
            {
                info = new FileInfo(pending.Key);
                if (info.Length == 0
                    || info.Length != pending.Value.fileLength
                    || info.LastWriteTimeUtc != pending.Value.fileWriteTimeUtc)
                {
                    pending.Value.fileLength = info.Length;
                    pending.Value.fileWriteTimeUtc = info.LastWriteTimeUtc;
                    pending.Value.stableFilePolls = 0;
                    pending.Value.deleteAttempted = false;
                    continue;
                }
            }
            catch (Exception exception) when (exception is IOException
                or UnauthorizedAccessException)
            {
                continue;
            }

            pending.Value.stableFilePolls++;
            if (pending.Value.stableFilePolls < RequiredStableFilePolls
                || pending.Value.deleteAttempted)
                continue;

            pending.Value.deleteAttempted = true;
            if (TryDeletePendingFile(pending.Key, false))
            {
                PendingCaptureCleanupStates.Remove(pending.Key);
                changed = true;
            }
        }

        if (changed)
            PersistPendingCaptureCleanupPaths();
        if (PendingCaptureCleanupStates.Count == 0)
            EditorApplication.update -= PollPendingCaptureCleanup;
    }

    private static int QueueManagedPendingCaptureFiles(string validationRoot)
    {
        if (string.IsNullOrWhiteSpace(validationRoot) || !Directory.Exists(validationRoot))
            return 0;

        int queued = 0;
        try
        {
            foreach (string directory in Directory.EnumerateDirectories(
                         validationRoot,
                         "*",
                         SearchOption.TopDirectoryOnly))
            {
                FileAttributes attributes;
                try
                {
                    attributes = File.GetAttributes(directory);
                }
                catch (Exception exception) when (exception is IOException
                    or UnauthorizedAccessException)
                {
                    Debug.LogWarning(
                        $"Pending capture directory inspection failed: {exception.Message}");
                    continue;
                }

                if ((attributes & FileAttributes.ReparsePoint) != 0)
                    continue;

                try
                {
                    foreach (string path in Directory.EnumerateFiles(
                                 directory,
                                 "*.pending.png",
                                 SearchOption.TopDirectoryOnly))
                    {
                        if (!IsManagedPendingCapturePath(validationRoot, path))
                            continue;
                        SchedulePendingCaptureCleanup(path);
                        queued++;
                    }
                }
                catch (Exception exception) when (exception is IOException
                    or UnauthorizedAccessException)
                {
                    Debug.LogWarning(
                        $"Pending capture directory scan failed: {exception.Message}");
                }
            }
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException)
        {
            Debug.LogWarning($"Pending capture scan failed: {exception.Message}");
        }

        return queued;
    }

    private static bool TryDeletePendingFile(string path, bool reportFailure = true)
    {
        try
        {
            if (!File.Exists(path))
                return true;
            using (File.Open(path, FileMode.Open, FileAccess.ReadWrite, FileShare.None))
            {
            }
            File.Delete(path);
            return true;
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException)
        {
            if (reportFailure)
                Debug.LogWarning($"Pending capture cleanup failed: {exception.Message}");
            return false;
        }
    }

    private static void PersistPendingCaptureCleanupPaths()
    {
        SessionState.SetString(
            PendingCaptureSessionKey,
            string.Join("\n", PendingCaptureCleanupStates.Keys));
    }

    public static bool IsManagedPendingCapturePath(string validationRoot, string path)
    {
        if (string.IsNullOrWhiteSpace(validationRoot) || string.IsNullOrWhiteSpace(path))
            return false;

        try
        {
            string root = Path.GetFullPath(validationRoot)
                .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
                + Path.DirectorySeparatorChar;
            string candidate = Path.GetFullPath(path);
            if (!candidate.StartsWith(root, StringComparison.OrdinalIgnoreCase))
                return false;

            string candidateDirectory = Path.GetDirectoryName(candidate);
            if (string.IsNullOrEmpty(candidateDirectory))
                return false;
            string candidateParent = Path.GetDirectoryName(candidateDirectory);
            string normalizedRoot = root.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);
            if (!string.Equals(
                    candidateParent,
                    normalizedRoot,
                    StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!Directory.Exists(candidateDirectory)
                || (File.GetAttributes(candidateDirectory) & FileAttributes.ReparsePoint) != 0)
            {
                return false;
            }

            const string pendingSuffix = ".pending.png";
            string fileName = Path.GetFileName(candidate);
            if (!fileName.EndsWith(pendingSuffix, StringComparison.OrdinalIgnoreCase))
                return false;
            string withoutSuffix = fileName[..^pendingSuffix.Length];
            int markerIndex = withoutSuffix.LastIndexOf(
                PendingCaptureMarker,
                StringComparison.Ordinal);
            if (markerIndex <= 0)
                return false;
            string token = withoutSuffix[(markerIndex + PendingCaptureMarker.Length)..];
            return Guid.TryParseExact(token, "N", out _);
        }
        catch (Exception exception) when (exception is ArgumentException
            or IOException
            or NotSupportedException
            or UnauthorizedAccessException)
        {
            return false;
        }
    }

    public static int AdvanceStablePlayFrameCount(
        bool matchesTarget,
        int currentFrame,
        ref int lastObservedFrame,
        int stableFrameCount)
    {
        if (currentFrame == lastObservedFrame)
            return matchesTarget ? stableFrameCount : 0;

        if (currentFrame < lastObservedFrame)
        {
            lastObservedFrame = currentFrame;
            return 0;
        }

        lastObservedFrame = currentFrame;
        if (!matchesTarget)
            return 0;
        return stableFrameCount + 1;
    }

    public static bool CanDispatchStateChangingCommand(
        bool isPlaying,
        bool isPaused,
        bool operationPending) => isPlaying && !isPaused && !operationPending;

    public static string BuildOutputPath(
        string projectRoot,
        string session,
        string scope,
        string state,
        int width,
        int height)
    {
        if (string.IsNullOrWhiteSpace(projectRoot))
            throw new ArgumentException("Project root is required.", nameof(projectRoot));
        if (width <= 0 || height <= 0)
            throw new ArgumentOutOfRangeException(nameof(width), "Capture dimensions must be positive.");

        string safeSession = SanitizePathSegment(session, "session");
        string safeScope = SanitizePathSegment(scope, "capture");
        string safeState = SanitizePathSegment(state, "state");
        return Path.Combine(
            projectRoot,
            "Logs",
            "Validation",
            $"{safeSession}_{safeScope}",
            $"{width}x{height}_{safeScope}_{safeState}.png");
    }

    public static string SanitizePathSegment(string value, string fallback)
    {
        string source = string.IsNullOrWhiteSpace(value) ? fallback : value.Trim();
        string sanitized = new(source
            .Select(character => char.IsLetterOrDigit(character)
                    || character == '-'
                    || character == '_'
                ? character
                : '-')
            .ToArray());
        while (sanitized.Contains("--", StringComparison.Ordinal))
            sanitized = sanitized.Replace("--", "-", StringComparison.Ordinal);
        sanitized = sanitized.Trim('-', '.');
        if (string.IsNullOrEmpty(sanitized))
            sanitized = fallback;
        return sanitized.Length <= 64 ? sanitized : sanitized[..64].TrimEnd('-', '_');
    }

    public static string GetAvailableOutputPath(string requestedPath)
    {
        if (!File.Exists(requestedPath))
            return requestedPath;

        string directory = Path.GetDirectoryName(requestedPath)
            ?? throw new ArgumentException("Capture path must include a directory.", nameof(requestedPath));
        string name = Path.GetFileNameWithoutExtension(requestedPath);
        string extension = Path.GetExtension(requestedPath);
        for (int sequence = 2; sequence <= 999; sequence++)
        {
            string candidate = Path.Combine(directory, $"{name}-{sequence:00}{extension}");
            if (!File.Exists(candidate))
                return candidate;
        }

        throw new IOException($"No available capture sequence for '{requestedPath}'.");
    }

    private static bool TryReadPngDimensions(string path, out Vector2Int dimensions)
    {
        dimensions = default;
        try
        {
            byte[] bytes = File.ReadAllBytes(path);
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            try
            {
                if (!ImageConversion.LoadImage(texture, bytes, false))
                    return false;
                dimensions = new Vector2Int(texture.width, texture.height);
                return true;
            }
            finally
            {
                DestroyImmediate(texture);
            }
        }
        catch (Exception exception) when (exception is IOException
            or UnauthorizedAccessException
            or ArgumentException)
        {
            return false;
        }
    }

    private static string FormatResolution(Vector2Int resolution) =>
        $"{resolution.x}×{resolution.y}";

    private static string GetProjectRoot() =>
        Directory.GetParent(Application.dataPath)?.FullName
        ?? throw new InvalidOperationException("Project root could not be resolved.");

    private static string GetValidationRoot() => GetValidationRoot(GetProjectRoot());

    private static string GetValidationRoot(string projectRoot) =>
        Path.Combine(projectRoot, "Logs", "Validation");
}
