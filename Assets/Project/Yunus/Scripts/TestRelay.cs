using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Multiplayer;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;

public class TestRelay : MonoBehaviour
{
    public static TestRelay Instance { get; private set; }

    //[Header("UI References")]
    //public Button createRelayButton;
    //public Button joinRelayButton;
    //public TMP_InputField joinCodeInput;

    private void Awake()
    {
        Instance = this;
    }
    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };
        //await AuthenticationService.Instance.SignInAnonymouslyAsync();

        //createRelayButton.onClick.AddListener(() => { _ = CreateRelay(); });
        //joinRelayButton.onClick.AddListener(() => { _ = JoinRelay(joinCodeInput.text); });
    }

    public async Task<string> CreateRelay()
    {
        try
        {
            var allocation = await Relay.Instance.CreateAllocationAsync(3);

            string joinCode = await Relay.Instance.GetJoinCodeAsync(allocation.AllocationId);
            Debug.Log("JoinCode "+joinCode);

            RelayServerData relayServerData=new RelayServerData(allocation,"dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartHost();

            return joinCode;
        }
        catch (System.Exception e)
        {
            Debug.Log(e);
            return null;
        }
    }
    public async Task JoinRelay(string joinedCode)
    {
        try
        {
            Debug.Log("Joined Relay With " + joinedCode);
            var joinAllocation= await Relay.Instance.JoinAllocationAsync(joinedCode);

            RelayServerData relayServerData = new RelayServerData(joinAllocation, "dtls");

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(relayServerData);

            NetworkManager.Singleton.StartClient();
        }
        catch (System.Exception e)
        {

            Debug.Log(e);
        }
    }
}
