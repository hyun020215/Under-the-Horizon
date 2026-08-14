using UnityEditor;
using UnityEditor.Build;
using UnityEngine;

public static class BuildPreflightValidator
{
    [MenuItem("Under The Horizon/Validate/Build Preflight")]
    public static void Run()
    {
        var errors = ContentValidator.ValidateAll();
        errors.AddRange(ProjectIdentityValidator.ValidateAll());
        if (errors.Count > 0)
            throw new BuildFailedException(string.Join("\n", errors));
        Debug.Log("Under the Horizon build preflight passed.");
    }
}
