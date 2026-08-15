using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;

public sealed class P01InvitationNarrativeContractTests
{
    private const string DialogueSourcePath =
        "Assets/_Project/Content/Dialogue/Source/Dialogue_Master_KR.csv";

    [Test]
    public void InvitationSourceExplainsTheBenchAndPreservesTheClueContract()
    {
        string[] rows = File.ReadAllLines(DialogueSourcePath, Encoding.UTF8);
        string opening = FindRow(rows, "P-01_002,");
        string inspectionTitle = FindRow(rows, "P-01_003,");
        string inspection = FindRow(rows, "P-01_004,");

        Assert.That(
            opening,
            Is.EqualTo(
                "P-01_002,P-01,2,opening,narration,NARRATION,\""
                + "다니엘 머서는 탑승 줄에서 한 걸음 떨어져 있었다. "
                + "청록색 옷깃은 구겨져 있었다. 주머니가 진동하자 그는 손에 쥔 초대장을 "
                + "벤치 위에 내려놓고 오른손을 주머니로 가져갔다.\",observe,,,,PORT,N,,"),
            "The non-VO opening must establish why Daniel's invitation is on the bench.");
        Assert.That(
            inspectionTitle,
            Is.EqualTo(
                "P-01_003,P-01,3,inspection,narration,NARRATION,"
                + "[조사: 벤치 위 구겨진 초대장],observe,,,,PORT,N,,"));
        Assert.That(
            inspection,
            Is.EqualTo(
                "P-01_004,P-01,4,inspection,inspection,ADRIAN,"
                + "앞면에는 수신인 ‘DANIEL MERCER’가 선명하다. "
                + "종이는 두 번 접혔다가 급히 펴졌다. 리처드의 전자서명은 선명하지만 "
                + "발송 코드는 초대장 본문과 다른 서체다.,focused,,,,PORT,Y,,"),
            "The voiced inspection must identify Daniel without removing the signature/code clue.");

        Assert.That(System.Array.IndexOf(rows, opening) + 1, Is.EqualTo(System.Array.IndexOf(rows, inspectionTitle)));
        Assert.That(System.Array.IndexOf(rows, inspectionTitle) + 1, Is.EqualTo(System.Array.IndexOf(rows, inspection)));
    }

    private static string FindRow(string[] rows, string prefix)
    {
        string[] matches = rows.Where(row => row.StartsWith(prefix)).ToArray();
        Assert.That(matches, Has.Length.EqualTo(1), prefix);
        return matches[0];
    }
}
