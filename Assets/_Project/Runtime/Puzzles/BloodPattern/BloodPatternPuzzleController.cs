using System;
using UnityEngine;

public sealed class BloodPatternPuzzleController : ValidatedPuzzleController
{
    private readonly int[] pieces = new int[9];
    private readonly int[] rotations = new int[9];

    protected override void ResetPuzzle()
    {
        int[] order = { 7, 2, 5, 1, 8, 0, 6, 3, 4 };
        Array.Copy(order, pieces, 9);
        Array.Clear(rotations, 0, 9);
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
