using Cysharp.Threading.Tasks;
using EditorAttributes;
using Facility.Generation;
using UnityEngine;

public class IntroSequence : MonoBehaviour
{
    [Space]
    [ReadOnly] public bool IntroTriggered;

    [Space]
    [SerializeField] private RoomInstance introStartRoom;
    [SerializeField] private RoomInstance startRoom;
    [SerializeField] private WakeUpCutscene wakeUpCutscene;
    [SerializeField] private DocumentBehavior introDocument;

    [Header("Timers")]
    [SerializeField, Range(0, 40)] private float delayAfterWakeUp = 1f;
    [SerializeField, Range(0, 40)] private float delayAfterReadDocument = 4f;
    [SerializeField, Range(0, 40)] private float delayBeforeCellDoorOpens = 11.3f;
    [SerializeField, Range(0, 40)] private float delayAfterDoorOpensToSpeak = 0.7f;
    [SerializeField, Range(0, 40)] private float delayAfterCellExitToWalk = 9f;
    [SerializeField, Range(0, 40)] private float minTimeToExit = 18f;
    [SerializeField, Range(0, 40)] private float maxTimeToExit = 26f;
    [SerializeField, Range(0, 40)] private float delayBeforeCellDoorClosesOnKill = 3.8f;
    [SerializeField, Range(0, 40)] private float delayBeforeGasEmitted = 4.2f;

    [Header("NPCs")]
    [SerializeField] private NPC_Guard agentUlgrin;
    [SerializeField] private Transform[] guardWalkPath;

    [Header("Cull Triggers")]
    [SerializeField] private IntroSectionTrigger[] introSectionTriggers;
    [SerializeField] private IntroSectionTrigger cell311Triggers;
    [SerializeField] private IntroSectionTrigger cellBlockTriggers;
    [SerializeField] private Collider cellBlockCellCollider;

    [Header("Event Triggers")]
    [SerializeField] private IntroEventTrigger cellExitTrigger;
    [SerializeField] private IntroEventTrigger startConvoTrigger;

    [Header("Gas Particles")]
    [SerializeField] private GameObject gasParticleSources;
    [SerializeField] private GameObject gasHissSound;
    [SerializeField] private Color32 inGasFogColor;
    [SerializeField] private float inGasFogDensity;

    [Header("Important Doors")]
    [SerializeField] private BaseDoorController cellExitDoor;
    [SerializeField] private BaseDoorController cellBlockExitDoor;
    [SerializeField] private BaseDoorController junctionExitDoor;
    [SerializeField] private BaseDoorController transportExitDoor;
    [SerializeField] private BaseDoorController officeExitDoor;
    [SerializeField] private BaseDoorController queueExitDoor;
    [SerializeField] private BaseDoorController peanutBootyDoor;

    private bool _wokeUp = false;
    private bool _documentUsed = false;
    private bool _guardIsInNorthLiberty = false;
    private bool _exitedCell = false;
    private bool _escortingPlayer = false;

    private void OnEnable()
    {
        DocumentBehavior.OnDocumentUsed += OnDocumentUsed;

        // Event triggers
        cellExitTrigger.OnPlayerEntered += OnPlayerExitedCell;
        startConvoTrigger.OnPlayerEntered += OnGuardsGetAwkward;
    }

    private void OnDisable()
    {
        DocumentBehavior.OnDocumentUsed -= OnDocumentUsed;

        // Event triggers
        cellExitTrigger.OnPlayerEntered -= OnPlayerExitedCell;
        startConvoTrigger.OnPlayerEntered -= OnGuardsGetAwkward;
    }

    [ContextMenu("Start Intro now!!!!")]
    public void StartIntro()
    {
        if (IntroTriggered) return;

        _documentUsed = false;

        Core.MusicManager.PlayMusic(Core.AudioDataAccess.Music.IntroMusic);

        ToggleCellBlockCulling(false);
        cell311Triggers.SetLights(true);
        cellExitDoor.CloseDoorImmediate();

        StartIntroAsync().Forget();
    }

    private async UniTaskVoid StartIntroAsync()
    {
        await wakeUpCutscene.PlayCutscene(introSectionTriggers);
        _wokeUp = true;
        await UniTask.WaitForSeconds(delayAfterWakeUp);

        // Wait for document to be read
    }

    public async UniTaskVoid ContinueIntroAfterReadAsync()
    {
        await UniTask.WaitForSeconds(delayAfterReadDocument);
        FMODHelper.PlayInstanceWithSubtitles(Core.AudioDataAccess.Characters.Ulgrin.BeforeCellOpen, agentUlgrin.VoiceEmitter);
        await UniTask.WaitForSeconds(1f);
        ToggleCellBlockCulling(true);
        await UniTask.WaitForSeconds(delayBeforeCellDoorOpens);
        cellExitDoor.OpenDoor();
        await UniTask.WaitForSeconds(delayAfterDoorOpensToSpeak);
        FMODHelper.PlayInstanceWithSubtitles(Core.AudioDataAccess.Characters.Ulgrin.ExitCellRequest, agentUlgrin.VoiceEmitter);

        StartCellExitTimer().Forget();
    }

