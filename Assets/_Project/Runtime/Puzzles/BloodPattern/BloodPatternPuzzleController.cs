using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class BloodPatternPuzzleController : ValidatedPuzzleController
{
    private readonly int[] pieces = new int[9];
    private readonly int[] rotations = new int[9];
    private readonly HashSet<string> observations = new();

    protected override void ResetPuzzle()
    {
        int[] order = { 7, 2, 5, 1, 8, 0, 6, 3, 4 };
        Array.Copy(order, pieces, 9);
        Array.Clear(rotations, 0, 9);
        observations.Clear();
    }

    public bool SelectObservation(string id)
    {
        string[] allowed = Context.Definition?.Rules?.AllowedInputIds;
        if (string.IsNullOrWhiteSpace(id) || allowed == null || Array.IndexOf(allowed, id) < 0)
            return false;
        return observations.Add(id);
    }

    public bool SubmitAuthoredRule()
    {
        string[] solution = Context.Definition?.Rules?.SolutionIds;
        if (solution == null || solution.Length == 0 || !HasRequiredEvidence())
            return false;
        foreach (string id in solution)
            if (!observations.Contains(id))
                return false;
        return CompleteWhen(true, string.Join(",", solution));
    }

    public void Swap(int a, int b)
    {
        if (a < 0 || a >= 9 || b < 0 || b >= 9)
            return;
        (pieces[a], pieces[b]) = (pieces[b], pieces[a]);
    }

    public void Rotate(int slot)
    {
        if (slot >= 0 && slot < 9)
            rotations[slot] = (rotations[slot] + 1) % 4;
    }

    public bool Submit()
    {
        for (int i = 0; i < 9; i++)
            if (pieces[i] != i || rotations[i] != 0)
                return false;
        return CompleteWhen(true, "reconstructed");
    }
}
