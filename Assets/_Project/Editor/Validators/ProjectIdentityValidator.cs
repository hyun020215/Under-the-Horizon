using System;
using System.Collections.Generic;
using UnityEditor;

public static class ProjectIdentityValidator
{
    public const string CanonicalProductName = "Under the Horizon";
    public const string CanonicalWsaPackageName = "UnderTheHorizon";

    public static List<string> ValidateAll()
    {
        var errors = new List<string>();
        RequireExactValue(
            "PlayerSettings.productName",
            PlayerSettings.productName,
            CanonicalProductName,
            errors);
        RequireExactValue(
            "PlayerSettings.WSA.packageName",
            PlayerSettings.WSA.packageName,
            CanonicalWsaPackageName,
            errors);
        RequireExactValue(
            "PlayerSettings.WSA.applicationDescription",
            PlayerSettings.WSA.applicationDescription,
            CanonicalProductName,
            errors);
        return errors;
    }

    private static void RequireExactValue(
        string field,
        string actual,
        string expected,
        ICollection<string> errors)
    {
        if (string.Equals(actual, expected, StringComparison.Ordinal))
            return;

        errors.Add(
            $"{field} must be '{expected}', but was '{actual}'.");
    }
}
