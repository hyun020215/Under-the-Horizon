using UnityEngine;

public static class DialogueTypography
{
    public static int ResolveFontSize(int baseline, int characterCount, bool narration)
    {
        int safeBaseline = Mathf.Max(12, baseline);
        int narrationAdjustment = narration ? -1 : 0;
        if (characterCount >= 180)
            return Mathf.Max(18, safeBaseline - 5 + narrationAdjustment);
        if (characterCount >= 100)
            return Mathf.Max(20, safeBaseline - 3 + narrationAdjustment);
        if (characterCount <= 45)
            return safeBaseline + 1 + narrationAdjustment;
        return safeBaseline + narrationAdjustment;
    }
}