    private async UniTaskVoid StartCellExitTimer()
    {
        float totalTime = Random.Range(minTimeToExit, maxTimeToExit);
        float warningTime = totalTime * 0.5f;

        await UniTask.WaitForSeconds(warningTime);

        if (!_exitedCell)
        {
            FMODHelper.PlayInstanceWithSubtitles(FMODHelper.PickRandomEvent(
                Core.AudioDataAccess.Characters.Ulgrin.ExitCellRefuseA,
                Core.AudioDataAccess.Characters.Ulgrin.ExitCellRefuseB), agentUlgrin.VoiceEmitter
            );

            await UniTask.WaitForSeconds(6f);
            await UniTask.WaitForSeconds(totalTime - warningTime);

            if (!_exitedCell)
            {
                Log.VerboseInfo("what the fuck are you doing in North Liberty");
                KillExitCellRefused().Forget();
                Log.VerboseInfo("I'm fucking pissed");
            }
        }
    }

    public async UniTaskVoid ContinueIntroAfterCellExitAsync()
    {
        await UniTask.WaitForSeconds(delayAfterCellExitToWalk);

        if (agentUlgrin != null)
        {
            _escortingPlayer = true;
            GuardWalkSequenceAsync().Forget();
        }
    }

    public void EndIntro()
    {

    }

    private void ToggleCellBlockCulling(bool enabled)
    {
        cellBlockCellCollider.enabled = enabled;
        cellBlockTriggers.enabled = enabled;
        cellBlockTriggers.SetLights(enabled);
    }

    private void OnDocumentUsed(DocumentBehavior document)
    {
        if (_wokeUp)
        {
            if (document == introDocument && !_documentUsed)
            {
                _documentUsed = true;
                ContinueIntroAfterReadAsync().Forget();
            }
        }
    }

    private async UniTask KillExitCellRefused()
    {
        _guardIsInNorthLiberty = true;

        FMODHelper.PlayInstanceWithSubtitles(FMODHelper.PickRandomEvent(
            Core.AudioDataAccess.Characters.Ulgrin.ExitCellKillA,
            Core.AudioDataAccess.Characters.Ulgrin.ExitCellKillB), agentUlgrin.VoiceEmitter,
            useOcclusion: true
        );

        await UniTask.WaitForSeconds(delayBeforeCellDoorClosesOnKill);

        cellExitDoor.CloseDoor();
        await UniTask.WaitForSeconds(delayBeforeGasEmitted);
        gasParticleSources.SetActive(true);
        await UniTask.WaitForSeconds(0.3f);
        gasHissSound.SetActive(true);
        await UniTask.WaitForSeconds(0.4f);
        Core.FacilityManager.SetFogColor(inGasFogColor, 0f);
        Core.FacilityManager.SetFogEnabled(true);
        Core.FacilityManager.SetFogDensity(inGasFogDensity, 7f);
        await UniTask.WaitForSeconds(6.3f);
        gasParticleSources.SetActive(false);
    }

    private void OnPlayerExitedCell()
    {
        if (_documentUsed && !_guardIsInNorthLiberty)
        {
            _exitedCell = true;

            agentUlgrin.LookAtPlayer(true);
            FMODHelper.PlayInstanceWithSubtitles(FMODHelper.PickRandomEvent(
                Core.AudioDataAccess.Characters.Ulgrin.EscortStartA,
                Core.AudioDataAccess.Characters.Ulgrin.EscortStartB), agentUlgrin.VoiceEmitter
            );
            ContinueIntroAfterCellExitAsync().Forget();
        }
    }

    private void OnGuardsGetAwkward()
    {
        FMODHelper.PlayInstanceWithSubtitles(FMODHelper.PickRandomEvent(
            Core.AudioDataAccess.Characters.Ulgrin.EscortConvoA,
            Core.AudioDataAccess.Characters.Ulgrin.EscortConvoB,
            Core.AudioDataAccess.Characters.Ulgrin.EscortConvoC,
            Core.AudioDataAccess.Characters.Ulgrin.EscortConvoD,
            Core.AudioDataAccess.Characters.Ulgrin.EscortConvoE), agentUlgrin.VoiceEmitter
        );
    }

    private async UniTaskVoid GuardWalkSequenceAsync()
    {
        if (agentUlgrin == null || guardWalkPath == null || guardWalkPath.Length == 0) return;

        bool success = await agentUlgrin.FollowWaypoints(guardWalkPath, waitTimeAtWaypoint: 0f);
        agentUlgrin.StopMoving();

        _escortingPlayer = false;
    }
}