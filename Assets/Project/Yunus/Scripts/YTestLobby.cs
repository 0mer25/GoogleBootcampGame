using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Lobbies;
using Unity.Services.Multiplayer;
using Unity.Services.Lobbies.Models;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class YTestLobby : MonoBehaviour
{

    private Lobby hostLobby;
    private float heartbeatTimer;
    public InputField lobbyCodeInput;
    private string playerName;

    private async void Start()
    {
        await UnityServices.InitializeAsync();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
        playerName = "Bootcamp" + UnityEngine.Random.Range(10, 99);
        Debug.Log(playerName);
    }
    private void Update()
    {
        HandleLobbyHeartbeat();
    }
    private async void HandleLobbyHeartbeat()
    {
        if (hostLobby != null)
        {
            heartbeatTimer -= Time.deltaTime;
            if (heartbeatTimer < 0f)
            {
                float heartbeatTimerMax = 15f;
                heartbeatTimer = heartbeatTimerMax;

                try
                {
                    await LobbyService.Instance.SendHeartbeatPingAsync(hostLobby.Id);
                    Debug.Log("Heartbeat sent successfully");
                }
                catch (LobbyServiceException e)
                {
                    Debug.Log("Heartbeat failed: " + e);
                }
            }
        }
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = "MyLobby";
            int maxPlayer = 2;
            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                IsPrivate = false,
                Player = GetPlayer(),
                Data = new Dictionary<string, DataObject>
                {
                    {"GameMode",new DataObject(DataObject.VisibilityOptions.Public,"CaptureTheFlag") }
                }
            };
            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayer, createLobbyOptions);

            hostLobby = lobby;
            heartbeatTimer = 15f;


            Debug.Log("Created Lobby!" + lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Id + " " + lobby.LobbyCode);

            PrintPlayers(hostLobby);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }


    }

    public async void ListLobbies()
    {
        try
        {
            QueryLobbiesOptions queryLobbiesOptions = new QueryLobbiesOptions()
            {
                Count = 25,
                Filters = new List<QueryFilter>
                {
                    new QueryFilter(QueryFilter.FieldOptions.AvailableSlots, "0", QueryFilter.OpOptions.GT)
                },
                Order = new List<QueryOrder>
                {
                    new QueryOrder(false, QueryOrder.FieldOptions.Created)
                }
            };


            QueryResponse queryResponse = await LobbyService.Instance.QueryLobbiesAsync();

            Debug.Log("Lobbies found:" + queryResponse.Results.Count);
            foreach (Lobby lobby in queryResponse.Results)
            {
                Debug.Log(lobby.Name + " " + lobby.MaxPlayers + " " + lobby.Data["GameMode"].Value);
            }
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }
    public async void JoinLobbyByCode()
    {
        string lobbyCode = lobbyCodeInput.text;
        if (string.IsNullOrEmpty(lobbyCode))
        {
            Debug.LogWarning("Lobby code is empty!");
            return;
        }

        try
        {
            JoinLobbyByCodeOptions joinLobbyByCodeOptions = new JoinLobbyByCodeOptions
            {
                Player = GetPlayer()
            };
            Lobby joinLobby = await LobbyService.Instance.JoinLobbyByCodeAsync(lobbyCode, joinLobbyByCodeOptions);
            Debug.Log("Joined Lobby with code " + lobbyCode);

            PrintPlayers(joinLobby);
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }
    }

    public async void QuickJoinLobby()
    {
        try
        {
            await LobbyService.Instance.QuickJoinLobbyAsync();
            Debug.Log("Joined Lobby with Quick Lobby");
        }
        catch (LobbyServiceException e)
        {

            Debug.Log(e);
        }

    }

    private Player GetPlayer()
    {
        return new Player
        {
            Data = new Dictionary<string, PlayerDataObject>
                    {
                        {"PlayerName",new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, playerName) }
                    }
        };
    }
    private void PrintPlayers(Lobby lobby)
    {
        Debug.Log("Player in Lobby " + lobby.Name + " " + lobby.Data["GameMode"].Value);

        foreach (Player player in lobby.Players)
        {
            Debug.Log(player.Id + " " + player.Data["PlayerName"].Value);

        }
    }
}
